using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CFC.Dto
{
    public class RetourPaiement
    {
        public bool statut { get; set; }
        public string url { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        [JsonProperty(PropertyName = "pay_token")]
        public string pay_token { get; set; }
        [JsonProperty(PropertyName = "payment_url")]
        public string payment_url { get; set; }
        [JsonProperty(PropertyName = "notif_token")]
        public string notif_token { get; set; }
        public string orderId { get; set; }
    }

    public class RetourPaiementMoov
    {
        public string status { get; set; }
        [JsonProperty(PropertyName = "payment_token")]
        public string payment_token { get; set; }
        [JsonProperty(PropertyName = "payment_url")]
        public string payment_url { get; set; }
        public string orderId { get; set; }
    }

    public class Meta
    {

        public string type { get; set; }
        public string source { get; set; }
        public string channel { get; set; }

    }

    //A donner au client pour recevoir la notification après paiement
    public class NotifClientModel
    {
        public Meta meta { get; set; }
        public string status { get; set; }
        public string amount { get; set; }
        public string orderId { get; set; }
        public string currency { get; set; }
        public string reference { get; set; }
        public string countryCode { get; set; }
        public string state { get; set; }
        public string userMsisdn { get; set; }
        public string transactionId { get; set; }
        public string payToken { get; set; }
    }
}