using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace CFC.Dto
{
    public class Api
    {
        public static HttpClient Url { get; set; }

        static Api()
        {
            var url = ConfigurationManager.AppSettings["ApiBIZAO"];
            Url = new HttpClient();
            Url.BaseAddress = new Uri(url);
        }
    }
}