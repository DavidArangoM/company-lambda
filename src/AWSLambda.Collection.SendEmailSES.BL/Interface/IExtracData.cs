using Amazon.S3;
using AWSLambda.Collection.SendEmailSES.BL.DO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambda.Collection.SendEmailSES.BL.Interface
{
    public interface IExtracData
    {
        public Task<ResponseTransaction> ReadFile(IAmazonS3 S3Client, StreamReader st, string name, string? folder = null);
    }
}
