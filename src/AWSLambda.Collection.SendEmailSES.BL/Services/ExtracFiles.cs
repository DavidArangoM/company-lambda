using Amazon.Lambda.Core;
using Amazon.S3;
using AWSLambda.Collection.SendEmailSES.BL.DO;
using AWSLambda.Collection.SendEmailSES.BL.Interface;
using AWSLambda.Collection.SendEmailSES.BL.Utils;
using AWSLambda.Collection.SendEmailSES.Infra.Cache;
using AWSLambda.Collection.SendEmailSES.Infra.Enviroment;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambda.Collection.SendEmailSES.BL.Services
{
    public class ExtracFiles : IExtracFiles, IDisposable
    {
        #region propiedades privadas 
        private static ExtracFiles? instance;
        private readonly ILambdaContext _logger;
        private readonly ICacheProvider _cacheProvider;
        private DistributedCacheEntryOptions _cacheConfig = new DistributedCacheEntryOptions
        { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) };


        #endregion

        #region singleton
        public ExtracFiles(ILambdaContext logger)
        {
            _logger = logger;
        }

        public ExtracFiles() { }


        public static ExtracFiles Getinstance(ILambdaContext logger)
        {

            if (instance == null)
            {
                instance = new ExtracFiles(logger);
            }
            return instance;
        }
        private IExtracData ReadData
        {
            get { return ExtracData.Getinstance(_logger); }
        }
        private IS3Files s3Files
        {
            get { return S3Files.Getinstance(_logger); }
        }

        private SendEmail sendmail
        {
            get { return SendEmail.Getinstance(_logger); }
        }
        #endregion
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// si el archiuvo que llega no es un zip llama directo a leer el archivo
        /// </summary>
        /// <param name="buscket"></param>
        /// <param name="name"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<ResponseTransaction> ReadTextFile(IAmazonS3 S3Client, string buscket, string name, ILambdaContext context)
        {
            S3Client = new AmazonS3Client();

            context.Logger.LogInformation("Bucket "+EnvironmentVariables.BucketS3 +" Key " + EnvironmentVariables.FolderBase + "/" + EnvironmentVariables.FolderEmail + "/" + name);
            using var response = await S3Client.GetObjectAsync(EnvironmentVariables.BucketS3, EnvironmentVariables.FolderBase + "/" + EnvironmentVariables.FolderEmail + "/" + name);
            if (response.HttpStatusCode == HttpStatusCode.OK)
            {
                context.Logger.LogInformation("Se obtuvo el archivo correctamente " + name);
                var finalResult = await ReadData.ReadFile(S3Client, new StreamReader(response.ResponseStream), name);
                if (finalResult != null)
                    return new ResponseTransaction() { CodMessage = finalResult.CodMessage, Message = finalResult.Message };
                else
                    return new ResponseTransaction() { CodMessage = finalResult.CodMessage, Message = finalResult.Message };
            }
            else
            {
                return new ResponseTransaction() { CodMessage = "500", Message = "No fue posible obtener los archivos del bucket s3." };
            }
        }


        /// <summary>
        /// hace unzip para dejar los archivos en una carpeta de adjuntos
        /// </summary>
        /// <param name="buscket"></param>
        /// <param name="name"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<ResponseTransaction> UnZipToMemory(IAmazonS3 S3Client,string buscket, string name, ILambdaContext context)
        {

            string namefile = "";
            S3Client = new AmazonS3Client();
            using var response = await S3Client.GetObjectAsync(EnvironmentVariables.BucketS3, EnvironmentVariables.FolderBase + "/" + EnvironmentVariables.FolderEmail + "/" + name);
            using var zip = new ZipArchive(response.ResponseStream, ZipArchiveMode.Read);
            string folder = Guid.NewGuid().ToString();
            if (zip.Entries.Where(x => x.Name.Contains(".txt")).Any())
            {
                
                int succestry = 0;
                #region unzip 
                foreach (var entry in zip.Entries)
                {
                    using Stream stream = entry.Open();

                    try
                    { 
                        var result = await s3Files.PutObjectAsynS3(S3Client, EnvironmentVariables.BucketS3, 
                            EnvironmentVariables.FolderBase + "/" + EnvironmentVariables.Folder + "/" + folder + "/" + entry.Name,
                            sendmail.ToMemoryStream(stream));
                        if (result.HttpStatusCode != HttpStatusCode.OK)
                        {
                            return new ResponseTransaction() { CodMessage = "500", Message = "Problemas al intentar subir los archivos al bucket s3." };
                        }
                        else
                        {
                            succestry++;
                        }
                    }
                    catch (Exception e)
                    {
                        context.Logger.LogError("Al intentar hacer unzip el método UnZipToMemory se ha caído con el siguiente error:");
                        context.Logger.LogError(e.Message + " " + buscket.Split('/')[0] + "/" + EnvironmentVariables.Folder + "/");
                        return new ResponseTransaction()
                        {
                            CodMessage = "500",
                            Message = "Al intentar hacer unzip usando el método UnZipToMemory en el servidor de bucket s3 se ha caído con el siguiente error :" +
                            e.Message + " " + buscket.Split('/')[0] + "/" + EnvironmentVariables.Folder + "/"
                        };
                    }
                }
                if (succestry == zip.Entries.Count)
                {
                    namefile = zip.Entries.Where(x => x.Name.Contains(".txt")).FirstOrDefault().Name;
                    var finalResult = await ReadData.ReadFile( S3Client,new StreamReader(zip.Entries.Where(x => x.Name.Contains(".txt")).FirstOrDefault().Open()), namefile, folder);
                    return new ResponseTransaction() { CodMessage = finalResult.CodMessage, Message = finalResult.Message };
                }
                else
                {
                    return new ResponseTransaction() { CodMessage = "500", Message = "No se pudieron desplegar todos los archivos al bucket s3." };
                }
            }
            else
            {
                return new ResponseTransaction() { CodMessage = "200", Message = "No se encontro archivo .txt con listado de correos." };
            }

            #endregion 
        }

       
    }
}
