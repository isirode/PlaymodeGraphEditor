using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Assets.Isirode.UIToolkitExtension.UI.Elements
{
    public struct MenuNodeDefinition
    {
        public String text;

        public List<MenuNodeDefinition> children;

        public EventCallback<PointerUpEvent> callback;

    }
}