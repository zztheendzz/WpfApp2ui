using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WpfApp2.modelDTO;

    namespace WpfApp2.Services
    {
        public class CurrencyService
        {
            private static readonly HttpClient client = new HttpClient();

            public async Task<CurrencyDto> GetRates()
            {
                try
                {
                    var json = await client.GetStringAsync("https://open.er-api.com/v6/latest/USD");
                    var data = JObject.Parse(json);

                    return new CurrencyDto
                    {
                        VND = data["rates"]["VND"].Value<decimal>(),
                        KRW = data["rates"]["KRW"].Value<decimal>()
                    };
                }
                catch
                {
                    return new CurrencyDto
                    {
                        VND = 0,
                        KRW = 0
                    };
                }
            }
        }
    }
