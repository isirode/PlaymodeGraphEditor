using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace Assets.Isirode.UIToolkitExtension.UI.Elements
{
    public class MenuNode : VisualElement
    {
        public Button button;
        public VisualElement child;
        public List<MenuNode> children = new List<MenuNode>();

        public MenuNode(MenuNodeDefinition menuNode)
        {
            button = new Button();
            button.text = menuNode.text;

            button.AddToClassList("menu_node");

            Add(button);


            if (menuNode.callback != null)
            {
                button.RegisterCallback<PointerUpEvent>(menuNode.callback);
            }

            button.RegisterCallback<PointerDownEvent>(OnPointerDown);

            button.RegisterCallback<PointerOverEvent>(ShowChildren);
            button.RegisterCallback<PointerLeaveEvent>(HideChildren);

            child = new VisualElement();
            child.RegisterCallback<PointerLeaveEvent>(OnPointerLeaveChild);

            // Info : using display instead of visibility is important, otherwise, it will not look correct
            // child.style.visibility = Visibility.Hidden;
            child.style.display = DisplayStyle.None;

            child.style.paddingLeft = 3;

            Add(child);

            if (menuNode.children != null)
            {
                foreach (var node in menuNode.children)
                {
                    MenuNode menuItem = new MenuNode(node);
                    child.Add(menuItem);
                    children.Add(menuItem);
                }
            }
        }

        public bool SelfOrChildrenContains(Vector2 position)
        {
            return this.ContainsPoint(position) || this.child.ContainsPoint(position) || this.children.Exists(x => x.SelfOrChildrenContains(position));
        }

        private void ShowChildren(PointerOverEvent evt)
        {
            // Debug.Log(nameof(ShowChildren));

            child.style.position = Position.Absolute;

            var position = this.layout.position;

            // Info : absolute positionning is relative to the parent, so 0 in top is required
            child.style.top = 0;
            child.style.left = position.x + this.layout.width - 1;

            // child.style.visibility = Visibility.Visible;
            child.style.display = DisplayStyle.Flex;
        }

        private void HideChildren(PointerLeaveEvent evt)
        {
            // Debug.Log(nameof(HideChildren));

            if (child.worldBound.Contains(evt.position))
            {
                Debug.Log("Mouse " + evt.position + " over " + child.worldBound);
                return;
            }

            // child.style.visibility = Visibility.Hidden;
            child.style.display = DisplayStyle.None;

        }

        private void OnPointerLeaveChild(PointerLeaveEvent evt)
        {
            // Debug.Log(nameof(OnPointerLeaveChild));

            if (button.worldBound.Contains(evt.position))
            {
                Debug.Log("Mouse " + evt.position + " over " + button.worldBound);
                return;
            }

            // child.style.visibility = Visibility.Hidden;
            child.style.display = DisplayStyle.None;
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            // Debug.Log(nameof(OnPointerDown));
        }

        // TODO : find a way to instantiate the menu from the designer window
        #region UXML
        // TODO : fix this 
        // [Preserve]
        // public new class UxmlFactory : UxmlFactory<MenuItem, UxmlTraits> { }
        [Preserve]
        public new class UxmlTraits : VisualElement.UxmlTraits { }
        #endregion

    }
}