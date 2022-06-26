using Isirode.PlaymodeGraphEditor.Playmode.Nodes;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ExportNode : BaseNode, GuidProvider
{
    public Guid Guid { get; set; }

    public ExportNode(VisualElement absoluteParent) : base()
    {
        this.AbsoluteParent = absoluteParent;

        this.Label = "Export";
        this.Inputs = ":number:connection";

        this.AddToClassList("export_node");
    }

    public override void ConnectedInputChanged(ValueChange valueChange, KnobConfiguration knobConfiguration)
    {
        // Debug.Log(nameof(ConnectedInputChanged));

        ChangeValue(valueChange.newValue.ToString());

    }

    public void ChangeValue(float value)
    {
        ChangeValue(value.ToString());
    }

    public void ChangeValue(string value)
    {
        // Debug.Log(nameof(ChangeValue));

        var label = GetLabel();
        label.text = value;
    }

    public Label GetLabel()
    {
        // We get the first label
        return (Label)this.Query<VisualElement>(null, "node_input_label");
    }
}
