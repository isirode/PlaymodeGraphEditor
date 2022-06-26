using UnityEngine.UIElements;

namespace Isirode.PlaymodeGraphEditor.Playmode.Nodes
{
    public class Knob : VisualElement
    {
        // Info : We are using a outer element and a inner element so that we can do an interaction visualization
        // The main component (itself) will have it width affecter by the flex layout so we cannot use it for that
        public VisualElement OuterElement { get; set; }
        public VisualElement InnnerElement { get; set; }

        public Knob()
        {
            OuterElement = new VisualElement();
            OuterElement.AddToClassList("knob_outer_element");

            InnnerElement = new VisualElement();
            InnnerElement.AddToClassList("knob_inner_element");

            OuterElement.Add(InnnerElement);
            Add(OuterElement);
        }

        // TODO : implement the XML instantiation

    }
}

