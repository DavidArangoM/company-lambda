using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambda.Collection.SendEmailSES.Entities
{
    [Serializable]
    public class Sections
    {
        public List<PlaceHolder>? placeHolder { get; set; }
    }
}
