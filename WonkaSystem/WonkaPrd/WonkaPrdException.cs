using System;
using System.Runtime.Serialization;

using WonkaRef;

namespace WonkaPrd
{
    /// <summary>
    /// 
    /// This exception should be used when encountering any issue with processing
    /// a single attribute of interest.
    /// 
    /// </summary>
    [DataContract(Namespace = "http://sample.wonkasystem.com")]
    public class WonkaPrdException : Exception
    {
        public WonkaPrdException(WonkaRefAttr poAttribute, string psErrorMessage)
        {
            if (poAttribute == null) throw new Exception("Provided Attribute is null!");

            AttrId = poAttribute.AttrId;
            Msg    = psErrorMessage;
        }

        [DataMember]
        public int AttrId { get; set; }

        [DataMember]
        public string Msg { get; set; }
    }
}