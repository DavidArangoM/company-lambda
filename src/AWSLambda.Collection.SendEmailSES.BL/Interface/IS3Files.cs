using Amazon.S3.Model;
using Amazon.S3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambda.Collection.SendEmailSES.BL.Interface
{
    public interface IS3Files
    {
        public Task<GetObjectResponse> GetObjectAsynS3(IAmazonS3 S3Client, string buscket, string name);
        public Task<PutObjectResponse> PutObjectAsynS3(IAmazonS3 S3Client, string buscket, string name, Stream st);
    }
}
