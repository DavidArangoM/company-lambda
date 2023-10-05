using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambda.Collection.SendEmailSES.BL.DO
{
    [Serializable]
    public class Sections
    {
        public List<PlaceHolder>? placeHolder { get; set; }
    }
}
