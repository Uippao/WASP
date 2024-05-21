using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace WASP_Lite
{
    internal class UpdaterClass
    {
        public static async void UpdateApplication()
        {
            string owner = "Uippao";
            string repo = "WASP";
            string latestReleaseUrl = $"https://api.github.com/repos/{owner}/{repo}/releases/latest";
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string updaterPath = Path.Combine(appDirectory, "WASP Lite Updater.exe");

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("WASP", "1.0"));
                    Console.WriteLine("Fetching the latest release information...");
                    HttpResponseMessage response = await client.GetAsync(latestReleaseUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        var release = System.Text.Json.JsonDocument.Parse(json).RootElement;
                        string downloadUrl = release.GetProperty("assets")[0].GetProperty("browser_download_url").GetString();

                        Console.WriteLine("Launching the updater...");
                        ProcessStartInfo startInfo = new ProcessStartInfo
                        {
                            FileName = updaterPath,
                            Arguments = $"\"{downloadUrl}\" \"{appDirectory}\"",
                            UseShellExecute = true
                        };
                        Process.Start(startInfo);

                        Environment.Exit(0);
                    }
                    else
                    {
                        Console.WriteLine("Failed to fetch the latest release information.");
                    }
                }
            }
            catch (Exception ex)
            {
                WASP.Program.LogError(ex);
                Console.WriteLine("Failed to update the application: " + ex.Message);
            }
        }
    }
}
