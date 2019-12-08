using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mutate4l.Core;
using Mutate4l.Utility;
using static Mutate4l.Cli.TokenType;

namespace Mutate4l.Cli
{
    public static class OptionParser
    {
        public static (bool Success, string Message) TryParseOptions<T>(Command command, out T result) where T : new()
        {
            var options = command.Options;
            result = new T();
            var props = result.GetType().GetProperties();

            foreach (var property in props)
            {
                if (Enum.TryParse(property.Name, out TokenType option))
                {
                    if (!(option > _OptionsBegin && option < _OptionsEnd ||
                          option > _TestOptionsBegin && option < _TestOptionsEnd))
                    {
                        return (false, $"Property {property.Name} is not a valid option or test option.");
                    }
                }
                else
                {
                    return (false, $"No corresponding entity found for {property.Name}");
                }

                bool noImplicitCast = property
                                          .GetCustomAttributes(false)
                                          .Select(a => (OptionInfo) a)
                                          .FirstOrDefault(a => a.NoImplicitCast)?.NoImplicitCast ?? false;
                
                OptionInfo defaultAttribute = property
                    .GetCustomAttributes(false)
                    .Select(a => (OptionInfo) a)
                    .FirstOrDefault(a => a.Type == OptionType.Default);

                if (defaultAttribute != null && command.DefaultOptionValues.Count > 0)
                {
                    var tokens = command.DefaultOptionValues;
                    ProcessResultArray<object> res = ExtractPropertyData(property, tokens, noImplicitCast);
                    if (!res.Success)
                    {
                        return (false, res.ErrorMessage);
                    }

                    property.SetMethod?.Invoke(result, res.Result);
                    continue;
                }

                // handle value properties
                if (options.ContainsKey(option))
                {
                    var tokens = options[option];
                    ProcessResultArray<object> res = ExtractPropertyData(property, tokens, noImplicitCast);
                    if (!res.Success)
                    {
                        return (false, res.ErrorMessage);
                    }

                    property.SetMethod?.Invoke(result, res.Result);
                }
            }

            return (true, "");
        }

