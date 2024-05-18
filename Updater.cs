﻿using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

namespace WASPUpdater
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("This is meant to be used by the main application. Usage (don't know why you'd need this): Updater.exe <download_url> <target_dir>");
                return;
            }

            string downloadUrl = args[0];
            string targetDir = args[1];
            string tempFilePath = Path.Combine(Path.GetTempPath(), "WASP-lite.zip");

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    Console.WriteLine("Downloading the latest version of WASP Lite...");
                    await client.DownloadFileTaskAsync(new Uri(downloadUrl), tempFilePath);

                    Console.WriteLine("Extracting the update...");
                    ZipFile.ExtractToDirectory(tempFilePath, targetDir, true);

                    Console.WriteLine("Update successful.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to update the application: " + ex.Message);
            }
            finally
            {
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }

                // Restart the main application
                string mainAppPath = Path.Combine(targetDir, "WASP Lite.exe");
                Process.Start(mainAppPath);
            }
        }
    }

    public static class HttpClientExtensions
    {
        public static async Task DownloadFileTaskAsync(this HttpClient client, Uri uri, string outputPath)
        {
            using (var response = await client.GetAsync(uri))
            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
            {
                await stream.CopyToAsync(fileStream);
            }
        }
    }
}