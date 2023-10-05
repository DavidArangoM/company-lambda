using Amazon.Lambda.Core;
using Amazon.S3.Model;
using Amazon.S3;
using AWSLambda.Collection.SendEmailSES.BL.DO;
using AWSLambda.Collection.SendEmailSES.Infra.Cache;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AWSLambda.Collection.SendEmailSES.BL.Interface;

namespace AWSLambda.Collection.SendEmailSES.BL.Services
{
    public class S3Files : IS3Files, IDisposable
    {
        #region propiedades privadas 
        private static S3Files? instance;
        private readonly ILambdaContext _logger;
        private readonly ICacheProvider _cacheProvider;
        private DistributedCacheEntryOptions _cacheConfig = new DistributedCacheEntryOptions
        { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) };


        #endregion

        #region singleton
        public S3Files(ILambdaContext logger)
        {
            _logger = logger;
        }

        public S3Files() { }


        public static S3Files Getinstance(ILambdaContext logger)
        {

            if (instance == null)
            {
                instance = new S3Files(logger);
            }
            return instance;
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
        public async Task<GetObjectResponse> GetObjectAsynS3(IAmazonS3 S3Client, string buscket, string name )
        {
            S3Client = new AmazonS3Client();
            var response = await S3Client.GetObjectAsync(buscket, name);
            return response;
        }


        /// <summary>
        /// hace unzip para dejar los archivos en una carpeta de adjuntos
        /// </summary>
        /// <param name="buscket"></param>
        /// <param name="name"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<PutObjectResponse> PutObjectAsynS3(IAmazonS3 S3Client, string buscket, string name, Stream st)
        { 
            var requestPut = new PutObjectRequest
            {
                BucketName = buscket,
                Key = name,InputStream = st
            };
            return await S3Client.PutObjectAsync(requestPut);
        }
    }
}
