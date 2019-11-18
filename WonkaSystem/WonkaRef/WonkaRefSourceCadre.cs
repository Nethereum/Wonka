using System.Runtime.Serialization;

namespace Wonka.MetaData
{
    /// <summary>
    /// 
    /// This class represents permissions that an user has in regard 
    /// to a specific Cadre (for example, the Price cadre).
    /// 
    /// </summary>
    [DataContract(Namespace = "http://sample.wonkasystem.com")]
    public class WonkaRefSourceCadre
    {
        public WonkaRefSourceCadre()
        {
            AutoLock = false;

            SourceCadreId = SourceId = CadreId = -1;
            SecurityLevel = SecurityWeight = -1;
        }

        public int SourceCadreId { get; set; }

        public int SourceId { get; set; }

        public int CadreId { get; set; }

        public int SecurityLevel { get; set; }

        public bool AutoLock { get; set; }

        public int SecurityWeight { get; set; }

    }
}