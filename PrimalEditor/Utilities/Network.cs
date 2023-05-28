using System.Net.Http;
using System.Threading.Tasks;

namespace PrimalEditor.Utilities
{
    internal class Network
    {
        public static async Task DownloadFileFromURLAsync(string url, string destinationFolder)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    byte[] content = await response.Content.ReadAsByteArrayAsync();
                    string? fileName = response.Content.Headers.ContentDisposition?.FileName;
                    if (string.IsNullOrEmpty(fileName))
                    {
                        fileName = System.IO.Path.GetFileName(url);
                    }
                    fileName = GenerateUniqueFileName(destinationFolder, fileName);
                    System.IO.File.WriteAllBytes(fileName, content);
                }
            }
        }

        private static string GenerateUniqueFileName(string destinationFolder, string? originalFileName)
        {
            if (string.IsNullOrEmpty(originalFileName))
            {
                originalFileName = "downloadedFile.xml";
            }
            string fileName = System.IO.Path.Combine(destinationFolder, originalFileName);
            int count = 1;
            while (System.IO.File.Exists(fileName))
            {
                fileName = System.IO.Path.Combine(destinationFolder, System.IO.Path.GetFileNameWithoutExtension(originalFileName) + $" ({count})" + System.IO.Path.GetExtension(originalFileName));
                count++;
            }
            return fileName;
        }
    }
}
