using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WonkaPrd
{
    /// <summary>
    /// 
    /// This class represents an error that occurred when attempting to save a 
    /// product record.  It is used to persist the errors to tables (for auditing purposes)
    /// , and it's used in order to inform users of a service.
    ///     
    /// </summary>
    [DataContract(Namespace = "http://sample.wonkasystem.com")]
    [XmlType(TypeName = "Error")]
    public class WonkaProductError
    {
        public WonkaProductError()
        {
            ProductId = "";

            AttrName = ErrorMessage = null;
        }

        [DataMember]
        public string ProductId { get; set; }

        public bool ShouldSerializeProductId()
        {
            return string.IsNullOrEmpty(ProductId);
        }

        [DataMember]
        public string AttrName { get; set; }

        [DataMember]
        public string ErrorMessage { get; set; }
    }
}
