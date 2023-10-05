using AWSLambda.Collection.SendEmailSES.BL.DO; 
using Amazon.Lambda.Core; 
using System.Net;
using System.Text;
using System.Text.Json;
using AWSLambda.Collection.SendEmailSES.Infra.Enviroment;
using AWSLambda.Collection.SendEmailSES.Infra.Cache;
using Microsoft.Extensions.Caching.Distributed;
using Amazon.S3;
using AWSLambda.Collection.SendEmailSES.BL.Interface;
using AWSLambda.Collection.SendEmailSES.BL.Utils;

namespace AWSLambda.Collection.SendEmailSES.BL.Services
{
    public class ExtracData : IExtracData,   IDisposable
    {
        #region propiedades privadas 
        private static ExtracData? instance;
        private readonly ILambdaContext _logger;
        private readonly ICacheProvider _cacheProvider; 
        private DistributedCacheEntryOptions _cacheConfig = new DistributedCacheEntryOptions
        { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) };


        #endregion

        #region singleton
        public ExtracData(ILambdaContext logger )
        {
            _logger = logger ; 
        }

        public ExtracData() { }


        public static ExtracData Getinstance(ILambdaContext logger)
        { 
            if (instance == null)
            {
                instance = new ExtracData(logger);
            }
            return instance;
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
        /// lee el archivo fila por fila para llamar al motor de correo
        /// </summary>
        /// <param name="st"></param>
        /// <param name="folder"></param>
        /// <returns></returns> 
        public async Task<ResponseTransaction> ReadFile(IAmazonS3 S3Client, StreamReader st, string name, string? folder = null)
        {
            try
            {
                if (st == null)
                {
                    return new ResponseTransaction() { CodMessage = "500", Message = "Problemas al intentar leer el archivo." };
                } 

                string? folderOrganitation = "";

                var jsonStructure = "";
                int total = 0;
                List<MailCustomResponse> listResponseSES = new List<MailCustomResponse>();
                while ((jsonStructure = st.ReadLine()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(jsonStructure))
                    {
                        MailCustom? data = JsonSerializer.Deserialize<MailCustom>(jsonStructure);
                        if (data != null && data.mandatorySection != null)
                        {
                            folderOrganitation = data.mandatorySection.project;
                            data.mandatorySection.attachmentId = folder;
                            total++;
                            if (folder == null)
                            {
                                data.mandatorySection.attachments = null;   
                            }
                            await sendmail.ProcessLineAsync(data, folder, listResponseSES);
                        }
                    }
                }

                // luego de enviar los registros se revisa si es que no hubo errores
                if (listResponseSES != null && listResponseSES.Count > 0)
                {
                    string key = EnvironmentVariables.FolderBase + "/" + EnvironmentVariables.FolderLog + "/" +
                         folderOrganitation + "/" + name.Replace(".txt", "").Replace(".TXT", "") + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
              

                    // Serializar la lista de respuestas a JSON
                    string jsonResponse = string.Join(Environment.NewLine, listResponseSES.Select(response => JsonSerializer.Serialize(response)));

                    // Convertir la cadena JSON a bytes
                    byte[] bytes = Encoding.UTF8.GetBytes(jsonResponse);

                    // Crear un MemoryStream a partir de los bytes
                    using (MemoryStream stream = new MemoryStream(bytes))
                    { 
                        var filelog = await s3Files.PutObjectAsynS3(S3Client, EnvironmentVariables.BucketS3, key, stream);
                        if (filelog.HttpStatusCode != HttpStatusCode.OK)
                        {
                            return new ResponseTransaction() { CodMessage = "500", Message = "El archivo de log no pudo ser guardado." };
                        }

                        return new ResponseTransaction() { CodMessage = "200", Message = "El proceso de envío se completó pero se encontraron errores que se cargaron en: (" + key + "), revise los detalles." };
                    }

                }

                // ... Código para guardar el archivo de log y retornar respuesta
                return new ResponseTransaction() { CodMessage = "200", Message = "Se realizaron los envíos de correos." };

            }
            catch (Exception ee)
            {
                _logger.Logger.LogError(ee.StackTrace);
                return new ResponseTransaction() { CodMessage = "500", Message = ee.Message }; ;
            }
        }
    }
}
