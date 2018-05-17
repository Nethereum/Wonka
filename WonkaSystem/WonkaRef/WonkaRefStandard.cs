using System.Runtime.Serialization;

namespace WonkaRef
{
    /// <summary>
    /// 
    /// This class represents an environment variable for a specific aspect of the 
    /// system.  For example, let's say the database tables that refer
    /// to a Product ID all have a "pid" column, and when we wish to refer to 
    /// that column within the code, we can use a Standard variable with the name
    /// of that column (instead of it being hard-coded within the application).
    /// 
    /// </summary>
    [DataContract(Namespace = "http://sample.wonkasystem.com")]
    public class WonkaRefStandard
    {
        public WonkaRefStandard()
        {
            StandardId = -1;

            StandardName = StandardValue = null;
        }

        public int StandardId { get; set; }

        public string StandardName { get; set; }

        public string StandardValue { get; set; }

    }
}
