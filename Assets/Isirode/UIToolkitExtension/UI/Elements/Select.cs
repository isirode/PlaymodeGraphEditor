using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

// TODO : implement Unity's ChangeEvent

namespace Assets.Isirode.UIToolkitExtension.UI.Elements
{
    public class Select : VisualElement
    {
        // TODO : allow to pass the separator as an XML input, or a text input (maybe)
        public string OptionsSeparator { get; set; } = ",";

        public string _options = string.Empty;
        public string Options
        {
            get
            {
                return _options;
            }
            set
            {
                this._options = value;

                var options = _options.Split(OptionsSeparator);
                selectionLabel.text = options[0];

                if (!InstantiateOptions)
                {
                    GenerateOptions();
                }
            }
        }

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
                this.presentationLabel.text = this._label;

                if (!string.IsNullOrEmpty(this._label))
                {
                    this.presentationLabel.style.display = DisplayStyle.Flex;
                }
                else
                {
                    presentationLabel.style.display = DisplayStyle.None;
                }
            }
        }

        // Label
        public Label presentationLabel;

        // Contains the selected option and the options
        public VisualElement selectionContainer;

        // Selected option
        public Label selectionLabel;

        // Warn
        // We do that because there is no z-index, so this will be drawn behind other elements, and will not be visible
        // Furthermore, once the mouse is over another element, it will trigger a pointer leave event
        // See https://forum.unity.com/threads/z-index-support.736781/
        public VisualElement AbsoluteParent;
        // Warn
        // This is important because there will be a gap between the label and the options when AbsoluteParent is present
        // And border & padding are not part of the contentRect (it seem so)
        public Vector2 ExtraRect { get; set; }

        // Info : this is important because in case where we zoom out, the hidden elements (they are indicated as hidden in the debugger)
        // are then displayed, but weirdly
        // TODO : allow to set it through XML
        public bool InstantiateOptions { get; set; }

        // Contains the options
        public VisualElement optionsContainer;

        // Option changed event
        public delegate void OnChangeEvent(object sender, string previousValue, string newValue);
        public event OnChangeEvent OnChange;

        // TODO : display the options when getting Focus
        // TODO : allow to navigate the options

        public Select()
        {
            AddToClassList("select_root");

            // Container for the selection
            selectionContainer = new VisualElement();
            selectionContainer.AddToClassList("select_selection_container");

            // Label
            presentationLabel = new Label();
            presentationLabel.text = string.Empty;
            presentationLabel.style.display = DisplayStyle.None;
            presentationLabel.AddToClassList("select_label");
            presentationLabel.AddToClassList("select_presentation_label");

            // Selected option
            selectionLabel = new Label();
            selectionLabel.text = string.Empty;
            selectionLabel.AddToClassList("select_label");
            selectionLabel.AddToClassList("select_selection_label");

            // Container for the dropdown of options
            optionsContainer = new VisualElement();
            optionsContainer.AddToClassList("select_options_container");
            optionsContainer.style.position = Position.Absolute;
            optionsContainer.style.visibility = Visibility.Hidden;

            Add(presentationLabel);
            selectionContainer.Add(selectionLabel);
            // Info : This is not always true, if AbsoluteParent is present, the parent will be AbsoluteParent
            selectionContainer.Add(optionsContainer);
            Add(selectionContainer);

            selectionContainer.RegisterCallback<PointerUpEvent>(ShowOptions);
            selectionContainer.RegisterCallback<PointerLeaveEvent>(HideOptions);

            optionsContainer.RegisterCallback<PointerLeaveEvent>(HideOptionsWhenLeaveOptionsContainer);
        }

        public Select(string options) : this()
        {
            this.Options = options;
        }

        protected void ShowOptions(PointerUpEvent evt)
        {
            // Debug.Log(nameof(ShowOptions));

            if (InstantiateOptions)
            {
                GenerateOptions();
            }

            // TODO : Should close if click somewhere else

            if (AbsoluteParent != null)
            {
                // Warn : we need to clear it, we probably want to reuse this element, if we do not clear it, it might have more content that intended
                AbsoluteParent.Clear();

                optionsContainer.style.visibility = Visibility.Visible;

                // Info : we do not need to bring to front, because AbsoluteParent should be at the end of the draw calls
                // AbsoluteParent.BringToFront();

                AbsoluteParent.style.position = Position.Absolute;
                // Info : worlBound is actually accurate while LocalToWorld is not
                // AbsoluteParent.style.top = this.LocalToWorld(this.layout.position + this.layout.size).y;
                // AbsoluteParent.style.left = this.LocalToWorld(this.layout.position).x;
                AbsoluteParent.style.top = (this.worldBound.position + this.worldBound.size).y;
                AbsoluteParent.style.left = this.worldBound.position.x;

                optionsContainer.style.minWidth = this.selectionContainer.layout.width;

                AbsoluteParent.Add(optionsContainer);
            }
            else
            {
                optionsContainer.style.visibility = Visibility.Visible;
                optionsContainer.style.top = this.selectionContainer.layout.height;
                optionsContainer.style.left = 0;
                optionsContainer.style.minWidth = this.selectionContainer.layout.width;
            }
        }

        private void HideOptions(PointerLeaveEvent evt)
        {
            // Debug.Log(nameof(HideOptions));
            // Hide options
            if (AbsoluteParent == null)
            {
                HideOptions();
            }
            else
            {
                // Warn : we cannot use the code below
                // else if !optionsContainer.contentRect.Contains(optionsContainer.WorldToLocal(evt.position))
                // We need to calcultate an extra rect for the padding & border of the element (probably margin also)
                // Otherwise we will not be able to pick an option
                var rect = new Rect(optionsContainer.contentRect);
                rect.y += ExtraRect.y;
                rect.height += Mathf.Abs(ExtraRect.y) * 2;
                rect.x += ExtraRect.x;
                rect.width += Mathf.Abs(ExtraRect.x) * 2;

                if (!rect.Contains(optionsContainer.WorldToLocal(evt.position)))
                {
                    HideOptions();
                }
            }
        }

        private void HideOptionsWhenLeaveOptionsContainer(PointerLeaveEvent evt)
        {
            // Debug.Log(nameof(HideOptionsWhenLeaveOptionsContainer));

            if (AbsoluteParent != null && !selectionContainer.contentRect.Contains(selectionContainer.WorldToLocal(evt.position)))
            {
                HideOptions();
            }
        }

        private void SelectOption(PointerUpEvent evt, string option)
        {
            // Debug.Log(nameof(SelectOption));
            // Change label and hide options
            string oldValue = selectionLabel.text;
            selectionLabel.text = option;
            HideOptions();
            OnChange?.Invoke(this, oldValue, option);
        }

        private void HideOptions()
        {
            optionsContainer.style.visibility = Visibility.Hidden;
            if (AbsoluteParent != null)
            {
                AbsoluteParent.Clear();
            }
            if (InstantiateOptions)
            {
                optionsContainer.Clear();
            }
        }

        private void GenerateOptions()
        {
            var options = _options.Split(OptionsSeparator);

            optionsContainer.Clear();

            foreach (var option in options)
            {
                var button = new Button();
                button.text = option;
                button.AddToClassList("select_option");

                button.RegisterCallback<PointerUpEvent, string>(SelectOption, option);

                optionsContainer.Add(button);
            }
        }

        #region UXML

        [Preserve]
        public new class UxmlFactory : UxmlFactory<Select, UxmlTraits> { }

        [Preserve]
        public new class UxmlTraits : VisualElement.UxmlTraits
        {

            readonly UxmlStringAttributeDescription Options = new UxmlStringAttributeDescription { name = "options" };
            readonly UxmlStringAttributeDescription Label = new UxmlStringAttributeDescription { name = "label" };

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                ((Select)ve).Options = Options.GetValueFromBag(bag, cc);
                ((Select)ve).Label = Label.GetValueFromBag(bag, cc);
            }

        }

        #endregion
    }
}