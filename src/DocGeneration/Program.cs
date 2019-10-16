using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DocGeneration
{
    /*
     * Source code scraper which generates docs from the Command classes
     *
     * Special keywords/tokens used:
     * '// # desc:' Used to signify description for a command
     * '/* ActualDecimal *\/' When appearing right next to a decimal declaration, signifies that a decimal is the intended type, rather than a musical division, which is otherwise assumed 
     */
    internal static class Program
    {
        private static Dictionary<string, string> ParameterTypes = new Dictionary<string, string>
        {
            {"Clip","<[Clip reference](#parameter-types)>"}, 
            {"int", "<[Number](#parameter-types)>"}, 
            {"int[]", "<list of [Number](#parameter-types)>"}, 
            {"bool", ""},
            {"decimal", "<[Musical fraction](#parameter-types)>"},
            {"decimal[]", "<list of [Musical fraction](#parameter-types)>"},
            {"decimal/*ActualDecimal*/", "<[Decimal number](#parameter-types)>"},
            {"decimal[]/*ActualDecimal*/", "<list of [Decimal number](#parameter-types)>"}
        };
        
        private static void Main(string[] args)
        {
            var srcFiles = Directory.EnumerateFiles(Path.Join(Environment.CurrentDirectory, @"..\..\..\..\Mutate4l\Commands\"));
            var commandReference = new StringBuilder();

            foreach (var srcFile in srcFiles)
            {
                var enums = ExtractEnumData(srcFile);
                if (enums.Keys.Count > 0)
                {
                    foreach (var key in enums.Keys)
                    {
                        ParameterTypes.Add(key, string.Join("&#124;", enums[key]));
                    }
                }
            }
            
            foreach (var srcFile in srcFiles)
            {
                commandReference.Append(GetCommandReferenceRow(File.ReadAllLines(srcFile)));
            }

            Console.WriteLine(commandReference.ToString());
        }

        private static string GetCommandReferenceRow(string[] lines)
        {
            var inOptionsBlock = false;
            var options = new Dictionary<string, (string, string, string)>();
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
                        var defaultValue = "";
                        var equalsIx = Array.IndexOf(parts, "=");
                        if (equalsIx >= 0)
                        {
                            if (parts[equalsIx + 1] == "new")
                            {
                                if (parts[equalsIx + 2].Contains("[]"))
                                {
                                    defaultValue = parts[equalsIx + 4];
                                    Console.WriteLine("Setting default to " + defaultValue);
                                }
                            }
                            else if (parts[equalsIx + 1] == "{")
                            {
                                defaultValue = parts[equalsIx + 2];
                                Console.WriteLine("Setting default to " + defaultValue);
                            }
                            else
                            {
                                defaultValue = parts[equalsIx + 1].Trim(';');
                            }
                        }

                        if (defaultValue.Contains('m') && (defaultValue.Contains('.') || defaultValue.Contains('/')))
                        {
                            defaultValue = defaultValue.Substring(0, defaultValue.Length - 1);
                        }
                        options.Add(optionName, (optionType, optionInfo, defaultValue));
                        optionInfo = "";
                    }
                }
            }

//            Console.WriteLine($"Command: {commandName}");
            var output = new StringBuilder(FormatCommandName(commandName)).Append(" | ");
            var formattedOptions = new List<string>();
            foreach (var key in options.Keys)
            {
                var prepend = false;
                var (type, info, @default) = options[key];
                if (info.Contains("OptionType.Default"))
                {
                    prepend = true;
                }
                else
                {
                    formattedOptions.Add(FormatOptionName(key));
                }
                var formattedType = FormatTypeDescription(type, @default);
                if (formattedType.Length > 0)
                {
                    if (prepend) formattedOptions.Insert(0, formattedType);
                    else
                    {
                        if (formattedOptions.Count > 0)
                            formattedOptions[formattedOptions.Count - 1] += "&nbsp;" + formattedType;
                        else 
                            formattedOptions.Add(formattedType);
                    }
                }
            }

            output
                .Append(string.Join("<br>", formattedOptions))
                .Append(" | ")
                .Append(commandDescription)
                .Append(Environment.NewLine);
            return output.ToString();
        }

        private static Dictionary<string, List<string>> ExtractEnumData(string filename)
        {
            var enums = new Dictionary<string, List<string>>();
            var enumName = "";
            var enumValues = new List<string>();
            var lines = File.ReadAllLines(filename);

            var inEnumBlock = false;
            foreach (var line in lines)
            {
                if (inEnumBlock)
                {
                    if (line.Trim() == "}")
                    {
                        enums.Add(enumName, enumValues);
                        enumValues = new List<string>();
                        enumName = "";
                        inEnumBlock = false;
                        continue;
                    }

                    if (!(line.Trim() == "{" || line.Trim() == "}" || line.Trim().StartsWith("/")))
                    {
                        var cleanedLine = line.Trim(' ', ',');
                        if (line.Contains("/"))
                        {
                            cleanedLine = line.Substring(0, line.IndexOf('/')).Trim(' ', ',');
                        }
                        enumValues.Add(cleanedLine);
                    }
                }
                if (line.Contains(" enum "))
                {
                    var parts = line.Split(' ').ToArray();
                    
                    if (parts.Length > 3) enumName = parts[Array.IndexOf(parts, "enum") + 1];
                    inEnumBlock = true;
                }
                
            }

            return enums;
        }
        
        private static bool IsComment(string line)
        {
            var trimmedLine = line.Trim();
            var slashIx = trimmedLine.IndexOf('/');
            return slashIx == 0 && trimmedLine[slashIx + 1] == '/';
        }
        
        private static string FormatOptionName(string option)
        {
            return $"&#8209;{option.ToLower()}";
        }

        private static string FormatCommandName(string command)
        {
            return command.ToLower();
        }

        private static string FormatTypeDescription(string type, string @default)
        {
            var result = "";
            if (ParameterTypes.ContainsKey(type))
            {
                result = ParameterTypes[type];
                if (@default.Length > 0) 
                {
                    if (@default.IndexOf('.') > 0 /* If it starts with . it's a number */ && @default.Length > 1 && @default[1] >= 'a' && @default[1] <= 'z')
                    {
                        // enum
                        @default = @default.Substring(@default.IndexOf('.') + 1);
                    }

                    if (result.Contains(@default))
                    {
                        var defaultIx = result.IndexOf(@default, StringComparison.InvariantCultureIgnoreCase);
                        result = result.Insert(defaultIx + @default.Length, "**").Insert(defaultIx, "**");
                    }
                    else if (@default != "true" && @default != "false")
                    {
                        result = result.Trim('>') + ": **" + @default + "**>";
                    }
                }
            }
            return result;
        }
    }
}