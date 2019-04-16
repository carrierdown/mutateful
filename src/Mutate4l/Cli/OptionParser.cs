using Mutate4l.Cli;
using Mutate4l.Core;
using Mutate4l.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mutate4l.Cli
{
    public static class OptionParser
    {
        public static (bool Success, string Message) TryParseOptions<T>(Command command, out T result) where T : new()
        {
            var options = command.Options;
            result = new T();
            MemberInfo info = typeof(T);
            var props = result.GetType().GetProperties();
            var togglesByGroupId = new Dictionary<int, List<TokenType>>();

            // todo: better checking for whether incoming options are valid for the current options class or not
            foreach (var property in props)
            {
                if (Enum.TryParse<TokenType>(property.Name, out TokenType option))
                {
                    if (!(option > TokenType._OptionsBegin && option < TokenType._OptionsEnd || option > TokenType._TestOptionsBegin && option < TokenType._TestOptionsEnd))
                    {
                        return (false, $"Property {property.Name} is not a valid option or test option.");
                    }
                }
                else
                {
                    return (false, $"No corresponding entity found for {property.Name}");
                }

                OptionInfo defaultAttribute = property
                    .GetCustomAttributes(false)
                    .Select(a => (OptionInfo)a)
                    .FirstOrDefault(a => a.Type == OptionType.Default);

                if (defaultAttribute != null && command.DefaultOptionValues.Count > 0)
                {
                    var tokens = command.DefaultOptionValues;
                    ProcessResultArray<object> res = ExtractPropertyData(property, tokens);
                    if (!res.Success)
                    {
                        return (false, res.ErrorMessage);
                    }
                    property.SetMethod?.Invoke(result, res.Result);
                    continue;
                }

                var attributes = property.GetCustomAttributes(false)
                    .Select(a => (OptionInfo)a)
                    .Where(a => a.Type == OptionType.AllOrSpecified)
                    .ToArray(); //.GroupBy(p => p.GroupId).Select(g => new OptionGroup(g.S))

                // handle value properties
                if (options.ContainsKey(option))
                {
                    var tokens = options[option];
                    ProcessResultArray<object> res = ExtractPropertyData(property, tokens);
                    if (!res.Success)
                    {
                        return (false, res.ErrorMessage);
                    }
                    property.SetMethod?.Invoke(result, res.Result);
                }
            }
            return (true, "");
        }

        public static ProcessResultArray<object> ExtractPropertyData(PropertyInfo property, List<Token> tokens)
        {
            TokenType? type = tokens.FirstOrDefault()?.Type;
            if (!tokens.All(t => t.Type == type))
            {
                return new ProcessResultArray<object>("Invalid option values: Values for a single option need to be of the same type.");
            }
            if (tokens.Count == 0)
            {
                if (property.PropertyType == typeof(bool))
                {
                    // handle simple bool flag
                    return new ProcessResultArray<object>(new object[] { true });
                } else
                {
                    return new ProcessResultArray<object>($"Missing property value for non-bool parameter: {property.Name}");
                }
            }
            else
            {
                switch (type)
                {
                    // handle single value
                    case TokenType.MusicalDivision when property.PropertyType == typeof(decimal):
                        return new ProcessResultArray<object>(new object[] { Utilities.MusicalDivisionToDecimal(tokens[0].Value) });
                    case TokenType.Decimal when property.PropertyType == typeof(decimal):
                        return new ProcessResultArray<object>(new object[] { decimal.Parse(tokens[0].Value) });
                    case TokenType.Decimal when property.PropertyType == typeof(int):
                        return new ProcessResultArray<object>(new object[] { (decimal)int.Parse(tokens[0].Value) });
                    case TokenType.Number when property.PropertyType == typeof(int):
                    {
                        // todo: extract this logic so that it can be used in the list version below as well
                        var rangeInfo = property
                            .GetCustomAttributes(false)
                            .Select(a => (OptionInfo)a).FirstOrDefault(a => a.MaxNumberValue != null && a.MinNumberValue != null);
                        int value = int.Parse(tokens[0].Value);
                        if (value > rangeInfo?.MaxNumberValue)
                        {
                            value = (int)rangeInfo.MaxNumberValue;
                        }
                        if (value < rangeInfo?.MinNumberValue)
                        {
                            value = (int)rangeInfo.MinNumberValue;
                        }
                        return new ProcessResultArray<object>(new object[] { value });
                    }
                    default:
                    {
                        if (property.PropertyType.IsEnum)
                        {
                            var result = new object();
                            if (Enum.TryParse(property.PropertyType, tokens[0].Value, true, out result))
                            {
                                return new ProcessResultArray<object>(new object[] { result });
                            }
                            else
                            {
                                return new ProcessResultArray<object>($"Enum {property.Name} does not support value {tokens[0].Value}");
                            }
                        }
                        else switch (type)
                        {
                            case TokenType.MusicalDivision when property.PropertyType == typeof(decimal[]):
                            {
                                decimal[] values = tokens.Select(t => Utilities.MusicalDivisionToDecimal(t.Value)).ToArray();
                                return new ProcessResultArray<object>(new object[] { values });
                            }
                            case TokenType.InlineClip when property.PropertyType == typeof(Clip):
                                return new ProcessResultArray<object>(new object[] { tokens[0].Clip });
                            case TokenType.Number when property.PropertyType == typeof(int[]):
                            {
                                int[] values = tokens.Select(t => int.Parse(t.Value)).ToArray();
                                return new ProcessResultArray<object>(new object[] { values });
                            }
                            default:
                                return new ProcessResultArray<object>($"Invalid combination. Token of type {type.ToString()} and property of type {property.PropertyType.Name} are not compatible.");
                        }
                    }
                }
            }
        }
    }
}
