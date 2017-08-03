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
        InterleaveMode,
        _OptionsEnd,

        _EnumValuesBegin,
        eventcount,
        timerange,
        _EnumValuesEnd,

        _ValuesBegin,
        ClipReference,
        Number,
        MusicalDivision,
        _ValuesEnd,

        Colon,
        Destination,
        Unset,

        _TestOptionsBegin,
        GroupOneToggleOne,
        GroupOneToggleTwo,
        GroupTwoToggleOne,
        GroupTwoToggleTwo,
        DecimalValue,
        IntValue,
        EnumValue,
        _TestOptionsEnd,

        _TestEnumValuesBegin,
        enumvalue1,
        enumvalue2,
        _TestEnumValuesEnd
    }
}
