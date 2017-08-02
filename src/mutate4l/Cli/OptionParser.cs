using Mutate4l.Cli;
using Mutate4l.ClipActions;
using Mutate4l.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mutate4l.Dto
{
    public class OptionParser
    {
        public static T ParseOptions<T>(Dictionary<TokenType, List<Token>> options) where T : new()
        {
            T result = new T();
            System.Reflection.MemberInfo info = typeof(T);
            var props = result.GetType().GetProperties();
            var togglesByGroupId = new Dictionary<int, List<TokenType>>();

            foreach (var property in props)
            {
                TokenType option;
                try
                {
                    option = Enum.Parse<TokenType>(property.Name);
                    if (!(option > TokenType._OptionsBegin && option < TokenType._OptionsEnd || option > TokenType._TestOptionsBegin && option < TokenType._TestOptionsEnd))
                    {
                        throw new Exception($"Property {property.Name} is not a valid option or test option.");
                    }
                }
                catch (ArgumentException)
                {
                    throw new Exception($"No corresponding entity found for {property.Name}");
                }
                var attributes = property.GetCustomAttributes(false);
                if (attributes.Length > 0)
                {
                    foreach (var attrib in attributes)
                    {
                        OptionInfo attribInfo = (OptionInfo)attrib;
                        if (attribInfo.Type == OptionType.AllOrSpecified)
                        {
                            List<TokenType> toggles;
                            if (togglesByGroupId.ContainsKey(attribInfo.GroupId))
                            {
                                toggles = togglesByGroupId[attribInfo.GroupId];
                            }
                            else
                            {
                                toggles = new List<TokenType>();
                                togglesByGroupId[attribInfo.GroupId] = toggles;
                            }
                            toggles.Add(option);
                        }
                    }
                }
                else
                {
                    if (options.ContainsKey(option))
                    {
                        var tokens = options[option];
                        TokenType? type = tokens.FirstOrDefault()?.Type;
                        if (!tokens.All(t => t.Type == type))
                        {
                            throw new Exception("Invalid option values: Values for a single option need to be of the same type.");
                        }
                        if (tokens.Count == 1)
                        {
                            // handle single value
                            if (type == TokenType.MusicalDivision && property.PropertyType == typeof(decimal))
                            {
                                property.SetMethod?.Invoke(result, new object[] { Utility.MusicalDivisionToDecimal(tokens[0].Value) });
                            }
                            else if (type == TokenType.Number && property.PropertyType == typeof(Int32))
                            {
                                property.SetMethod?.Invoke(result, new object[] { int.Parse(tokens[0].Value) });
                            }
                            else
                            {
                                throw new Exception($"Invalid combination. Token of type {type.ToString()} and property of type {property.PropertyType.Name} are not compatible.");
                            }
                        }
                        else if (tokens.Count > 1)
                        {
                            // handle list
                            if (type == TokenType.MusicalDivision && property.PropertyType == typeof(decimal[]))
                            {
                                decimal[] values = tokens.Select(t => Utility.MusicalDivisionToDecimal(t.Value)).ToArray();
                                property.SetMethod?.Invoke(result, new object[] { values });
                            }
                            else if (type == TokenType.Number && property.PropertyType == typeof(int[]))
                            {
                                int[] values = tokens.Select(t => int.Parse(t.Value)).ToArray();
                                property.SetMethod?.Invoke(result, new object[] { values });
                            }
                            else
                            {
                                throw new Exception($"Invalid combination. Token of type {type.ToString()} and property of type {property.PropertyType.Name} are not compatible.");
                            }
                        }
                    }
                }
            }

            var optionGroups = new List<OptionGroup>();
            foreach (var toggle in togglesByGroupId.Keys)
            {
                var group = new OptionGroup();
                group.Options = togglesByGroupId[toggle].ToArray();
                optionGroups.Add(group);
            }
            var optionsDefinition = new OptionsDefinition() { OptionGroups = optionGroups.ToArray() };

            foreach (var optionGroup in optionsDefinition.OptionGroups)
            {
                var specifiedOptions = Utility.GetValidOptions(options, optionGroup.Options);
                bool noneOrAllSpecified = specifiedOptions.Keys.Count == 0 || specifiedOptions.Keys.Count == optionGroup.Options.Length;
                foreach (var option in optionGroup.Options)
                {
                    if (noneOrAllSpecified || specifiedOptions.ContainsKey(option))
                    {
                        result.GetType().GetProperty(option.ToString())?.SetMethod?.Invoke(result, new object[] { true });
                    }
                }
            }
            return result;
        }
    }
}
