using System;
using UnityEngine.UIElements;

public class IntNode : BaseNode, ValueNode, GuidProvider
{

    public int IntValue { get; set; }

    public float Value
    {
        get
        {
            var textField = (TextField)this.Query<VisualElement>(null, "node_textfield");
            float value = float.Parse(textField.value);
            return value;
        }
        set
        {
            var textField = (TextField)this.Query<VisualElement>(null, "node_textfield");
            textField.value = value.ToString();
        }
    }

    public Guid Guid { get; set; }

    public IntNode(VisualElement absoluteParent) : base()
    {
        this.AbsoluteParent = absoluteParent;

        this.Label = "Int";
        this.Outputs = ":int:raw";

        this.AddToClassList("int_node");
    }
}
