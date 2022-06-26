using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CalculatorBehavior : MonoBehaviour
{
    private CalculatorController calculatorController;

    private void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        calculatorController = new CalculatorController(root);
        calculatorController.Init();
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
        calculatorController.OnGUI();
    }
}
