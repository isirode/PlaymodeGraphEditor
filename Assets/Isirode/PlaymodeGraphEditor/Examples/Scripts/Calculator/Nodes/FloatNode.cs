using Isirode.PlaymodeGraphEditor.Playmode.Nodes;
using System;
using System.Globalization;
using UnityEngine.UIElements;

public class FloatNode : BaseNode, ValueNode, GuidProvider
{

    public float Value { 
        get
        {
            var textField = (TextField)this.Query<VisualElement>(null, "node_textfield");
            // Info : we do this because my computer is in a different culture
            // TODO : make an utility for this, or special type of VisualElement
            if (!float.TryParse(textField.value, NumberStyles.Float, CultureInfo.InvariantCulture, out float value)) {
                throw new ArgumentException($"Could not parse float value {textField.value}");
            }
            return value;
        }
        set
        {
            var textField = (TextField)this.Query<VisualElement>(null, "node_textfield");
            textField.value = value.ToString();
        }
    }

    public Guid Guid { get; set; }

    public FloatNode(VisualElement absoluteParent) : base()
    {
        this.AbsoluteParent = absoluteParent;

        this.Label = "Float";
        this.Outputs = ":float:raw";

        this.AddToClassList("float_node");
    }
}
