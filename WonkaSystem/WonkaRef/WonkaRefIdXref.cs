using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Wonka.MetaData
{
    /// <summary>
    /// 
    /// This class serves as an index/cache for storing the members of our abstract views.
    /// For example, we would use this class in order to obtain the list of Attributes 
    /// that belong to a specific Field.
    ///     
    /// </summary>
    [DataContract(Namespace = "http://sample.wonkasystem.com")]
    public class WonkaRefIdXref
    {
        public WonkaRefIdXref()
        {
            FieldIdToAttrIds    = new Dictionary<int, HashSet<int>>();
            GroupIdToAttrIds    = new Dictionary<int, HashSet<int>>();
            GroupIdToFieldIds   = new Dictionary<int, HashSet<int>>();
            GroupIdToKeyAttrIds = new Dictionary<int, HashSet<int>>();

            GroupIdToGroupSeqAttrId = new Dictionary<int, int>();

            SourceFields = new Dictionary<int, Dictionary<int, int>>();
        }

        public Dictionary<int, HashSet<int>> FieldIdToAttrIds { get; set; }

        public Dictionary<int, HashSet<int>> GroupIdToAttrIds { get; set; }

        public Dictionary<int, HashSet<int>> GroupIdToFieldIds { get; set; }

        public Dictionary<int, int> GroupIdToGroupSeqAttrId { get; set; }

        public Dictionary<int, HashSet<int>> GroupIdToKeyAttrIds { get; set; }

        public Dictionary<int, Dictionary<int, int>> SourceFields { get; set; }

    }
}