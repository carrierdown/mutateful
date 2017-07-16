using Mutate4l.Cli;
using Mutate4l.ClipActions;
using System.Collections.Generic;

namespace Mutate4l.Dto
{
    public class OptionParser
    {
        public static T ParseOptions<T>(Dictionary<TokenType, List<string>> options, OptionsDefinition optionsDefinition) where T : new()
        {
            T result = new T();

            foreach (var optionGroup in optionsDefinition.OptionGroups)
            {
                switch (optionGroup.Type)
                {
                    case OptionGroupType.InverseToggle:
                        var allOptionsInGroup = new TokenType[optionGroup.Options.Length];
                        var i = 0;
                        foreach (var option in optionGroup.Options)
                        {
                            allOptionsInGroup[i++] = option.Name;
                        }

                        var specifiedOptions = Utility.GetValidOptions(options, allOptionsInGroup);
                        bool noneOrAllSpecified = specifiedOptions.Keys.Count == 0 || specifiedOptions.Keys.Count == allOptionsInGroup.Length;
                        foreach (var option in allOptionsInGroup)
                        {
                            if (noneOrAllSpecified || specifiedOptions.ContainsKey(option))
                            {
                                result.GetType().GetProperty(option.ToString())?.SetMethod?.Invoke(result, new object[] { true });
                            }
                        }
                        break;
                    case OptionGroupType.Value:
                        break;
                }
            }
            return result;
        }
    }
}
