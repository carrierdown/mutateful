using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DocGeneration
{
    internal static class Program
    {
        private static Dictionary<string, string> ParameterTypes = new Dictionary<string, string>
        {
            {"Clip","<[ClipReference](#parameter-types)>"}, 
            {"int", "<[Number](#parameter-types)>"}, 
            {"int[]", "<list of [Number](#parameter-types)>"}, 
            {"bool", ""},
            {"decimal", "<[MusicalDivision](#parameter-types)>"},
            {"decimal[]", "<list of [MusicalDivision](#parameter-types)>"}
        };
        
        private static void Main(string[] args)
        {
            var srcFiles = Directory.EnumerateFiles(Path.Join(Environment.CurrentDirectory, @"..\..\..\..\Mutate4l\Commands\"));
            var commandReference = new StringBuilder();

            foreach (var srcFile in srcFiles)
            {
                commandReference.Append(GetCommandReferenceRow(File.ReadAllLines(srcFile)));
            }

            Console.WriteLine(commandReference.ToString());
        }

        private static string GetCommandReferenceRow(string[] lines)
        {
            var inOptionsBlock = false;
            var options = new Dictionary<string, (string, string)>();
            var optionInfo = "";
            var commandName = "";
            var commandDescription = "";

            foreach (var line in lines)
            {

                if (IsComment(line))
                {
                    if (line.Contains("# desc:"))
                    {
                        commandDescription = line.Substring(line.IndexOf(':') + 1).Trim();
                    }
                    continue;
                }
                if (line.Contains("public class") && line.Contains("Options"))
                {
                    inOptionsBlock = true;
                }

                if (line.Contains(" class ") && !line.Contains("Options"))
                {
                    var parts = line.Trim().Split(' ');
                    if (parts.Length < 4) continue;
                    commandName = parts[3];
                }

                if (inOptionsBlock && line.Trim() == "}")
                {
                    inOptionsBlock = false;
                }

                if (inOptionsBlock)
                {
                    if (line.Contains("[OptionInfo"))
                    {
                        optionInfo = line.Trim();
//                        Console.WriteLine($"Found OptionInfo: {optionInfo}");
                    }

                    if (line.Contains("public") && line.Contains("get;"))
                    {
                        var parts = line.Trim().Split(' ');
                        if (parts.Length < 3) continue;
                        var optionName = parts[2];
                        var optionType = parts[1];
                        options.Add(optionName, (optionType, optionInfo));
                        optionInfo = "";
                    }
                }
            }

//            Console.WriteLine($"Command: {commandName}");
            var output = new StringBuilder(FormatCommandName(commandName)).Append(" | ");
            var formattedOptions = new List<string>();
            foreach (var key in options.Keys)
            {
                var (type, info) = options[key];
                formattedOptions.Add(FormatOptionName(key));
                var formattedType = FormatTypeDescription(type);
                if (formattedType.Length > 0) formattedOptions.Add(formattedType);
            }

            output
                .Append(string.Join(' ', formattedOptions))
                .Append(" | ")
                .Append(commandDescription)
                .Append(Environment.NewLine);
            return output.ToString();
        }

        private static bool IsComment(string line)
        {
            var trimmedLine = line.Trim();
            var slashIx = trimmedLine.IndexOf('/');
            return slashIx >= 0 && trimmedLine[slashIx + 1] == '/';
        }
        
        private static string FormatOptionName(string option)
        {
            return $"-{option.ToLower()}";
        }

        private static string FormatCommandName(string command)
        {
            return command.ToLower();
        }

        private static string FormatTypeDescription(string type)
        {
            if (ParameterTypes.ContainsKey(type))
            {
                return ParameterTypes[type];
            }
            return "";
        }
    }
}