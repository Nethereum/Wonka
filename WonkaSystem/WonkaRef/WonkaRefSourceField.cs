using System.Runtime.Serialization;

namespace WonkaRef
{
    /// <summary>
    /// 
    /// This class represents permissions that an user has in regard 
    /// to a specific Field (for example, the Price field).
    /// 
    /// </summary>
    [DataContract(Namespace = "http://sample.wonkasystem.com")]
    public class WonkaRefSourceField
    {
        public WonkaRefSourceField()
        {
            AutoLock = false;

            SourceFieldId = SourceId = FieldId = -1;
            SecurityLevel = SecurityWeight = -1;
        }

        public int SourceFieldId { get; set; }

        public int SourceId { get; set; }

        public int FieldId { get; set; }

        public int SecurityLevel { get; set; }

        public bool AutoLock { get; set; }

        public int SecurityWeight { get; set; }

    }
}