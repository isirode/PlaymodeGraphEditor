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
