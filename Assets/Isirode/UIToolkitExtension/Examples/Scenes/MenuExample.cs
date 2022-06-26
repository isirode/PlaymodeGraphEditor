using Assets.Isirode.UIToolkitExtension.UI.Elements;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuExample : MonoBehaviour
{
    private VisualElement root;
    private VisualElement container;

    private VisualElement menu;
    private List<MenuNodeDefinition> menuList;
    private List<MenuNode> menuItems = new List<MenuNode>();

    private void Awake()
    {

        root = GetComponent<UIDocument>().rootVisualElement;
        container = root.Query<VisualElement>("Container");

        // FIXME : we probably want to instantiate it
        menu = root.Query<VisualElement>("Menu");
        menu.style.position = Position.Absolute;
        menu.style.visibility = Visibility.Hidden;
        // menu.style.display = DisplayStyle.None;
        menu.RegisterCallback<PointerLeaveEvent>(HideMenu);

        // FIXME : this is not correct
        menu.Clear();

        menuList = new List<MenuNodeDefinition>()
        {
            new MenuNodeDefinition()
            {
                text = "First choice",
                callback = null,
                children = new List<MenuNodeDefinition>()
                {
                    new MenuNodeDefinition()
                    {
                        text = "Second choice",
                        callback = SecondChoice
                    },
                    new MenuNodeDefinition()
                    {
                        text = "Third choice",
                        callback = ThirdChoice
                    }
                }
            },
            new MenuNodeDefinition()
            {
                text = "Other choice",
                callback = null,
                children = new List<MenuNodeDefinition>()
                {
                    new MenuNodeDefinition()
                    {
                        text = "Second choice",
                        callback = SecondChoice
                    },
                    new MenuNodeDefinition()
                    {
                        text = "Third choice",
                        callback = ThirdChoice
                    }
                }
            },
            new MenuNodeDefinition()
            {
                text = "Another choice",
                callback = null,
            }
        };

        foreach (var menuNode in menuList)
        {
            var menuItem = new MenuNode(menuNode);
            menu.Add(menuItem);
            menuItems.Add(menuItem);
        }

        container.RegisterCallback<PointerUpEvent>(ShowMenu);
    }

    private void ShowMenu(PointerUpEvent evt)
    {
        Debug.Log(nameof(ShowMenu));

        if (evt.button == ((int)MouseButton.LeftMouse))
        {
            return;
        }

        menu.style.top = evt.position.y - menu.layout.height / 2;
        menu.style.left = evt.position.x - menu.layout.width / 2;
        menu.style.visibility = Visibility.Visible;
        // menu.style.display = DisplayStyle.Flex;
    }

    private void HideMenu(PointerLeaveEvent evt)
    {
        Debug.Log(nameof(HideMenu));

        if (this.menuItems.Exists(x => x.SelfOrChildrenContains(evt.position)))
        {
            Debug.Log("Over children");
            return;
        }

        HideMenu();
    }

    private void HideMenu()
    {
        menu.style.visibility = Visibility.Hidden;
    }

    private void SecondChoice(PointerUpEvent evt)
    {
        Debug.Log(nameof(SecondChoice));
    }
    private void ThirdChoice(PointerUpEvent evt)
    {
        Debug.Log(nameof(ThirdChoice));
    }

}
