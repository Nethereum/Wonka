using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Wonka.Product
{
    /// <summary>
    /// 
    /// This class represents the current state and history of a Cadre within 
    /// a specific product.
    /// 
    /// For example, a product could have its Price cadre locked by a specific user, 
    /// and this data would be contained within an instance of this class.
    ///     
    /// </summary>
    [DataContract(Namespace = "http://sample.wonkasystem.com")]
    [XmlType(TypeName = "ProductCadre")]
    public class WonkaProductCadre
    {

        public WonkaProductCadre()
        {
            ProductId = "";

            CadreId = -1;

            CadreName = "";

            LockCd = NullInd = "N";

            LastTouchedSourceId = LockSourceId = -1;

            ModTime = LockTime = "";

            CanUpdate = CanLock = "N";

            PersistFlag = false;
        }

        [DataMember]
        public string ProductId { get; set; }

        [XmlIgnore]
        public int CadreId { get; set; }

        [DataMember]
        public string CadreName { get; set; }

        [DataMember]
        public string LockCd { get; set; }

        [DataMember]
        public string NullInd { get; set; }

        [DataMember]
        public int LastTouchedSourceId { get; set; }

        [DataMember]
        public string ModTime { get; set; }

        [DataMember]
        public int LockSourceId { get; set; }

        [DataMember]
        public string LockTime { get; set; }

        [DataMember]
        public string CanUpdate { get; set; }

        [DataMember]
        public string CanLock { get; set; }

        [DataMember]
        public bool PersistFlag { get; set; }

    }
}

