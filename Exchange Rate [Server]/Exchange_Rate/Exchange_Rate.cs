namespace Message_Exchange_Rate
{
    using System.Runtime.Serialization;
    using Сurrencies;

    [Serializable]
    [DataContract]
    public class Exchange_Rate
    {
        [DataMember]
        public Сurrencies currency_1 { get; set; }
        [DataMember]
        public Сurrencies currency_2 { get; set; }
    }
}
