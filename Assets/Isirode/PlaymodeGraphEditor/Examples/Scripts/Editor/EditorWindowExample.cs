using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class QuickTool : EditorWindow
{
    // Warn : you cannot have the UI Toolkit Debugger and this window opened at the same time
    // Otherwise, on right clicks, it will show a context menu containing only "Inspect Element"
    // It does not bug on left click, but right click events are not send
    // See https://forum.unity.com/threads/right-click-only-brings-up-inspect-element.1314108/ maybe

    private CalculatorController calculatorController;

    [MenuItem("PlaymodeGraphEditor/Open calculator example _%#T")]
    public static void ShowWindow()
    {
        // Opens the window, otherwise focuses it if it’s already open.
        var window = GetWindow<QuickTool>();

        // Adds a title to the window.
        window.titleContent = new GUIContent("Calculator example");

        // Sets a minimum size to the window.
        window.minSize = new Vector2(500, 500);
    }

    private void OnEnable()
    {
        // Reference to the root of the window.
        var root = rootVisualElement;

        // Info : this is not the same resources as the one for the Calculator example
        // It is a different one since it need to be put inside a 'Resources' folder (maybe even a local 'Resources' folder, idk)

        // Loads and clones our VisualTree (eg. our UXML structure) inside the root.
        var quickToolVisualTree = Resources.Load<VisualTreeAsset>("CalculatorCanvasBis");
        quickToolVisualTree.CloneTree(root);

        // Loads and clones our VisualTree(eg.our UXML structure) inside the root.
        calculatorController = new CalculatorController(root);
        calculatorController.Init();

        root.RegisterCallback<PointerUpEvent>(ShowSomeMenu);
    }

    private void ShowSomeMenu(PointerUpEvent evt)
    {
        Debug.Log(nameof(ShowSomeMenu));
    }

    // Info : as long as there is no border in one of the elements inside the scroll container, we cant put zero
    private static float HEADER_HEIGTH = 0;// 20, 23 ?
    private Rect canvasWindowRect { get { return new Rect(0, HEADER_HEIGTH, position.width, position.height - HEADER_HEIGTH); } }
    private static Vector2 EDITOR_WINDOW_OFFSET = new Vector2(0f, -20f);

    // Info : correct method to use the GL methods
    void OnGUI()
    {
        // FIXME : does not work in the Editor Window
        // Debug.Log(nameof(OnGUI));
        /*if (Event.current.type != EventType.Repaint)
            return;*/

        // NodeEditorGUI.StartNodeGUI(true);

        // Warn : this is important, otherwise there will be errors and the lines will not be drawn silently
        // if (Event.current.type != EventType.Repaint && Event.current.type != EventType.Layout) return;
        //if (Event.current.type == EventType.Repaint) return;
        //if (Event.current.type == EventType.Layout) return;

        // Warn : do not use the code below with if (Event.current.type != EventType.Repaint) return;
        // Event.current.Use();
        // You will get the warning message Event.Use() should not be called for events of type repaint

        // TODO : set them once for all to avoid unecessary assigments
        calculatorController.editorWindowOffset = EDITOR_WINDOW_OFFSET;
        calculatorController.editorWindowRect = canvasWindowRect;
        // calculatorController.isEditorWindow = true;
        calculatorController.OnGUI();

        // Info : a test below
        // RTEditorGUI.DrawLine(Vector2.zero, new Vector2(400, 400), Color.red, null, 3);

        // Info : this is drawn behind the UI Elements as well
        /*
        Handles.BeginGUI();
        Handles.color = Color.red;
        Handles.DrawLine(new Vector3(0, 0), new Vector3(300, 300));
        Handles.EndGUI();
        */
    }
}
