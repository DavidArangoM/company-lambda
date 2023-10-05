using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambda.Collection.SendEmailSES.BL.Exceptions
{
    /// <summary>
    /// AWSSendEmailSESException exception custom
    /// </summary>
    [Serializable]
    public class AWSSendEmailSESException : Exception, ISerializable
    {
        /// <summary>
        /// CTOR with base message
        /// </summary>
        /// <param name="message"></param>
        public AWSSendEmailSESException(string message) : base(message)
        { }

        public AWSSendEmailSESException(string message, Exception inner) : base(message, inner)
        { }

        /// <summary>
        /// Serialization Pattenr ctor
        /// </summary>
        /// <param name="serialInfo"></param>
        /// <param name="strContext"></param>
        protected AWSSendEmailSESException(SerializationInfo serialInfo, StreamingContext strContext)
        {

        }
    }
}
