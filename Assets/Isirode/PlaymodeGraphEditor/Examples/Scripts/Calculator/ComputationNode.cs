using Isirode.PlaymodeGraphEditor.Playmode.Nodes;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ComputationNode : BaseNode, ValueNode, GuidProvider
{
    private float _value;
    public float Value { 
        get
        {
            return _value;
        } 
        set
        {
            this._value = value;

            var label = (Label)this.Query<VisualElement>(null, "node_input_label");
            label.text = _value.ToString();

        }
    }

    public enum OperationType
    {
        multiply,
        divide,
        add,
        substract
    }

    public Guid Guid { get; set; }

    // Warn : we need to pass absoluteParent & selectExtraRect as parameters because they will be used when we are assigning the inputs / outputs
    public ComputationNode(VisualElement absoluteParent, Vector2 selectExtraRect) : base()
    {
        this.AbsoluteParent = absoluteParent;
        this.SelectExtraRect = selectExtraRect;

        this.Label = "Computation";
        this.Inputs = "Inputs:number:connection,Operation:enum:raw:multiply-divide-add-substract";
        this.Outputs = "Result:number:computed";

        this.AddToClassList("computation_node");
    }

    public override void ConnectedInputChanged(ValueChange valueChange, KnobConfiguration knobConfiguration)
    {
        // Debug.Log(nameof(ConnectedInputChanged));

        // We write in the first label
        float newValue = float.Parse((string)valueChange.newValue);

        var label = (Label)this.Query<VisualElement>(null, "node_input_label");
        label.text = valueChange.newValue.ToString();

        Value = newValue;
    }

    public OperationType GetOperationType()
    {
        var label = GetSelectLabel();

        return ParseOperationType(label.text);
    }

    public OperationType ParseOperationType(String operationTypeAsString)
    {
        switch (operationTypeAsString)
        {
            case "multiply":

                return OperationType.multiply;

            case "divide":

                return OperationType.divide;

            case "add":

                return OperationType.add;

            case "substract":

                return OperationType.substract;

            default:
                throw new Exception($"Operation {operationTypeAsString} is unknown.");
        }
    }

    public void SetOperationType(String operationTypeAsString)
    {
        SetOperationType(
            ParseOperationType(operationTypeAsString)
        );
    }

    public void SetOperationType(OperationType operationType)
    {
        var label = GetSelectLabel();
        label.text = operationType.ToString();
    }

    public Label GetSelectLabel()
    {
        return (Label)this.Query<VisualElement>(null, "select_selection_label");
    }

    public Knob GetOutputKnob()
    {
        return (Knob)this.Query<VisualElement>(null, "node_output_knob");
    }

    public Knob GetInputKnob()
    {
        return (Knob)this.Query<VisualElement>(null, "node_input_knob");
    }
}
