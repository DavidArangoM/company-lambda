using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambda.Collection.SendEmailSES.Entities
{
    public class ResponseTransaction
    {
        public string? Message { get; set; }
        public string? CodMessage { get; set; }

    }
}
