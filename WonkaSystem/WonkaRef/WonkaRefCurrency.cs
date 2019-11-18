using System.Runtime.Serialization;

namespace Wonka.MetaData
{
    /// <summary>
    /// 
    /// This class represents the data about a currency of interest and its conversion
    /// to US currency.
    ///     
    /// </summary>
    [DataContract(Namespace = "http://sample.wonkasystem.com")]
    public class WonkaRefCurrency
    {
        public WonkaRefCurrency()
        {
            CurrencyId = -1;
            CurrencyCd = null;

            USDList = USDCost = 0.00f;
        }

        [DataMember]
        public int CurrencyId { get; set; }

        [DataMember]
        public string CurrencyCd { get; set; }

        [DataMember]
        public float USDList { get; set; }

        [DataMember]
        public float USDCost { get; set; }

    }
}