        private static ProcessResultArray<object> ExtractPropertyData(PropertyInfo property, List<Token> tokens,
            bool noImplicitCast = false)
        {
            if (tokens.Count == 0)
            {
                if (property.PropertyType == typeof(bool))
                {
                    // handle simple bool flag
                    return new ProcessResultArray<object>(new object[] {true});
                }

                return new ProcessResultArray<object>($"Missing property value for non-bool parameter: {property.Name}");
            }

            TokenType type = tokens[0].Type;

            var rangeInfo = property
                .GetCustomAttributes(false)
                .Select(a => (OptionInfo) a)
                .FirstOrDefault(a => a.MaxNumberValue != null || a.MinNumberValue != null || a.MinDecimalValue != null || a.MaxDecimalValue != null);

            switch (type)
            {
                // handle single value
                case Number when property.PropertyType == typeof(decimal) && noImplicitCast:
                    return new ProcessResultArray<object>(new object[] {ClampIfSpecified(decimal.Parse(tokens[0].Value), rangeInfo)});
                case MusicalDivision when property.PropertyType == typeof(decimal) && !noImplicitCast:
                case Number when property.PropertyType == typeof(decimal) && !noImplicitCast:
                    return new ProcessResultArray<object>(new object[] {Utilities.MusicalDivisionToDecimal(tokens[0].Value)});
                case BarsBeatsSixteenths when property.PropertyType == typeof(decimal) && !noImplicitCast:
                    return new ProcessResultArray<object>(new object[] {Utilities.BarsBeatsSixteenthsToDecimal(tokens[0].Value)});
                case TokenType.Decimal when property.PropertyType == typeof(decimal):
                    return new ProcessResultArray<object>(new object[] {ClampIfSpecified(decimal.Parse(tokens[0].Value), rangeInfo)});
                case TokenType.Decimal when property.PropertyType == typeof(int) && !noImplicitCast:
                    return new ProcessResultArray<object>(new object[] {ClampIfSpecified((decimal) int.Parse(tokens[0].Value), rangeInfo)});
                case InlineClip when property.PropertyType == typeof(Clip):
                    return new ProcessResultArray<object>(new object[] {tokens[0].Clip});
                case Number when property.PropertyType == typeof(int):
                {
                    if (int.TryParse(tokens[0].Value, out int value))
                    {
                        return new ProcessResultArray<object>(new object[]
                        {
                            ClampIfSpecified(value, rangeInfo)
                        });
                    }
                    return new ProcessResultArray<object>($"Unable to parse value {tokens[0].Value} for parameter {property.Name}");
                }

                default:
                {
                    if (property.PropertyType.IsEnum)
                    {
                        if (Enum.TryParse(property.PropertyType, tokens[0].Value, true, out object result))
                        {
                            return new ProcessResultArray<object>(new[] {result});
                        }

                        return new ProcessResultArray<object>($"Enum {property.Name} does not support value {tokens[0].Value}");
                    }

                    if (property.PropertyType == typeof(decimal[]) && !noImplicitCast &&
                        tokens.All(t => t.Type == Number || t.Type == MusicalDivision || t.Type == TokenType.Decimal || t.Type == BarsBeatsSixteenths))
                    {
                        // handle implicit cast from number or MusicalDivision to decimal
                        decimal[] values = tokens.Select(t =>
                        {
                            if (t.Type == MusicalDivision || t.Type == Number) return Utilities.MusicalDivisionToDecimal(t.Value);
                            if (t.Type == BarsBeatsSixteenths) return Utilities.BarsBeatsSixteenthsToDecimal(t.Value);
                            return ClampIfSpecified(decimal.Parse(t.Value), rangeInfo);
                        }).ToArray();
                        return new ProcessResultArray<object>(new object[] {values});
                    }

                    if (tokens.Any(t => t.Type != type))
                    {
                        return new ProcessResultArray<object>("Invalid option values: Values for a single option need to be of the same type.");
                    }

                    switch (type)
                    {
                        case MusicalDivision when property.PropertyType == typeof(decimal[]) && !noImplicitCast:
                        {
                            decimal[] values = tokens.Select(t => Utilities.MusicalDivisionToDecimal(t.Value)).ToArray();
                            return new ProcessResultArray<object>(new object[] {values});
                        }                        
                        case BarsBeatsSixteenths when property.PropertyType == typeof(decimal[]) && !noImplicitCast:
                        {
                            decimal[] values = tokens.Select(t => Utilities.BarsBeatsSixteenthsToDecimal(t.Value)).ToArray();
                            return new ProcessResultArray<object>(new object[] {values});
                        }
                        case TokenType.Decimal when property.PropertyType == typeof(decimal[]):
                        {
                            decimal[] values = tokens.Select(t => ClampIfSpecified(decimal.Parse(t.Value), rangeInfo)).ToArray();
                            return new ProcessResultArray<object>(new object[] {values});
                        }
                        case Number when property.PropertyType == typeof(int[]):
                        {
                            int[] values = tokens.Select(t => ClampIfSpecified(int.Parse(t.Value), rangeInfo)).ToArray();
                            return new ProcessResultArray<object>(new object[] {values});
                        }
                        default:
                            return new ProcessResultArray<object>($"Invalid combination. Token of type {type.ToString()} and property of type {property.PropertyType.Name} are not compatible.");
                    }
                }
            }
        }

        private static int ClampIfSpecified(int value, OptionInfo rangeInfo)
        {
            if (value > rangeInfo?.MaxNumberValue)
            {
                value = (int) rangeInfo.MaxNumberValue;
            }

            if (value < rangeInfo?.MinNumberValue)
            {
                value = (int) rangeInfo.MinNumberValue;
            }

            return value;
        }

        private static decimal ClampIfSpecified(decimal value, OptionInfo rangeInfo)
        {
            if (rangeInfo?.MinDecimalValue == null && rangeInfo?.MaxDecimalValue == null) return value;
            
            if (rangeInfo?.MinDecimalValue != null)
            {
                var minValue = (decimal) rangeInfo.MinDecimalValue;
                if (value < minValue) value = minValue;
            }

            if (rangeInfo?.MaxDecimalValue != null)
            {
                var maxValue = (decimal) rangeInfo.MaxDecimalValue;
                if (value > maxValue) value = maxValue;
            }

            return value;
        }
    }
}