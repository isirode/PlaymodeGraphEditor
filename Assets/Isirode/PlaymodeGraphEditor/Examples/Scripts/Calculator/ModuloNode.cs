using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ModuloNode : BaseNode, GuidProvider
{
    private int _quotient;
    public int Quotient { 
        get
        {
            return _quotient;
        }
        set
        {
            _quotient = value;
        }
    }

    private int _remainder;
    public int Remainder
    {
        get
        {
            return _remainder;
        }
        set
        {
            _remainder = value;
        }
    }

    private const String QuotientLabel = "Quotient";
    private const String RemainderLabel = "Remainder";
    private const String DividendLabel = "Dividend";
    private const String DivisorLabel = "Divisor";

    public Guid Guid { get; set; }

    // Warn : we need to pass absoluteParent & selectExtraRect as parameters because they will be used when we are assigning the inputs / outputs
    public ModuloNode(VisualElement absoluteParent, Vector2 selectExtraRect) : base()
    {
        this.AbsoluteParent = absoluteParent;
        this.SelectExtraRect = selectExtraRect;

        this.Label = "Computation";

        var inputs = new List<InputConfiguration>();
        inputs.Add(new InputConfiguration()
        {
            label = DividendLabel,
            valueType = ValueType.Number,
            inputType = InputType.Connection,
            classes = new List<String>() { DividendLabel.ToLower() }// FIXME : maybe do not use ToLower ?
        });
        inputs.Add(new InputConfiguration()
        {
            label = DivisorLabel,
            valueType = ValueType.Number,
            inputType = InputType.Connection,
            classes = new List<String>() { DivisorLabel.ToLower() }
        });
        this.AddInputs(inputs);

        var outputs = new List<OutputConfiguration>();
        outputs.Add(new OutputConfiguration()
        {
            label = QuotientLabel,
            valueType = ValueType.Number,
            outputType = OutputType.Computed,
            classes = new List<String>() { QuotientLabel.ToLower() }
        });
        outputs.Add(new OutputConfiguration()
        {
            label = RemainderLabel,
            valueType = ValueType.Number,
            outputType = OutputType.Computed,
            classes = new List<String>() { RemainderLabel.ToLower() }
        });

        this.AddOutputs(outputs);

        this.AddToClassList("modulo_node");
    }

    // Warn : this will throw the cryptic 'ArgumentNullException: Value cannot be null. Parameter name: e' if the 'quotient' class is missing
    public Knob GetQuotientOutputKnob()
    {
        return (Knob)this.Query<VisualElement>(null, QuotientLabel.ToLower()).First().Query<VisualElement>(null, "node_output_knob");
    }

    public Knob GetRemainderOutputKnob()
    {
        return (Knob)this.Query<VisualElement>(null, RemainderLabel.ToLower()).First().Query<VisualElement>(null, "node_output_knob");
    }

    public Knob GetDividendInputKnob()
    {
        return (Knob)this.Query<VisualElement>(null, DividendLabel.ToLower()).First()?.Query<VisualElement>(null, "node_input_knob");
    }

    public Knob GetDivisorInputKnob()
    {
        return (Knob)this.Query<VisualElement>(null, DivisorLabel.ToLower()).First()?.Query<VisualElement>(null, "node_input_knob");
    }

    public Knob GetKnob(String label)
    {
        switch(label)
        {
            case QuotientLabel:

                return GetQuotientOutputKnob();

            case RemainderLabel:

                return GetRemainderOutputKnob();

            case DividendLabel:

                return GetDividendInputKnob();

            case DivisorLabel:

                return GetDivisorInputKnob();

            default:
                throw new ArgumentException($"Unknown ModuloNode label {label}");
        }
    }
}

