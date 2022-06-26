using Isirode.PlaymodeGraphEditor.Playmode.Nodes;
using System;

[Serializable]
public struct ConnectionDto
{
    public String startGuid;
    public String startKnobLabel;
    public BaseNode.ValueType startValueType;
    public String endGuid;
    public String endKnobLabel;
    public BaseNode.ValueType endValueType;
}

