using Amazon;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
using AWSLambda.Collection.SendEmailSES.BL.Exceptions;
using AWSLambda.Collection.SendEmailSES.BL.DO;
using RestSharp;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using AWSLambda.Collection.SendEmailSES.Infra.Enviroment;
using AWSLambda.Collection.SendEmailSES.BL.Services;
using AWSLambda.Collection.SendEmailSES.BL.Interface;
using AWSLambda.Collection.SendEmailSES.BL.Utils;

//// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AWSLambda.Collection.SendEmailSES
{

    public class Function
    {
        private IAmazonS3 S3Client { get; set; }
        private ILambdaContext _logger;

        private IExtracData ReadData
        {
            get { return ExtracData.Getinstance(_logger); }
        }
        private IExtracFiles Files
        {
            get { return ExtracFiles.Getinstance(_logger); }
        }
        private IS3Files s3Files
        {
            get { return S3Files.Getinstance(_logger); }
        }

        private SendEmail sendmail
        {
            get { return SendEmail.Getinstance(_logger); }
        }
        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public Function()
        {
            S3Client = new AmazonS3Client();
        }

        /// <summary>
        /// Constructs an instance with a preconfigured S3 client. This can be used for testing outside of the Lambda environment.
        /// </summary>
        /// <param name="s3Client"></param>
        public Function(IAmazonS3 s3Client)
        {
            this.S3Client = s3Client;
        }

        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an S3 event object and can be used 
        /// to respond to S3 notifications.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<ResponseTransaction> FunctionHandler(S3Event evnt, ILambdaContext context)
        {
            if (EnvironmentVariables.BucketS3 == null)
                EnvironmentVariables.ConfigurationEnvironmentVariables();

            _logger = context;
            var eventRecords = evnt.Records ?? new List<S3Event.S3EventNotificationRecord>();
            var responses = new ResponseTransaction();
           
            foreach (var record in eventRecords)
            {
                var s3Event = record.S3;
                if (s3Event == null)
                {
                    continue;
                }

                try
                {
                    string namefile = s3Event.Object.Key.Replace(EnvironmentVariables.FolderBase + "/" + EnvironmentVariables.FolderEmail + "/", "");

                    context.Logger.LogInformation("Key " + namefile + "");
                    if (s3Event.Object.Key.ToString().Contains(".zip"))
                    {
                        context.Logger.LogInformation("Encontre un zip.");
                        var unzip = await Files.UnZipToMemory(S3Client, s3Event.Bucket.Name, namefile, context);
                        if (unzip.CodMessage == "200")
                        {
                            responses.CodMessage = unzip.CodMessage;
                            responses.Message = unzip.Message;
                        }
                        else
                        {
                            responses.CodMessage = unzip.CodMessage;
                            responses.Message = unzip.Message;
                        }
                    }
                    else
                    {
                        context.Logger.LogInformation("Encontre un txt.");
                        var readFile = await Files.ReadTextFile(S3Client, s3Event.Bucket.Name, namefile, context);
                        if (readFile.CodMessage == "200")
                        {
                            responses.CodMessage = "200";
                            responses.Message = readFile.Message;
                        }
                        else
                        {
                            responses.CodMessage = responses.CodMessage;
                            responses.Message = readFile.Message;
                            context.Logger.LogError("code: "+responses.CodMessage + ", message:" +readFile.Message);
                        }

                    }
                }
                catch (Exception e)
                {
                    responses.CodMessage = "500"; 
                    responses.Message = e.Message;
                    context.Logger.LogError($"Variable entorno bucket s3 {EnvironmentVariables.BucketS3} -- Error al intentar obtener el siguiente objeto {s3Event.Object.Key} desde el bucket {s3Event.Bucket.Name}.");
                    context.Logger.LogError(e.Message);
                    context.Logger.LogError(e.StackTrace);

                }
            }
            return responses;
        } 
    }
}