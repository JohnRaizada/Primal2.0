using System.Net.Http;
using System.Threading.Tasks;

namespace PrimalEditor.Utilities
{
    internal class Network
    {
        internal static async Task DownloadFileFromURLAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    System.IO.File.WriteAllText("downloadedFile.xml", content);
                }
            }
        }
    }
}
