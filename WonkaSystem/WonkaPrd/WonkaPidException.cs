using System;
using System.Runtime.Serialization;

namespace Wonka.Product
{
    /// <summary>
    /// 
    /// This exception should be used when encountering any issue with referencing
    /// or creating a Product ID for a submitted product.
    /// 
    /// </summary>
    [DataContract(Namespace = "http://sample.wonkasystem.com")]
    class WonkaPidException : Exception
    {
        public WonkaPidException(int pnProductId, string psErrorMessage)
        {
            ProductId = pnProductId;
            Msg = psErrorMessage;
        }

        [DataMember]
        public int ProductId { get; set; }

        [DataMember]
        public string Msg { get; set; }
    }
}
