using System.Runtime.Serialization;

namespace Wonka.MetaData
{
    /// <summary>
    /// 
    /// This class represents a user.
    /// 
    /// </summary>
    [DataContract(Namespace = "http://sample.wonkasystem.com")]
    public class WonkaRefSource
    {
        public WonkaRefSource()
        {
            SourceId = -1;

            SourceName = Status = null;
        }

        public int SourceId { get; set; }

        public string SourceName { get; set; }

        public string Status { get; set; }
    }
}