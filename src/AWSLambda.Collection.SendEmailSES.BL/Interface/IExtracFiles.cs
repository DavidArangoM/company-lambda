using Amazon.Lambda.Core;
using Amazon.S3;
using AWSLambda.Collection.SendEmailSES.BL.DO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambda.Collection.SendEmailSES.BL.Interface
{
    public  interface IExtracFiles
    {
        public Task<ResponseTransaction> ReadTextFile(IAmazonS3 S3Client, string buscket, string name, ILambdaContext context);
        public Task<ResponseTransaction> UnZipToMemory(IAmazonS3 S3Client, string buscket, string name, ILambdaContext context);
    }
}
