using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO.Compression;

namespace WASP
{
    static class WASPLite
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                string wspFilePath = args[0];
                try
                {
                    HandleWspFile(wspFilePath);
                }
                catch (Exception ex)
                {
                    LogError(ex);
                }
            }
            else
            {
                RegisterFileType();

                Console.WriteLine("Welcome to the Workspace Access and Storage Portal (WASP)!");
                Console.WriteLine("[LITE VERSION]");
                Console.WriteLine();
                Console.WriteLine("This program is designed to handle .wsp files. You can choose to open all .wsp files using WASP!");
                Console.WriteLine("Commands:");
                Console.WriteLine("wasp - Open a .wsp file");
                Console.WriteLine("update - Update the application to the latest version");
                Console.WriteLine("help - Show information on what is WASP and how to use it");
                Console.WriteLine("exit - Exit the program");

                while (true)
                {
                    string userInput = Console.ReadLine();
                    switch (userInput.ToLower())
                    {
                        case "wasp":
                            Console.WriteLine("Enter the path to the .wsp file:");
                            string wspFilePath = Console.ReadLine();
                            HandleWspFile(wspFilePath);
                            break;
                        case "update":
                            UpdateApplication();
                            break;
                        case "help":
                            Console.WriteLine("WASP Lite help");
                            Console.WriteLine("=======================");
                            Console.WriteLine();
                            Console.WriteLine("Welcome to the Workspace Access and Storage Portal (WASP)!");
                            Console.WriteLine("WASP is designed to handle .wsp (WASP Workspace File) files, which are configuration files for opening multiple applications, web links, and executing commands.");
                            Console.WriteLine();
                            Console.WriteLine("WASP Lite is a simplified, console-based version of WASP. The full version of WASP will include a graphical user interface and additional features.");
                            Console.WriteLine();
                            Console.WriteLine("Using WASP Lite:");
                            Console.WriteLine("----------------");
                            Console.WriteLine("To use WASP Lite, open a WASP Workspace File and choose to open it with WASP Lite.");
                            Console.WriteLine("You can use WASP Workspaces to open multiple apps, websites, and execute commands with your specified configuration.");
                            Console.WriteLine();
                            Console.WriteLine("Creating .wsp Files:");
                            Console.WriteLine("--------------------");
                            Console.WriteLine(".wsp files are written in YAML format. Below is an explanation of the structure and what is optional or mandatory.");
                            Console.WriteLine();
                            Console.WriteLine("The basic structure of a .wsp file includes three main sections: files, links, and commands.");
                            Console.WriteLine();
                            Console.WriteLine("Files Section:");
                            Console.WriteLine("  - Each file entry can contain the following fields, but the mandatory fields have to be specified:");
                            Console.WriteLine("    - path: (Mandatory) The path to the application executable, or shortcut (a '.lnk' file).");
                            Console.WriteLine("    - args: (Optional) Arguments to pass to the application. Leave out or set to an empty string if not needed.");
                            Console.WriteLine("    - workingDirectory: (Optional) The working directory for the application. If not specified, it defaults to the directory of the application executable.");
                            Console.WriteLine("    - useShellExecute: (Optional) A boolean value indicating whether to use shell execute. Defaults to true if not specified.");
                            Console.WriteLine("    - verb: (Optional) A verb to use when opening the application. Leave out or set to an empty string if not needed.");
                            Console.WriteLine("    - maximized: (Optional) A boolean value indicating whether to start the application maximized. Defaults to false if not specified.");
                            Console.WriteLine("    - delay: (Optional) The delay in milliseconds before starting the application.");
                            Console.WriteLine();
                            Console.WriteLine("Links Section:");
                            Console.WriteLine("  - Each link entry can contain the following fields, but the mandatory fields have to be specified:");
                            Console.WriteLine("    - url: (Mandatory) The URL to open.");
                            Console.WriteLine("    - browser: (Optional) The path to the browser executable. If not specified, the default browser is used.");
                            Console.WriteLine("    - windowStyle: (Optional) An integer value representing the window style. Defaults to normal (0) if not specified.");
                            Console.WriteLine("    - delay: (Optional) The delay in milliseconds before opening the link.");
                            Console.WriteLine();
                            Console.WriteLine("Commands Section:");
                            Console.WriteLine("  - Each command entry can contain the following fields:");
                            Console.WriteLine("    - type: (Mandatory) The type of command to execute (cmd or powershell).");
                            Console.WriteLine("    - script: (Mandatory) The script or command to execute.");
                            Console.WriteLine("    - runAsAdministrator: (Optional) A boolean value indicating whether to run the command as an administrator. Defaults to false.");
                            Console.WriteLine("    - delay: (Optional) The delay in milliseconds before executing the command.");
                            Console.WriteLine();
                            Console.WriteLine("Example .wsp File:");
                            Console.WriteLine("------------------");
                            Console.WriteLine("files:");
                            Console.WriteLine("  - path: \"C:\\Path\\To\\Application.exe\"");
                            Console.WriteLine("    args: \"\"");
                            Console.WriteLine("    workingDirectory: \"C:\\Path\\To\"");
                            Console.WriteLine("    useShellExecute: true");
                            Console.WriteLine("    verb: \"open\"");
                            Console.WriteLine("    maximized: false");
                            Console.WriteLine("    delay: 1000");
                            Console.WriteLine();
                            Console.WriteLine("links:");
                            Console.WriteLine("  - url: \"http://www.example.com\"");
                            Console.WriteLine("    browser: \"C:\\Path\\To\\Browser.exe\"");
                            Console.WriteLine("    windowStyle: 2");
                            Console.WriteLine("    delay: 2000");
                            Console.WriteLine();
                            Console.WriteLine("commands:");
                            Console.WriteLine("  - type: \"cmd\"");
                            Console.WriteLine("    script: \"echo Hello, World!\"");
                            Console.WriteLine("    runAsAdministrator: false");
                            Console.WriteLine("    delay: 3000");
                            Console.WriteLine();
                            Console.WriteLine("  - type: \"powershell\"");
                            Console.WriteLine("    script: \"Get-Process\"");
                            Console.WriteLine("    runAsAdministrator: true");
                            Console.WriteLine("    delay: 4000");
                            Console.WriteLine();
                            Console.WriteLine("Editing .wsp Files:");
                            Console.WriteLine("-------------------");
                            Console.WriteLine(".wsp files can be edited using any text editor. Simply right-click the file, choose 'open with', open with a text editor of choice and edit the file.");
                            Console.WriteLine("After editing, save the file and re-open it with WASP Lite.");
                            Console.WriteLine("It is strongly recommended to check the file contents with a YAML validator after editing for mistakes.");
                            Console.WriteLine();
                            Console.WriteLine("Thank you for using WASP Lite!");

                            break;
                        case "exit":
                            Environment.Exit(0);
                            break;
                        default:
                            Console.WriteLine("Invalid command. Please try again.");
                            break;
                    }
                }
            }
        }

        private static void RegisterFileType()
        {
            string appPath = Process.GetCurrentProcess().MainModule.FileName;
            string iconPath = Path.Combine(Path.GetDirectoryName(appPath), "wsp.ico");

            try
            {
                // Create the file association
                using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(".wsp"))
                {
                    if (key == null) throw new Exception("Failed to open or create registry key for .wsp.");
                    key.SetValue("", "WaspWorkspaceFile");
                }

                // Set the file type description and icon
                using (RegistryKey key = Registry.ClassesRoot.CreateSubKey("WaspWorkspaceFile"))
                {
                    if (key == null) throw new Exception("Failed to open or create registry key for WaspWorkspaceFile.");
                    key.SetValue("", "WASP Workspace File");

                    using (RegistryKey defaultIconKey = key.CreateSubKey("DefaultIcon"))
                    {
                        if (defaultIconKey == null) throw new Exception("Failed to open or create registry key for DefaultIcon.");
                        defaultIconKey.SetValue("", iconPath);
                    }

                    using (RegistryKey commandKey = key.CreateSubKey(@"shell\open\command"))
                    {
                        if (commandKey == null) throw new Exception("Failed to open or create registry key for command.");
                        commandKey.SetValue("", "\"" + appPath + "\" \"%1\"");
                    }
                }

                Console.WriteLine("File association for .wsp registered successfully.");
            }
            catch (Exception ex)
            {
                LogError(ex);
                Console.WriteLine("Failed to register file association: " + ex.Message);
            }
        }

        private static void HandleWspFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                string yamlContent = File.ReadAllText(filePath);
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();
                var wspFile = deserializer.Deserialize<WspFile>(yamlContent);
                ExecuteActions(wspFile);
                Environment.Exit(0); // Exit the application after handling the file
            }
            else
            {
                throw new FileNotFoundException("The specified .wsp file does not exist.");
            }
        }

        private static void ExecuteActions(WspFile wspFile)
        {
            foreach (var file in wspFile.Files)
            {
                if (file.Delay.HasValue)
                {
                    Thread.Sleep(file.Delay.Value);
                }

                string defaultWorkingDirectory = Path.GetDirectoryName(file.Path);

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = file.Path,
                    Arguments = file.Args ?? string.Empty, // Use empty string if args are not specified
                    WorkingDirectory = !string.IsNullOrEmpty(file.WorkingDirectory) ? file.WorkingDirectory : defaultWorkingDirectory, // Default to application's own directory
                    UseShellExecute = file.UseShellExecute ?? true, // Use true if not specified
                    Verb = file.Verb ?? string.Empty, // Use empty string if verb is not specified
                    WindowStyle = file.Maximized ? ProcessWindowStyle.Maximized : ProcessWindowStyle.Normal
                };

                try
                {
                    Process.Start(startInfo);
                }
                catch (Exception ex)
                {
                    LogError(ex);
                }
            }

            foreach (var link in wspFile.Links)
            {
                if (link.Delay.HasValue)
                {
                    Thread.Sleep(link.Delay.Value);
                }

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = link.Browser ?? "explorer.exe",
                    Arguments = link.Url,
                    WindowStyle = link.WindowStyle.HasValue ? (ProcessWindowStyle)link.WindowStyle.Value : ProcessWindowStyle.Normal
                };

                try
                {
                    Process.Start(startInfo);
                }
                catch (Exception ex)
                {
                    LogError(ex);
                }
            }

            foreach (var command in wspFile.Commands)
            {
                if (command.Delay.HasValue)
                {
                    Thread.Sleep(command.Delay.Value);
                }

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = command.Type == "powershell" ? "powershell.exe" : "cmd.exe",
                    Arguments = command.Type == "powershell" ? $"-Command \"{command.Script}\"" : $"/C \"{command.Script}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    Verb = command.RunAsAdministrator ? "runas" : null // Run as administrator if specified
                };

                try
                {
                    using (Process process = Process.Start(startInfo))
                    {
                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();
                        process.WaitForExit();

                        Console.WriteLine(output);
                        if (!string.IsNullOrEmpty(error))
                        {
                            Console.WriteLine("Error: " + error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex);
                }
            }
        }

        private static async void UpdateApplication()
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
                        Process.Start(updaterPath, $"\"{downloadUrl}\" \"{appDirectory}\"");

                        // Exit the main application to allow the updater to replace files
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
                LogError(ex);
                Console.WriteLine("Failed to update the application: " + ex.Message);
            }
        }


        private static void LogError(Exception ex)
        {
            string logFilePath = "WASP_error_log.txt";
            File.AppendAllText(logFilePath, $"[{DateTime.Now}] {ex.Message}\n");
        }
    }

    public class WspFile
    {
        public FileEntry[] Files { get; set; } = Array.Empty<FileEntry>();
        public Link[] Links { get; set; } = Array.Empty<Link>();
        public Command[] Commands { get; set; } = Array.Empty<Command>();
    }

    public class FileEntry
    {
        public string Path { get; set; }
        public string Args { get; set; }
        public string WorkingDirectory { get; set; }
        public bool? UseShellExecute { get; set; }
        public string Verb { get; set; }
        public bool Maximized { get; set; } = false; // Default to false
        public int? Delay { get; set; } // Delay in milliseconds
    }

    public class Link
    {
        public string Url { get; set; }
        public string Browser { get; set; }
        public ProcessWindowStyle? WindowStyle { get; set; } = ProcessWindowStyle.Normal; // Default to normal
        public int? Delay { get; set; } // Delay in milliseconds
    }

    public class Command
    {
        public string Type { get; set; }
        public string Script { get; set; }
        public bool RunAsAdministrator { get; set; } = false; // Default to false
        public int? Delay { get; set; } // Delay in milliseconds
    }
}
