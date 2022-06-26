using Assets.Isirode.UIToolkitExtension.UI.Elements;
using UnityEngine;
using UnityEngine.UIElements;

public class SelectProgrammaticExample : MonoBehaviour
{

    private VisualElement root;
    private VisualElement container;
    private VisualElement absoluteParent;

    private Select select;
    private TextField textField; 

    public bool InstantiateOptions = false;

    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        container = root.Query<VisualElement>("Container");

        absoluteParent = root.Query<VisualElement>("AbsoluteParent");
        // absoluteParent = container;

        select = new Select();
        select.AbsoluteParent = absoluteParent;
        select.Options = "option1,option2,option3,option4";
        select.ExtraRect = new Vector2(0, -4);
        select.InstantiateOptions = InstantiateOptions;

        textField = new TextField();
        textField.value = "Type here";

        container.Add(select);
        container.Add(textField);
    }

}
