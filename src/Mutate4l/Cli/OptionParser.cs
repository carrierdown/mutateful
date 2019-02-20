using Mutate4l.Cli;
using Mutate4l.Core;
using Mutate4l.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mutate4l.Dto
{
    public class OptionParser
    {
        public static T ParseOptions<T>(Command command) where T : new()
        {
            var options = command.Options;
            T result = new T();
            System.Reflection.MemberInfo info = typeof(T);
            var props = result.GetType().GetProperties();
            var togglesByGroupId = new Dictionary<int, List<TokenType>>();

            // todo: better checking for whether incoming options are valid for the current options class or not
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

                OptionInfo defaultAttribute = property.GetCustomAttributes(false)
                    .Select(a => (OptionInfo)a)
                    .Where(a => a.Type == OptionType.Default)
                    .FirstOrDefault();

                if (defaultAttribute != null && command.DefaultOptionValues.Count > 0)
                {
                    var tokens = command.DefaultOptionValues;
                    object[] propertyData = ExtractPropertyData(property, tokens);
                    property.SetMethod?.Invoke(result, propertyData);
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
                    object[] propertyData = ExtractPropertyData(property, tokens);
                    property.SetMethod?.Invoke(result, propertyData);
                }
            }
            return result;
        }

        public static object[] ExtractPropertyData(PropertyInfo property, List<Token> tokens)
        {
            TokenType? type = tokens.FirstOrDefault()?.Type;
            if (!tokens.All(t => t.Type == type))
            {
                throw new Exception("Invalid option values: Values for a single option need to be of the same type.");
            }
            if (tokens.Count == 0)
            {
                if (property.PropertyType == typeof(bool))
                {
                    // handle simple bool flag
                    return new object[] { true };
                } else
                {
                    // todo: report error here - missing property value(s)
                    return new object[] { };
                }
            }
            else
            {
                // handle single value
                if (type == TokenType.MusicalDivision && property.PropertyType == typeof(decimal))
                {
                    return new object[] { Utilities.MusicalDivisionToDecimal(tokens[0].Value) };
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
                    return new object[] { value };
                }
                else if (property.PropertyType.IsEnum)
                {
                    try
                    {
                        return new object[] { Enum.Parse(property.PropertyType, tokens[0].Value, ignoreCase: true) };
                    }
                    catch (ArgumentException)
                    {
                        throw new Exception($"Enum {property.Name} does not support value {tokens[0].Value}");
                    }
                }
                else if (type == TokenType.MusicalDivision && property.PropertyType == typeof(decimal[]))
                {
                    decimal[] values = tokens.Select(t => Utilities.MusicalDivisionToDecimal(t.Value)).ToArray();
                    return new object[] { values };
                }
                else if (type == TokenType.InlineClip && property.PropertyType == typeof(Clip))
                {
                    return new object[] { tokens[0].Clip };
                }
                else if (type == TokenType.Number && property.PropertyType == typeof(int[]))
                {
                    int[] values = tokens.Select(t => int.Parse(t.Value)).ToArray();
                    return new object[] { values };
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
