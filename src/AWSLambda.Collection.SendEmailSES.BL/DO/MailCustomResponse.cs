using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambda.Collection.SendEmailSES.BL.DO
{
    [Serializable]
    public class MailCustomResponse
    {
        public MandatorySectionResponse? mandatorySection { get; set; }

        public List<KeyValuePair<string, string>>? uniquePlaceHolder { get; set; }
        public List<Sections>? sections { get; set; }
    }
}
