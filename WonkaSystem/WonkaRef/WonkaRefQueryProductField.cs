using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Wonka.MetaData
{
    /// <summary>
    /// 
    /// This class represents specific criteria for querying. 
    /// In particular, this class would be used for queries against the product field metadata.  
    /// For example, it would be employed to find all products where a certain user has locked its
    /// Price field.
    /// 
    /// It is usually used when users issue a dynamic request, asking for
    /// data that fits the specific criteria provided.  (This class can be used in conjunction
    /// with the WonkaRefPidCollection class and other Query classes.)
    ///     
    /// </summary>
    [DataContract(Namespace = "http://sample.wonkasystem.com")]
    [XmlType(TypeName = "QueryProductField")]
    public class WonkaRefQueryProductField
    {
        public WonkaRefQueryProductField()
        {
            FieldId = CurrentSourceId = -1;

            LockCd = null;
        }

        [DataMember]
        public int FieldId { get; set; }

        [DataMember]
        public string LockCd { get; set; }

        [DataMember]
        public int CurrentSourceId { get; set; }

    }
}