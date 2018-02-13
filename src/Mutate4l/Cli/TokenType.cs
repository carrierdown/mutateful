namespace Mutate4l.Cli
{
    public enum TokenType
    {
        _CommandsBegin,
        Interleave,
        Constrain,
        Slice,
        Explode,
        Arpeggiate,
        Sustain,
        Filter,
        Monophonize,
        Ratchet,
        _CommandsEnd,

        _OptionsBegin,
        Start,
        Pitch,
        Ranges,
        Repeats,
        Mode,
        Strength,
        Mask,
        Lengths,
        Rescale,
        RemoveOffset,
        Min,
        Max,
        Shape,
        AutoScale,
        ControlMin,
        ControlMax,
        VelocityToStrength,
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
