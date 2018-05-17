using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WonkaRef
{
    /// <summary>
    /// 
    /// This class represents specific criteria for querying your database, in a way similar
    /// to GraphQL.  In particular, this class would be used for queries against the actual product data
    /// itself.  For example, it would be employed to find all products where the price is
    /// above $100 and where the weight > 100.
    /// 
    /// It is usually used when users issue a dynamic request, asking for data that fits the 
    /// specific criteria provided.
    ///     
    /// </summary>
    [DataContract(Namespace = "http://sample.wonkasystem.com")]
    [XmlType(TypeName = "QueryCriteria")]
    public class WonkaRefQueryCriteria
    {
        public WonkaRefQueryCriteria()
        {
            Type = AttrName = FieldName = null;

            LowerValue = UpperValue = null;

            Value = null;

            NotOperator = false;
        }

        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public string AttrName { get; set; }

        [DataMember]
        public string FieldName { get; set; }

        [DataMember]
        public string LowerValue { get; set; }

        [DataMember]
        public string UpperValue { get; set; }

        [DataMember]
        public string Value { get; set; }

        [DataMember]
        public bool NotOperator { get; set; }

    }
}

