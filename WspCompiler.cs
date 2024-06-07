using System;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace WASP
{
    public static class WspCompiler
    {
        public static void CompileWspToBat(string wspFilePath, string batFilePath)
        {
            if (!File.Exists(wspFilePath))
            {
                throw new FileNotFoundException("The specified .wsp file does not exist.");
            }

            string yamlContent = File.ReadAllText(wspFilePath);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var wspFile = deserializer.Deserialize<WspFile>(yamlContent);

            using (StreamWriter writer = new StreamWriter(batFilePath))
            {
                writer.WriteLine("@echo off");

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
                            WriteFileAction(writer, file);
                            break;
                        case Link link:
                            WriteLinkAction(writer, link);
                            break;
                        case Command command:
                            WriteCommandAction(writer, command);
                            break;
                    }
                }
            }
        }

        private static void WriteFileAction(StreamWriter writer, FileEntry file)
        {
            if (file.Delay.HasValue)
            {
                writer.WriteLine($"timeout /t {file.Delay.Value / 1000}");
            }
            string args = string.IsNullOrEmpty(file.Args) ? "" : $" {file.Args}";
            writer.WriteLine($"start \"\" \"{file.Path}\"{args}");
        }

        private static void WriteLinkAction(StreamWriter writer, Link link)
        {
            if (link.Delay.HasValue)
            {
                writer.WriteLine($"timeout /t {link.Delay.Value / 1000}");
            }
            string browser = string.IsNullOrEmpty(link.Browser) ? "explorer.exe" : link.Browser;
            writer.WriteLine($"start \"\" \"{browser}\" \"{link.Url}\"");
        }

        private static void WriteCommandAction(StreamWriter writer, Command command)
        {
            if (command.Delay.HasValue)
            {
                writer.WriteLine($"timeout /t {command.Delay.Value / 1000}");
            }
            string script = command.Type == "powershell" ? $"powershell -Command \"{command.Script}\"" : $"cmd /C \"{command.Script}\"";
            writer.WriteLine(script);
        }
    }
}
