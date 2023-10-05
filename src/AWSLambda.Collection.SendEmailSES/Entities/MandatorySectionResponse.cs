﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambda.Collection.SendEmailSES.Entities
{ 
   [Serializable]
    public class MandatorySectionResponse
    {

        public MandatorySectionResponse()
        {
        }

        public MandatorySectionResponse(string mailTarget, string mailSender, string? template, string project, string applicationCode, string organization,
            string subjectMail, string? mailBody, string? responsibleEmail, string? replyAddress, string? attachmentId, string[]? attachments,
            string? responseCode, string? responseMessage)
        {
            this.mailTarget = mailTarget;
            this.mailSender = mailSender;
            this.template = template;
            this.project = project;
            this.applicationCode = applicationCode;
            this.organization = organization;
            this.subjectMail = subjectMail;
            this.mailBody = mailBody;
            this.responsibleEmail = responsibleEmail;
            this.replyAddress = replyAddress;
            this.attachmentId = attachmentId;
            this.attachments = attachments;
            this.responseCode = responseCode;
            this.responseMessage = responseMessage;
        }

        public string? mailTarget { get; set; }
        public string? mailSender { get; set; }
        public string? template { get; set; }
        public string? project { get; set; }
        public string? applicationCode { get; set; }
        public string? organization { get; set; }
        public string? subjectMail { get; set; }
        public string? mailBody { get; set; }
        public string? responsibleEmail { get; set; }
        public string? replyAddress { get; set; }
        public string? attachmentId { get; set; }
        public string[]? attachments { get; set; }
        public string? responseCode { get; set; }
        public string? responseMessage { get; set; }
    }
}
