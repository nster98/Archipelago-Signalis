using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace ArchipelagoSignalis
{
    public class ArchipelagoStart : MelonMod {

        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();
            MelonLogger.Msg("ArchipelagoSignalis loaded");
            FetchData().GetAwaiter().GetResult();
        }

        private async Task FetchData()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync("https://api.github.com/repos/dotnet/runtime");
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();

                    JObject json = JObject.Parse(responseBody);
                    MelonLogger.Msg("Data fetched from GitHub API:");
                    MelonLogger.Msg(json.ToString());
                }
                catch (HttpRequestException e)
                {
                    MelonLogger.Error($"Request error: {e.Message}");
                }
            }
            
        }
    }
}
