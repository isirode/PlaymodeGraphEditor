using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SimpleController : CanvasController
{

    private void Awake()
    {
        Init();

        selectExtraRect = new Vector2(0, -3);

        // FIXME : this is not correct
        menu.Clear();

        menuDefinitionList = new List<MenuNodeDefinition>()
        {
            new MenuNodeDefinition()
            {
                text = "Nodes",
                callback = null,
                children = new List<MenuNodeDefinition>()
                {
                    new MenuNodeDefinition()
                    {
                        text = "CompleteNode",
                        callback = AddCompleteNode
                    },
                    new MenuNodeDefinition()
                    {
                        text = "OneOutput",
                        callback = AddOneOutputNode
                    }
                }
            }
        };
        foreach (var menuNode in menuDefinitionList)
        {
            var menuItem = new MenuNode(menuNode);
            menu.Add(menuItem);
            menuNodes.Add(menuItem);
        }
    }

    private void AddCompleteNode(PointerUpEvent evt)
    {
        Debug.Log(nameof(AddCompleteNode));

        var baseNode = new BaseNode();
        // FIXME : we are force to put it before the inputs / outputs assignment, otherwise it will be null when instantiating Selects
        baseNode.AbsoluteParent = absoluteParent;
        baseNode.SelectExtraRect = selectExtraRect;
        baseNode.Label = "CompleteNode";
        baseNode.Inputs = "input1:string:connection,input2:string:connection,input3:enum:raw:option1-option2-option3,input4:int:raw,input3:enum:raw:option1-option2-option3";
        baseNode.Outputs = "output1:string:raw,output2:string:computed,output3:enum:raw:option1-option2-option3,output4:int:raw";

        baseNode.style.position = Position.Absolute;

        // FIXME : replace by TargetToAddElementAndClickOn ?
        var position = fitter.WorldToLocal(evt.position);

        baseNode.style.top = position.y;
        baseNode.style.left = position.x;

        TargetToAddElementAndClickOn.Add(baseNode);

        baseNode.Fitter = fitter;

        baseNodes.Add(baseNode);

        SetupKnobCallbacks(baseNode);
        SetupNodeDraggingCallbacks(baseNode);
    }

    private void AddOneOutputNode(PointerUpEvent evt)
    {
        Debug.Log(nameof(AddOneOutputNode));

        var baseNode = new BaseNode();
        // FIXME : we are force to put it before the inputs / outputs assignment, otherwise it will be null when instantiating Selects
        baseNode.AbsoluteParent = absoluteParent;
        baseNode.SelectExtraRect = selectExtraRect;
        baseNode.Label = "OneOutputNode";
        baseNode.Inputs = "";
        baseNode.Outputs = "output1:string:raw";

        baseNode.style.position = Position.Absolute;

        // FIXME : replace by TargetToAddElementAndClickOn ?
        var position = fitter.WorldToLocal(evt.position);

        baseNode.style.top = position.y;
        baseNode.style.left = position.x;

        TargetToAddElementAndClickOn.Add(baseNode);

        baseNode.Fitter = fitter;

        baseNodes.Add(baseNode);

        SetupKnobCallbacks(baseNode);
        SetupNodeDraggingCallbacks(baseNode);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Pause))
        {
            UnityEditor.EditorApplication.isPaused = true;
        }
        // Info : this is because TextField do not display their value if the scale is not exactly 1
        if (Input.GetKeyDown(KeyCode.A))
        {
            ResetScaleToOne();
        }
    }

}
