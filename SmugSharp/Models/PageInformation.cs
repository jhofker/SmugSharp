using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SmugSharp.Models
{
    public class PageInformation
    {
        public int Total { get; set; }
        public int Start { get; set; }
        public int Count { get; set; }
        public int RequestedCount { get; set; }
        public string FirstPage { get; set; }
        public string NextPage { get; set; }
        public string LastPage { get; set; }

        public static PageInformation FromJson(string response)
        {
            var responseObj = JObject.Parse(response);
            var jObj = responseObj["Response"]["Pages"];

            PageInformation page = null;
            if (jObj != null)
            {
                page = JsonConvert.DeserializeObject<PageInformation>(jObj.ToString());
            }
            return page;
        }
    }
}
