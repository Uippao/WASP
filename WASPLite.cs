using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Microsoft.Win32;
using System.Net.Http.Headers;
using System.Linq;

namespace WASP
{
    class Program
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
                AddContextMenuEntry();

                Console.WriteLine("Welcome to the Workspace Access and Storage Portal (WASP)!");
                Console.WriteLine("[LITE VERSION]");
                Console.WriteLine();
                Console.WriteLine("This program is designed to handle .wsp files. You can choose to open all .wsp files using WASP!");
                Console.WriteLine("Commands:");
                Console.WriteLine("wasp - Open a .wsp file");
                Console.WriteLine("update - Update the application to the latest version");
                Console.WriteLine("help - Show information on what is WASP and how to use it");
                Console.WriteLine("create - Create a new Workspace file with default contents");
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
                            WASP_Lite.UpdaterClass.UpdateApplication();
                            break;
                        case "help":
                            DisplayHelp();
                            break;
                        case "create":
                            WorkspaceCreator.CreateWorkspace();
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

        private static void DisplayHelp()
        {
            try
            {
                string helpText = File.ReadAllText("HELP.txt");
                Console.WriteLine(helpText);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to read the help file: " + ex.Message);
            }
        }

        private static void RegisterFileType()
        {
            string appPath = Process.GetCurrentProcess().MainModule.FileName;
            string iconPath = Path.Combine(Path.GetDirectoryName(appPath), "wsp.ico");

            try
            {
                using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(".wsp"))
                {
                    if (key == null) throw new Exception("Failed to open or create registry key for .wsp.");
                    key.SetValue("", "WaspWorkspaceFile");
                }

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

        private static void AddContextMenuEntry()
        {
            try
            {
                using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(@"Directory\Background\shell\New WASP Workspace File"))
                {
                    if (key == null) throw new Exception("Failed to open or create registry key for context menu.");
                    key.SetValue("", "New WASP Workspace File");

                    using (RegistryKey commandKey = key.CreateSubKey("command"))
                    {
                        if (commandKey == null) throw new Exception("Failed to open or create registry key for context menu command.");
                        commandKey.SetValue("", "\"" + Process.GetCurrentProcess().MainModule.FileName + "\" create \"%V\\NewWorkspace.wsp\"");
                    }
                }

                using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(@".wsp\ShellNew"))
                {
                    if (key == null) throw new Exception("Failed to open or create registry key for ShellNew.");
                    key.SetValue("NullFile", "");
                }

                Console.WriteLine("Context menu entry for creating new WASP Workspace file registered successfully.");
            }
            catch (Exception ex)
            {
                LogError(ex);
                Console.WriteLine("Failed to register context menu entry: " + ex.Message);
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
                Environment.Exit(0);
            }
            else
            {
                throw new FileNotFoundException("The specified .wsp file does not exist.");
            }
        }

        private static void ExecuteActions(WspFile wspFile)
        {
            var actions = wspFile.Files.Cast<object>()
                .Concat(wspFile.Links.Cast<object>())
                .Concat(wspFile.Commands.Cast<object>())
                .OrderBy(a => a switch
                {
                    FileEntry file => file.Order,
                    Link link => link.Order,
                    Command command => command.Order,
                    _ => int.MaxValue
                });

            foreach (var action in actions)
            {
                switch (action)
                {
                    case FileEntry file:
                        ExecuteFile(file);
                        break;
                    case Link link:
                        ExecuteLink(link);
                        break;
                    case Command command:
                        ExecuteCommand(command);
                        break;
                }
            }
        }

        private static void ExecuteFile(FileEntry file)
        {
            if (file.Delay.HasValue)
            {
                Thread.Sleep(file.Delay.Value);
            }

            string defaultWorkingDirectory = Path.GetDirectoryName(file.Path);

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = file.Path,
                Arguments = file.Args ?? string.Empty,
                WorkingDirectory = !string.IsNullOrEmpty(file.WorkingDirectory) ? file.WorkingDirectory : defaultWorkingDirectory,
                UseShellExecute = file.UseShellExecute ?? true,
                Verb = file.Verb ?? string.Empty,
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

        private static void ExecuteLink(Link link)
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

        private static void ExecuteCommand(Command command)
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
                CreateNoWindow = command.CreateNoWindow,
                Verb = command.RunAsAdministrator ? "runas" : null
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

        public static void LogError(Exception ex)
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
        public bool Maximized { get; set; } = false;
        public int? Delay { get; set; }
        public int Order { get; set; }
    }

    public class Link
    {
        public string Url { get; set; }
        public string Browser { get; set; }
        public ProcessWindowStyle? WindowStyle { get; set; } = ProcessWindowStyle.Normal;
        public int? Delay { get; set; }
        public int Order { get; set; }
    }

    public class Command
    {
        public string Type { get; set; }
        public string Script { get; set; }
        public bool RunAsAdministrator { get; set; } = false;
        public int? Delay { get; set; }
        public bool CreateNoWindow { get; set; } = false;
        public int Order { get; set; }
    }
}
