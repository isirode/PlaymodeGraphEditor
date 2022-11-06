using Assets.Isirode.UIToolkitExtension.UI.Elements;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace Isirode.PlaymodeGraphEditor.Playmode.Nodes
{
    public class BaseNode : VisualElement
    {
        public Label title;

        public bool isDragged = false;

        public VisualElement container;
        public VisualElement inputContainer;
        public VisualElement outputContainer;

        public VisualElement Fitter { get; set; }

        public VisualElement AbsoluteParent { get; set; }
        public Vector2 SelectExtraRect { get; set; }

        public string _label = string.Empty;
        public string Label
        {
            get
            {
                return _label;
            }
            set
            {
                this._label = value;
                this.title.text = this._label;

                if (!string.IsNullOrEmpty(this._label))
                {
                    this.title.style.display = DisplayStyle.Flex;
                }
                else
                {
                    title.style.display = DisplayStyle.None;
                }
            }
        }

        // FIXME : maybe move the enums elsewhere ?

        public enum ValueType
        {
            Int,
            Float,
            Number,
            String,
            Enum,
            Undefined
        }

        // FIXME : check if Enum.TryParse is actually badly implemented
        public static ValueType TryParseValueType(String value)
        {
            switch (value)
            {
                case "int":
                    return ValueType.Int;
                case "float":
                    return ValueType.Float;
                case "number":
                    return ValueType.Number;
                case "string":
                    return ValueType.String;
                case "enum":
                    return ValueType.Enum;
                default:
                    return ValueType.Undefined;
            }
        }

        public enum InputType
        {
            Raw,
            Connection,
            Interactive,// both raw or connection
            Undefined
        }

        public static InputType TryParseInputType(String value)
        {
            switch (value)
            {
                case "raw":
                    return InputType.Raw;
                case "connection":
                    return InputType.Connection;
                case "interactive":
                    return InputType.Interactive;
                default:
                    return InputType.Undefined;
            }
        }

        public struct KnobConfiguration
        {
            public string label;
            public ValueType valueType;// FIXME : is this necessary ?
            public bool isInput;
            // TODO : change this ?
            public Knob knob;
            public BaseNode node;
        }

        public string _inputs = string.Empty;
        /// <summary>
        /// Allow to pass inputs as a String
        /// WARN : it was made to be used inside the UI Builder, but due to Unity's limitations, it is not currently used there.<br/>
        /// Prefer using <see cref="AddInputs(List{InputConfiguration})"/> instead.
        /// </summary>
        public string Inputs
        {
            get
            {
                return _inputs;
            }
            set
            {

                this._inputs = value;

                if (String.IsNullOrEmpty(value))
                {
                    inputContainer.style.display = DisplayStyle.None;
                    inputContainer.Clear();

                    return;
                }

                var inputConfigurations = new List<InputConfiguration>();

                // TODO : add separator possibility

                // format (without spaces) : label, type, connection type, 
                // Format example : input1:text:raw,input2:text:connection:input3:enum:interactive:enum1;enum2
                var inputs = value.Split(',');

                foreach (var input in inputs)
                {
                    var configurations = input.Split(":");
                    // TODO : handle missing configuration

                    var labelText = configurations[0];

                    var valueTypeAsString = configurations[1];
                    var valueType = TryParseValueType(valueTypeAsString);

                    if (valueType == ValueType.Undefined)
                    {
                        throw new ArgumentException($"Value type {valueTypeAsString} is unknown.");
                    }

                    var inputTypeAsString = configurations[2];
                    var inputType = TryParseInputType(inputTypeAsString);

                    if (inputType == InputType.Undefined)
                    {
                        throw new ArgumentException($"Inputtype {inputTypeAsString} is unknown.");
                    }

                    var optionsAsString = configurations.Length >= 4 ? configurations[3] : String.Empty;

                    InputConfiguration inputConfiguration = new InputConfiguration()
                    {
                        label = labelText,
                        valueType = valueType,
                        inputType = inputType,
                        options = optionsAsString
                    };

                    inputConfigurations.Add(inputConfiguration);
                }

                AddInputs(inputConfigurations);

            }
        }

        public void AddInputs(List<InputConfiguration> inputConfigurations)
        {
            if (inputConfigurations.Count == 0)
            {
                inputContainer.style.display = DisplayStyle.None;
                inputContainer.Clear();
                return;
            }
            inputContainer.style.display = DisplayStyle.Flex;
            inputContainer.Clear();
            inputConfigurations.ForEach(AddInput);
        }

        public struct InputConfiguration
        {
            public string label;
            public ValueType valueType;
            public InputType inputType;
            public string options;
            public List<String> classes;
        }

        public void AddInput(InputConfiguration inputConfiguration)
        {
            var container = new VisualElement();
            container.AddToClassList("node_input_row_container");
            container.AddToClassList("node_value_row_container");
            if (inputConfiguration.classes != null && inputConfiguration.classes.Count != 0)
            {
                inputConfiguration.classes.ForEach(_class => container.AddToClassList(_class));
            }

            var label = new Label();
            label.text = inputConfiguration.label;
            label.AddToClassList("node_label");
            label.AddToClassList("node_input_label");
            label.AddToClassList("node_value_label");

            switch (inputConfiguration.inputType)
            {
                case InputType.Raw:

                    container.Add(label);

                    var field = GetValueField(inputConfiguration.valueType, "input", inputConfiguration.options);

                    // TODO : allow user to specify their callback handling
                    field.RegisterCallback<ChangeEvent<string>>((evt) =>
                    {
                        var valueChange = new ValueChange()
                        {
                            label = inputConfiguration.label,
                            valueType = inputConfiguration.valueType,
                            newValue = evt.newValue,
                            previousValue = evt.previousValue,
                            isInput = true,
                            knob = null,
                            node = this
                        };
                        this.NotifyInputChange(valueChange);
                    });
                    if (field is Select)
                    {
                        ((Select)field).OnChange += (sender, previousValue, newValue) =>
                        {
                            var valueChange = new ValueChange()
                            {
                                label = inputConfiguration.label,
                                valueType = inputConfiguration.valueType,
                                newValue = newValue,
                                previousValue = previousValue,
                                isInput = true,
                                knob = null,
                                node = this
                            };
                            this.NotifyInputChange(valueChange);
                        };
                    }

                    container.Add(field);

                    inputContainer.Add(container);

                    break;
                case InputType.Connection:
                    var knob = new Knob();
                    knob.AddToClassList("node_input_knob");
                    knob.AddToClassList("node_knob");

                    var knobConfiguration = new KnobConfiguration
                    {
                        label = inputConfiguration.label,
                        valueType = inputConfiguration.valueType,
                        isInput = true,
                        knob = knob,
                        node = this
                    };
                    knob.RegisterCallback<PointerDownEvent, KnobConfiguration>(PointerDownOnKnob, knobConfiguration);
                    knob.RegisterCallback<PointerUpEvent, KnobConfiguration>(PointerUpOnKnob, knobConfiguration);

                    // button.RegisterCallback<PointerUpEvent, string>(SelectOption, option);

                    container.Add(knob);
                    container.Add(label);
                    inputContainer.Add(container);
                    break;
                case InputType.Interactive:
                    throw new NotImplementedException("Interactive input type is not implemented yet.");
                case InputType.Undefined:
                    throw new ArgumentException($"Input type is unknown.");
            }
        }

        public List<Knob> GetInputKnobs()
        {
            return this.Query<Knob>(null, "node_input_knob").ToList();
        }

        public enum OutputType
        {
            Raw,
            Computed,
            Interactive,// both raw or computed
            Undefined
        }

        public static OutputType TryParseOutputType(String value)
        {
            switch (value)
            {
                case "raw":
                    return OutputType.Raw;
                case "computed":
                    return OutputType.Computed;
                case "interactive":
                    return OutputType.Interactive;
                default:
                    return OutputType.Undefined;
            }
        }

        public string _outputs = string.Empty;
        /// <summary>
        /// Allow to pass inputs as a String
        /// WARN : it was made to be used inside the UI Builder, but due to Unity's limitations, it is not currently used there.<br/>
        /// Prefer using <see cref="AddOutputs(List{OutputConfiguration})"/> instead.
        /// </summary>
        public string Outputs
        {
            get
            {
                return _outputs;
            }
            set
            {

                this._outputs = value;

                if (String.IsNullOrEmpty(value))
                {
                    outputContainer.style.display = DisplayStyle.None;
                    outputContainer.Clear();

                    return;
                }

                var outputConfigurations = new List<OutputConfiguration>();

                // TODO : add separator possibility

                // format (without spaces) : label, type, connection type, 
                // Format example : output1:text:raw,output2:text:computed:output3:enum:interactive:enum1;enum2

                var outputs = value.Split(',');

                foreach (var output in outputs)
                {
                    var configurations = output.Split(":");

                    var labelText = configurations[0];

                    var valueTypeAsString = configurations[1];
                    var valueType = TryParseValueType(valueTypeAsString);

                    if (valueType == ValueType.Undefined)
                    {
                        throw new ArgumentException($"Value type {valueTypeAsString} is unknown.");
                    }

                    var outputTypeAsString = configurations[2];
                    var outputType = TryParseOutputType(outputTypeAsString);

                    if (outputType == OutputType.Undefined)
                    {
                        throw new ArgumentException($"Output type {outputTypeAsString} is unknown.");
                    }

                    var optionsAsString = configurations.Length >= 4 ? configurations[3] : String.Empty;

                    OutputConfiguration outputConfiguration = new OutputConfiguration()
                    {
                        label = labelText,
                        valueType = valueType,
                        outputType = outputType,
                        options = optionsAsString
                    };

                    outputConfigurations.Add(outputConfiguration);
                }

                AddOutputs(outputConfigurations);
            }
        }

        public struct OutputConfiguration
        {
            public string label;
            public ValueType valueType;
            public OutputType outputType;
            public string options;
            public List<String> classes;
        }

        public void AddOutputs(List<OutputConfiguration> outputConfigurations)
        {
            if (outputConfigurations.Count == 0)
            {
                outputContainer.style.display = DisplayStyle.None;
                outputContainer.Clear();
                return;
            }
            outputContainer.style.display = DisplayStyle.Flex;
            outputContainer.Clear();
            outputConfigurations.ForEach(AddOutput);
        }

        public void AddOutput(OutputConfiguration outputConfiguration)
        {
            var container = new VisualElement();
            container.AddToClassList("node_output_row_container");
            container.AddToClassList("node_value_row_container");
            if (outputConfiguration.classes != null && outputConfiguration.classes.Count != 0)
            {
                outputConfiguration.classes.ForEach(_class => container.AddToClassList(_class));
            }

            var label = new Label();
            label.text = outputConfiguration.label;
            label.AddToClassList("node_label");
            label.AddToClassList("node_output_label");
            label.AddToClassList("node_value_label");

            var knob = new Knob();
            knob.AddToClassList("node_output_knob");
            knob.AddToClassList("node_knob");

            var knobConfiguration = new KnobConfiguration
            {
                label = outputConfiguration.label,
                valueType = outputConfiguration.valueType,
                isInput = false,
                knob = knob,
                node = this
            };

            knob.RegisterCallback<PointerDownEvent, KnobConfiguration>(PointerDownOnKnob, knobConfiguration);
            knob.RegisterCallback<PointerUpEvent, KnobConfiguration>(PointerUpOnKnob, knobConfiguration);

            // Warn : here we add elements in reverse order
            container.Add(knob);

            switch (outputConfiguration.outputType)
            {
                case OutputType.Raw:

                    var field = GetValueField(outputConfiguration.valueType, "output", outputConfiguration.options);

                    // TODO : allow user to specify their callback handling
                    field.RegisterCallback<ChangeEvent<string>>((evt) =>
                    {
                        var valueChange = new ValueChange()
                        {
                            label = outputConfiguration.label,
                            valueType = outputConfiguration.valueType,
                            newValue = evt.newValue,
                            previousValue = evt.previousValue,
                            isInput = false,
                            knob = knob,
                            node = this
                        };
                        this.NotifyOutputChange(valueChange);
                    });
                    if (field is Select)
                    {
                        ((Select)field).OnChange += (sender, previousValue, newValue) =>
                        {
                            var valueChange = new ValueChange()
                            {
                                label = outputConfiguration.label,
                                valueType = outputConfiguration.valueType,
                                newValue = newValue,
                                previousValue = previousValue,
                                isInput = false,
                                knob = knob,
                                node = this
                            };
                            this.NotifyOutputChange(valueChange);
                        };
                    }

                    container.Add(field);

                    break;

                case OutputType.Computed:

                    // do nothing

                    // Warn : user need to implement their own logic when they implement computed outputs

                    break;

                case OutputType.Interactive:
                    throw new NotImplementedException("Interactive output type is not implemented yet.");

                case OutputType.Undefined:
                    throw new ArgumentException($"Output type is unknown.");
            }

            container.Add(label);
            outputContainer.Add(container);
        }

        public List<Knob> GetOutputKnobs()
        {
            return this.Query<Knob>(null, "node_output_knob").ToList();
        }

        public void PreventDefaultOnPointerDownOnInput(PointerDownEvent evt, VisualElement visualElement)
        {
            evt.StopPropagation();

            // Important : we take the focus because PreventDefault will stop the focus, which will make the element unusable
            visualElement.Focus();
        }

        // TODO : add name so that they can be identified, maybe an array as well

        public VisualElement GetValueField(ValueType valueType, String nodeType, String options)
        {
            var nodeTypePrefix = $"node_{nodeType}";

            switch (valueType)
            {
                case ValueType.Int:
                    var intTextField = new TextField();
                    intTextField.AddToClassList($"node_textfield");
                    intTextField.AddToClassList($"node_int_textfield");
                    intTextField.AddToClassList($"{nodeTypePrefix}_textfield");
                    intTextField.AddToClassList($"{nodeTypePrefix}_int_textfield");

                    intTextField.value = "1";

                    intTextField.RegisterCallback<PointerDownEvent, VisualElement>(PreventDefaultOnPointerDownOnInput, intTextField);

                    return intTextField;

                case ValueType.Float:

                    var floatTextField = new TextField();
                    floatTextField.AddToClassList($"node_textfield");
                    floatTextField.AddToClassList($"node_float_textfield");
                    floatTextField.AddToClassList($"{nodeTypePrefix}_textfield");
                    floatTextField.AddToClassList($"{nodeTypePrefix}_float_textfield");

                    floatTextField.value = "1.00";

                    floatTextField.RegisterCallback<PointerDownEvent, VisualElement>(PreventDefaultOnPointerDownOnInput, floatTextField);

                    return floatTextField;

                case ValueType.Number:

                    var numberTextField = new TextField();
                    numberTextField.AddToClassList($"node_textfield");
                    numberTextField.AddToClassList($"node_number_textfield");
                    numberTextField.AddToClassList($"{nodeTypePrefix}_textfield");
                    numberTextField.AddToClassList($"{nodeTypePrefix}_number_textfield");

                    numberTextField.value = "1.00";

                    numberTextField.RegisterCallback<PointerDownEvent, VisualElement>(PreventDefaultOnPointerDownOnInput, numberTextField);

                    return numberTextField;

                case ValueType.String:

                    var stringTextField = new TextField();
                    stringTextField.AddToClassList($"node_textfield");
                    stringTextField.AddToClassList($"node_string_textfield");
                    stringTextField.AddToClassList($"{nodeTypePrefix}_textfield");
                    stringTextField.AddToClassList($"{nodeTypePrefix}_string_textfield");

                    stringTextField.value = "type here";

                    stringTextField.RegisterCallback<PointerDownEvent, VisualElement>(PreventDefaultOnPointerDownOnInput, stringTextField);

                    return stringTextField;

                case ValueType.Enum:

                    var select = new Select();
                    select.AddToClassList($"{nodeTypePrefix}_select");
                    select.AddToClassList($"node_select");

                    select.OptionsSeparator = "-";

                    if (SelectExtraRect != null)
                    {
                        select.ExtraRect = SelectExtraRect;
                    }

                    if (AbsoluteParent != null)
                    {
                        select.AbsoluteParent = AbsoluteParent;
                    }
                    select.InstantiateOptions = true;

                    select.Options = options;

                    select.RegisterCallback<PointerDownEvent, VisualElement>(PreventDefaultOnPointerDownOnInput, select);

                    return select;

                case ValueType.Undefined:
                    throw new ArgumentException($"Value type is unknown.");
            }

            throw new Exception("This should not happen");
        }

        public BaseNode()
        {
            AddToClassList("node");

            title = new Label();
            title.text = string.Empty;
            title.style.display = DisplayStyle.None;
            title.AddToClassList("node_title");
            title.AddToClassList("node_label");

            Add(title);

            container = new VisualElement();
            container.AddToClassList("node_container");

            inputContainer = new VisualElement();
            inputContainer.AddToClassList("node_input_container");

            outputContainer = new VisualElement();
            outputContainer.AddToClassList("node_output_container");

            container.Add(inputContainer);
            container.Add(outputContainer);

            Add(container);

            // Info : we do not display them at first, so that if not value are passed, they are not displayed
            inputContainer.style.display = DisplayStyle.None;
            outputContainer.style.display = DisplayStyle.None;

            // TODO : rename those so that they have a clear purpose
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
        }

        #region "Dragging"

        public delegate void OnStartDraggingNodeEvent(object sender, PointerDownEvent evt);
        public event OnStartDraggingNodeEvent OnStartDraggingNode;

        private Vector2 pointerDownLocalPosition;

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (Fitter == null) return;

            if (evt.button == (int)MouseButton.LeftMouse)
            {
                //Set tracking variables
                isDragged = true;
                // m_OriginalSlot = originalSlot;

                pointerDownLocalPosition = new Vector2(evt.localPosition.x, evt.localPosition.y);

                OnStartDraggingNode?.Invoke(this, evt);
            }
        }

        // TODO : fix the fact we are moving incorrectly
        // TODO : implement Ctrl + move to move by steps
        private void OnPointerMove(PointerMoveEvent evt)
        {
            // Info : not working, parent (CanvasController) still receive the event
            evt.PreventDefault();

            if (Fitter == null) return;

            //Only take action if the player is dragging an item around the screen
            if (!isDragged)
            {
                return;
            }

            var position = Fitter.WorldToLocal(evt.position);

            // TODO : is this better ?
            // var position2 = this.WorldToLocal(evt.position);

            // Warn : this will center the element with the center of the element
            //this.style.top = position.y - this.layout.height / 2;
            //this.style.left = position.x - this.layout.width / 2;

            // not working, but almost (cannot drag up if click on title)
            //this.style.top = this.style.top.value.value - evt.deltaPosition.y;
            //this.style.left = this.style.left.value.value + evt.deltaPosition.x;

            // Info : this allow to maintain the relative position
            this.style.top = position.y - pointerDownLocalPosition.y;
            this.style.left = position.x - pointerDownLocalPosition.x;
        }

        public delegate void OnStopDraggingNodeEvent(BaseNode sender, PointerUpEvent evt);
        public event OnStopDraggingNodeEvent OnStopDraggingNode;

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (evt.button == (int)MouseButton.LeftMouse)
            {
                if (Fitter == null) return;

                if (!isDragged)
                {
                    return;
                }

                isDragged = false;

                OnStopDraggingNode?.Invoke(this, evt);
            }
        }

        private void OnPointerLeave(PointerLeaveEvent evt)
        {
            // Info : in case there is a bug, we do this
            if (isDragged)
            {
                isDragged = false;
            }
        }

        #endregion

        public delegate void OnPointerDownOnKnobEvent(object sender, PointerDownEvent evt, KnobConfiguration knobConfiguration);
        public event OnPointerDownOnKnobEvent OnPointerDownOnKnob;
        private void PointerDownOnKnob(PointerDownEvent evt, KnobConfiguration knobConfiguration)
        {
            evt.StopPropagation();

            OnPointerDownOnKnob?.Invoke(this, evt, knobConfiguration);
        }

        public delegate void OnPointerUpOnKnobEvent(object sender, PointerUpEvent evt, KnobConfiguration knobConfiguration);
        public event OnPointerUpOnKnobEvent OnPointerUpOnKnob;
        private void PointerUpOnKnob(PointerUpEvent evt, KnobConfiguration knobConfiguration)
        {
            evt.StopPropagation();

            OnPointerUpOnKnob?.Invoke(this, evt, knobConfiguration);
        }

        public struct ValueChange
        {
            public string label;
            public ValueType valueType;
            public object newValue;
            public object previousValue;
            public bool isInput;
            public Knob knob;
            public BaseNode node;
        }

        public delegate void OnOutputChangeEvent(BaseNode sender, ValueChange valueChange);
        public event OnOutputChangeEvent OnOutputChange;
        protected void NotifyOutputChange(ValueChange valueChange)
        {
            // Debug.Log(nameof(NotifyOutputChange));

            OnOutputChange?.Invoke(this, valueChange);
        }

        public delegate void OnInputChangeEvent(BaseNode sender, ValueChange valueChange);
        public event OnInputChangeEvent OnInputChange;
        protected void NotifyInputChange(ValueChange valueChange)
        {
            // Debug.Log(nameof(NotifyInputChange));

            OnInputChange?.Invoke(this, valueChange);
        }

        public virtual void ConnectedInputChanged(ValueChange valueChange, KnobConfiguration knobConfiguration)
        {
            // do nothing
        }

        #region UXML

        [Preserve]
        public new class UxmlFactory : UxmlFactory<BaseNode, UxmlTraits> { }
        [Preserve]
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            readonly UxmlStringAttributeDescription Label = new UxmlStringAttributeDescription { name = "label" };
            readonly UxmlStringAttributeDescription Inputs = new UxmlStringAttributeDescription { name = "inputs" };
            readonly UxmlStringAttributeDescription Outputs = new UxmlStringAttributeDescription { name = "outputs" };

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                ((BaseNode)ve).Label = Label.GetValueFromBag(bag, cc);
                ((BaseNode)ve).Inputs = Inputs.GetValueFromBag(bag, cc);
                ((BaseNode)ve).Outputs = Outputs.GetValueFromBag(bag, cc);
            }

        }

        #endregion
    }
}