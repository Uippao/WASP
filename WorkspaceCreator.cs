using System;
using System.Diagnostics;
using System.IO;

namespace WASP
{
    public static class WorkspaceCreator
    {
        public static void CreateWorkspace()
        {
            Console.WriteLine("Creating a new .wsp file.");
            Console.WriteLine("Enter the path where you want to save the new .wsp file:");
            string path = Console.ReadLine();

            var newWsp = new WspFile
            {
                Files = new[]
                {
                    new FileEntry
                    {
                        Path = "C:\\Path\\To\\Application.exe",
                        Args = "",
                        WorkingDirectory = "C:\\Path\\To",
                        UseShellExecute = true,
                        Verb = "open",
                        Maximized = false,
                        Delay = 1000
                    }
                },
                Links = new[]
                {
                    new Link
                    {
                        Url = "http://www.example.com",
                        Browser = "C:\\Path\\To\\Browser.exe",
                        WindowStyle = ProcessWindowStyle.Normal,
                        Delay = 2000
                    }
                },
                Commands = new[]
                {
                    new Command
                    {
                        Type = "cmd",
                        Script = "echo Hello, World!",
                        RunAsAdministrator = false,
                        Delay = 3000,
                        CreateNoWindow = false
                    }
                }
            };

            var serializer = new YamlDotNet.Serialization.SerializerBuilder()
                .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
                .Build();
            var yaml = serializer.Serialize(newWsp);

            File.WriteAllText(path, yaml);

            Console.WriteLine("New .wsp file created successfully at " + path);
        }
    }
}
