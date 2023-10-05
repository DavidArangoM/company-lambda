using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambda.Collection.SendEmailSES.Infra.Enviroment
{
    public class EnvironmentVariables
    {
        public static string? SendEmailSESEndpoint { get; private set; }
        public static string? WebAPI { get; private set; }
        public static string? BucketS3 { get; private set; }
        public static string? Folder { get; private set; }
        public static string? FolderLog { get; private set; }
        public static string? FolderEmail { get; private set; }
        public static string? FolderBase { get; private set; }
        public static void ConfigurationEnvironmentVariables()
        {

            try
            {
                SendEmailSESEndpoint = Environment.GetEnvironmentVariable(nameof(SendEmailSESEndpoint).ToString());
                WebAPI = Environment.GetEnvironmentVariable(nameof(WebAPI).ToString());
                BucketS3 = Environment.GetEnvironmentVariable(nameof(BucketS3).ToString());
                Folder = Environment.GetEnvironmentVariable(nameof(Folder).ToString());
                FolderLog = Environment.GetEnvironmentVariable(nameof(FolderLog).ToString());
                FolderEmail = Environment.GetEnvironmentVariable(nameof(FolderEmail).ToString());
                FolderBase = Environment.GetEnvironmentVariable(nameof(FolderBase).ToString());

            }
            catch
            {

            }
        }
    }
}
