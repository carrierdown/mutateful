namespace Mutate4l.Cli
{
    public enum TokenType
    {
        _CommandsBegin,
        Interleave,
        Constrain,
        Slice,
        Explode,
        _CommandsEnd,

        _OptionsBegin,
        Start,
        Pitch,
        Ranges,
        Counts,
        Mode,
        Strength,
        Mask,
        _OptionsEnd,

        _EnumValuesBegin,
        Event,
        Time,
        _EnumValuesEnd,

        _ValuesBegin,
        ClipReference,
        Number,
        MusicalDivision,
        _ValuesEnd,

        Colon,
        Destination,
        AddToDestination,
        Unset,

        _TestOptionsBegin,
        GroupOneToggleOne,
        GroupOneToggleTwo,
        GroupTwoToggleOne,
        GroupTwoToggleTwo,
        DecimalValue,
        IntValue,
        EnumValue,
        SimpleBoolFlag,
        _TestOptionsEnd,

        _TestEnumValuesBegin,
        EnumValue1,
        EnumValue2,
        _TestEnumValuesEnd
    }
}
