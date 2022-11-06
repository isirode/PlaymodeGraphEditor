using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SimpleGraphBehaviour : MonoBehaviour
{
    private SimpleGraphController simpleController;

    private void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        simpleController = new SimpleGraphController(root);
        simpleController.Init();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Pause))
        {
            UnityEditor.EditorApplication.isPaused = true;
        }
    }

    void OnGUI()
    {
        simpleController.OnGUI();
    }
}
