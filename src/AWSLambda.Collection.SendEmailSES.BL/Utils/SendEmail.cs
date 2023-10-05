using Amazon.Lambda.Core;
using AWSLambda.Collection.SendEmailSES.BL.DO;
using AWSLambda.Collection.SendEmailSES.BL.Exceptions;
using AWSLambda.Collection.SendEmailSES.BL.Services;
using AWSLambda.Collection.SendEmailSES.Infra.Cache;
using AWSLambda.Collection.SendEmailSES.Infra.Enviroment;
using Microsoft.Extensions.Caching.Distributed;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AWSLambda.Collection.SendEmailSES.BL.Utils
{
    public class SendEmail
    {
        #region propiedades privadas 
        private static SendEmail? instance;
        private readonly ILambdaContext _logger;
        private readonly ICacheProvider _cacheProvider; 
        private DistributedCacheEntryOptions _cacheConfig = new DistributedCacheEntryOptions
        { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) };


        #endregion

        #region singleton
        public SendEmail(ILambdaContext logger)
        {
            _logger = logger;
        }

        public SendEmail() { }



        public static SendEmail Getinstance(ILambdaContext logger)
        {

            if (instance == null)
            {
                instance = new SendEmail(logger);
            }
            return instance;
        }
        #endregion
        /// <summary>
        /// Consume la api para enviar mail
        /// </summary>
        /// <param name="jsonstructure"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        /// <exception cref="AWSSendEmailSESException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<ResponseTransaction> SendEmailSESAPI(MailCustom jsonstructure, string? folder = null)
        {
            try
            {
                var client = new RestClient(EnvironmentVariables.SendEmailSESEndpoint + EnvironmentVariables.WebAPI);

                var request = new RestRequest(EnvironmentVariables.SendEmailSESEndpoint + EnvironmentVariables.WebAPI, Method.Post);
                request.AddHeader("Content-Type", "application/json");
                var body = JsonSerializer.Serialize(jsonstructure);
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                request.Timeout = 10000;
                var response = await client.ExecuteAsync(request);

                if (response != null)
                    return new ResponseTransaction() { CodMessage = (((byte)response.StatusCode).ToString()), Message = response.StatusDescription.ToString() };
                else
                    return new ResponseTransaction() { CodMessage = (((byte)response.StatusCode).ToString()), Message = response.StatusDescription };

            }
            catch (AWSSendEmailSESException ex) // Excepción si servicio responde distinto a OK
            {
                _logger.Logger.LogError(ex.Message + " JsonCode" + JsonSerializer.Serialize(jsonstructure).ToString());
                return new ResponseTransaction() { CodMessage = "500", Message = ex.Message };
            }
            catch (Exception ex) // Excepción si se cae función por algo interno
            {
                _logger.Logger.LogError(ex.StackTrace + " exception " + " " + ex.Message + " JsonCode" + JsonSerializer.Serialize(jsonstructure).ToString());
                return new ResponseTransaction() { CodMessage = "500", Message = ex.Message };
            }
        }


        /// <summary>
        /// asignacion y llamado a enviar mail, lo deje en un metodo separado para que sea mas facil 
        /// luego pasarlo a hilos o cambiarlo a kafka
        /// </summary>
        /// <param name="data"></param>
        /// <param name="folder"></param>
        /// <param name="listResponseSES"></param>
        /// <returns></returns>
        public async Task ProcessLineAsync(MailCustom data, string folder, List<MailCustomResponse> listResponseSES)
        {

            var response = await SendEmailSESAPI(data, folder);

            if (response.CodMessage != "200")
            {
                var dataresponse = new MailCustomResponse();
                dataresponse.mandatorySection = new MandatorySectionResponse()
                {
                    attachmentId = data.mandatorySection.attachmentId,
                    applicationCode = data.mandatorySection.applicationCode,
                    attachments = data.mandatorySection.attachments,
                    mailBody = data.mandatorySection.mailBody,
                    mailSender = data.mandatorySection.mailSender,
                    mailTarget = data.mandatorySection.mailTarget,
                    organization = data.mandatorySection.organization,
                    project = data.mandatorySection.project,
                    replyAddress = data.mandatorySection.replyAddress,
                    responsibleEmail = data.mandatorySection.responsibleEmail,
                    subjectMail = data.mandatorySection.subjectMail,
                    template = data.mandatorySection.template
                };
                dataresponse.mandatorySection.responseMessage = response.Message;
                dataresponse.mandatorySection.responseCode = response.CodMessage;
                dataresponse.uniquePlaceHolder = data.uniquePlaceHolder;
                dataresponse.sections = data.sections;

                listResponseSES.Add(dataresponse);
            }
        }
        /// <summary>
        /// stream to memory stream 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public MemoryStream ToMemoryStream(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];

            int read;
            MemoryStream ms = new MemoryStream();
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer, 0, read);
            }
            return ms;
        }
    }
}
