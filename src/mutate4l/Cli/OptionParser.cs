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
                var attributes = property.GetCustomAttributes(false)
                    .Select(a => (OptionInfo)a)
                    .Where(a => a.Type == OptionType.AllOrSpecified)
                    .ToArray(); //.GroupBy(p => p.GroupId).Select(g => new OptionGroup(g.S))
                if (attributes.Length > 0)
                {
                    // handle properties with OptionType AllOrSpecified
                    foreach (var attrib in attributes)
                    {
                        List<TokenType> toggles;
                        if (togglesByGroupId.ContainsKey(attrib.GroupId))
                        {
                            toggles = togglesByGroupId[attrib.GroupId];
                        }
                        else
                        {
                            toggles = new List<TokenType>();
                            togglesByGroupId[attrib.GroupId] = toggles;
                        }
                        toggles.Add(option);
                    }
                }
                else
                {
                    // handle value properties
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
                                // todo: extract this logic so that it can be used in the list version below as well
                                var rangeInfo = property.GetCustomAttributes(false)
                                    .Select(a => (OptionInfo)a)
                                    .Where(a => a.MaxNumberValue != null && a.MinNumberValue != null).FirstOrDefault();
                                int value = int.Parse(tokens[0].Value);
                                if (value > rangeInfo?.MaxNumberValue)
                                {
                                    value = (int)rangeInfo.MaxNumberValue;
                                }
                                if (value < rangeInfo?.MinNumberValue)
                                {
                                    value = (int)rangeInfo.MinNumberValue;
                                }
                                property.SetMethod?.Invoke(result, new object[] { value });
                            }
                            else if (property.PropertyType.IsEnum)
                            {
                                try
                                {
                                    property.SetMethod?.Invoke(result, new object[] { Enum.Parse(property.PropertyType, tokens[0].Value, ignoreCase: true) });
                                }
                                catch (ArgumentException)
                                {
                                    throw new Exception($"Enum {property.Name} does not support value {tokens[0].Value}");
                                }
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
                            else if (property.PropertyType.IsEnum)
                            {
                                throw new Exception("Only one value supported for enums");
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
