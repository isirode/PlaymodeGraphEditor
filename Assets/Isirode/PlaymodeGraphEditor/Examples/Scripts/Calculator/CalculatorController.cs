using Assets.Isirode.UIToolkitExtension.UI.Elements;
using Isirode.PlaymodeGraphEditor.Playmode;
using Isirode.PlaymodeGraphEditor.Playmode.Nodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static Isirode.PlaymodeGraphEditor.Playmode.Nodes.BaseNode;

// should not need anything else than the nodes definition and callbacks / connection tree traversal

public class CalculatorController : CanvasController
{
    [NonSerialized]
    private VisualElement nodeMenu;

    [NonSerialized]
    private List<MenuNodeDefinition> nodeMenuList;

    [NonSerialized]
    public List<MenuNode> nodeMenuNodes = new List<MenuNode>();

    [NonSerialized]
    private BaseNode selectedNode;

    private void Awake()
    {
        Init();

        Debug.Log(selectExtraRect);

        // Info : will go to
        // "C:\Users\username\AppData\LocalLow\DefaultCompany\PlaytimeGraphEditor\Graph\graph.json".
        filePath = Application.persistentDataPath + "/Graph/";

        this.OnConnectionAdded += HandleOnConnectionAdded;
        this.OnAllConnectionsOfKnobRemoved += HandleAllConnectionsOfKnobRemoved;

        // FIXME : this is not correct
        menu.Clear();

        menuDefinitionList = new List<MenuNodeDefinition>()
        {
            new MenuNodeDefinition()
            {
                text = "Inputs",
                callback = null,
                children = new List<MenuNodeDefinition>()
                {
                    new MenuNodeDefinition()
                    {
                        text = "Int",
                        callback = AddIntNode
                    },
                    new MenuNodeDefinition()
                    {
                        text = "Float",
                        callback = AddFloatNode
                    }
                }
            },
            new MenuNodeDefinition()
            {
                text = "Operations",
                callback = null,
                children = new List<MenuNodeDefinition>()
                {
                    new MenuNodeDefinition()
                    {
                        text = "Operation",
                        callback = AddComputationNode
                    },
                    new MenuNodeDefinition()
                    {
                        text = "Modulo",
                        callback = AddModuloNode
                    }
                }
            },
            new MenuNodeDefinition()
            {
                text = "Outputs",
                callback = null,
                children = new List<MenuNodeDefinition>()
                {
                    new MenuNodeDefinition()
                    {
                        text = "Display",
                        callback = AddDisplayNode
                    },
                    new MenuNodeDefinition()
                    {
                        text = "Export",
                        callback = AddExportNode
                    }
                }
            },
            new MenuNodeDefinition()
            {
                text = "Graph",
                callback = null,
                children = new List<MenuNodeDefinition>()
                {
                    new MenuNodeDefinition()
                    {
                        text = "Save",
                        callback = SaveGraph
                    },
                    new MenuNodeDefinition()
                    {
                        text = "Load",
                        callback = LoadGraph
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

        // Node menu

        nodeMenu = root.Query<VisualElement>("NodeMenu");

        nodeMenu.Clear();

        var nodeMenuDefinitionList = new List<MenuNodeDefinition>()
        {
            new MenuNodeDefinition()
            {
                text = "Remove",
                callback = RemoveNode,
            },
        };
        foreach (var menuNode in nodeMenuDefinitionList)
        {
            var menuItem = new MenuNode(menuNode);
            nodeMenu.Add(menuItem);
            nodeMenuNodes.Add(menuItem);
        }

        nodeMenu.style.visibility = Visibility.Hidden;
        // menu.style.display = DisplayStyle.None;
        nodeMenu.RegisterCallback<PointerLeaveEvent>(HideNodeMenu);


    }

    #region "Add Nodes"

    private void AddIntNode(PointerUpEvent evt)
    {
        Debug.Log(nameof(AddIntNode));

        var node = new IntNode(this.absoluteParent);

        node.OnOutputChange += HandleOutputChange;

        // TODO : Maybe move this into a common method ?
        // Seem slightly difficult because there is not combinaisons of types in method signatures and BaseNode and GuidProvider are differents
        node.Guid = Guid.NewGuid();

        SetupNode(node, evt);

    }

    private void AddFloatNode(PointerUpEvent evt)
    {
        Debug.Log(nameof(AddFloatNode));

        var node = new FloatNode(this.absoluteParent);

        node.OnOutputChange += HandleOutputChange;

        node.Guid = Guid.NewGuid();

        SetupNode(node, evt);
    }

    private void AddDisplayNode(PointerUpEvent evt)
    {
        Debug.Log(nameof(AddDisplayNode));

        var node = new DisplayNode(this.absoluteParent);

        node.Guid = Guid.NewGuid();

        SetupNode(node, evt);
    }

    private void AddExportNode(PointerUpEvent evt)
    {
        Debug.Log(nameof(AddExportNode));

        var node = new ExportNode(this.absoluteParent);

        node.Guid = Guid.NewGuid();

        SetupNode(node, evt);
    }

    private void AddComputationNode(PointerUpEvent evt)
    {
        Debug.Log(nameof(AddComputationNode));

        var node = new ComputationNode(this.absoluteParent, selectExtraRect);

        node.OnInputChange += HandleComputationInputChange;

        node.Guid = Guid.NewGuid();

        SetupNode(node, evt);
    }

    private void AddModuloNode(PointerUpEvent evt)
    {
        Debug.Log(nameof(AddComputationNode));

        var node = new ModuloNode(this.absoluteParent, selectExtraRect);

        node.Guid = Guid.NewGuid();

        SetupNode(node, evt);
    }

    // TODO : move this to CanvasController ?
    private void SetupNode(BaseNode node, PointerUpEvent evt)
    {
        // FIXME : replace by TargetToAddElementAndClickOn ?
        var position = fitter.WorldToLocal(evt.position);

        SetupNode(node, position);
    }

    private void SetupNode(BaseNode node, Vector2 position)
    {
        node.SelectExtraRect = selectExtraRect;

        node.style.position = Position.Absolute;

        node.style.top = position.y;
        node.style.left = position.x;

        TargetToAddElementAndClickOn.Add(node);

        node.Fitter = fitter;

        baseNodes.Add(node);

        node.OnStopDraggingNode += HandleStopDraggingNode;

        node.RegisterCallback<PointerUpEvent, BaseNode>(ShowNodeMenu, node);

        SetupKnobCallbacks(node);
        SetupNodeDraggingCallbacks(node);
    }

    private void RemoveNode(PointerUpEvent evt)
    {
        Debug.Log(nameof(RemoveNode));

        HideNodeMenu();

        if (selectedNode != null)
        {
            // We remove connections

            // We start by inputs
            var inputKnobs = selectedNode.GetInputKnobs();
            Connections.RemoveAll(x => inputKnobs.Contains(x.end.knob));

            // We get outputs
            var outputKnobs = selectedNode.GetOutputKnobs();
            var connectionsToBeRemoved = Connections.FindAll(RemoveAllByOutputKnobsPredicate(outputKnobs));

            // We remove the connections
            Connections.RemoveAll(RemoveAllByOutputKnobsPredicate(outputKnobs));

            // We refresh connected values
            connectionsToBeRemoved.ForEach(RefreshConnectionEndValues);

            // We remove the element at the end to avoid race conditions
            TargetToAddElementAndClickOn.Remove(selectedNode);
        }
        else
        {
            throw new Exception("Attempt to remove a node but no nodes were selected");
        }

        static Predicate<Connection> RemoveAllByOutputKnobsPredicate(List<Knob> outputKnobs)
        {
            return x => outputKnobs.Contains(x.start.knob);
        }
    }

    #endregion

    #region "Files"

    String filePath;
    String fileName = "graph.json";

    private void SaveGraph(PointerUpEvent evt)
    {
        Debug.Log(nameof(SaveGraph));

        var nodes = TargetToAddElementAndClickOn.Query<BaseNode>().ToList();
        if (nodes.Count == 0)
        {
            // TODO : inform the user
            Debug.LogWarning("No elements to save");
            return;
        }
        List<NodeDto> nodeDtos = new List<NodeDto>();
        nodes.ForEach(node =>
        {
            if (node is not GuidProvider) 
            {
                throw new Exception("Only nodes implementing GuidProvider can be saved.");
            }
            Guid guid = ((GuidProvider)node).Guid;
            Vector2 position = new Vector2(node.style.left.value.value, node.style.top.value.value);
            String nodeType = node.GetType().Name;
            String userStateObject = null;
            switch(node)
            {
                case FloatNode typedNode:

                    userStateObject = typedNode.Value.ToString();

                    break;

                case IntNode typedNode:

                    userStateObject = typedNode.Value.ToString();

                    break;

                case ComputationNode typedNode:

                    userStateObject = ((ComputationNode)node).GetOperationType().ToString();

                    break;
            }

            nodeDtos.Add(new NodeDto()
            {
                nodeType = nodeType,
                position = position,
                guid = guid.ToString(),
                userStateObject = userStateObject
            });
        });

        List<ConnectionDto> connectionDtos = new List<ConnectionDto>();
        Connections.ForEach(connection =>
        {
            if (connection.start.node is not GuidProvider || connection.end.node is not GuidProvider)
            {
                throw new Exception("Only nodes implementing GuidProvider can be saved.");
            }
            var startGuid = ((GuidProvider)connection.start.node).Guid;
            var startLabel = connection.start.label;
            var endGuid = ((GuidProvider)connection.end.node).Guid;
            var endLabel = connection.end.label;
            connectionDtos.Add(new ConnectionDto()
            {
                startGuid = startGuid.ToString(),
                startKnobLabel = startLabel,
                startValueType = connection.start.valueType,
                endGuid = endGuid.ToString(),
                endKnobLabel = endLabel,
                endValueType = connection.end.valueType
            });
        });

        var graph = new GraphDto()
        {
            nodes = nodeDtos,
            connections = connectionDtos
        };

        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }

        Debug.Log(graph);

        System.IO.File.WriteAllText(filePath + fileName, JsonUtility.ToJson(graph));
    }

    private void LoadGraph(PointerUpEvent evt)
    {
        Debug.Log(nameof(LoadGraph));

        // Load the graph
        GraphDto graph = JsonUtility.FromJson<GraphDto>(System.IO.File.ReadAllText(filePath + fileName));

        Debug.Log(graph);

        // Clear the current graph
        var nodes = TargetToAddElementAndClickOn.Query<BaseNode>().ToList();
        if (nodes.Count != 0)
        {
            // TODO : inform the user
            Debug.LogWarning("You are loading a saved graph but there is nodes on the current graph.");
        }

        Connections.Clear();
        TargetToAddElementAndClickOn.Clear();

        // We maintain a map so that we can use it for the connections
        Dictionary<Guid, BaseNode> dictionary = new Dictionary<Guid, BaseNode>();

        // Add the nodes
        graph.nodes.ForEach(nodeDto =>
        {
            switch(nodeDto.nodeType)
            {
                case nameof(IntNode):

                    var intNode = new IntNode(this.absoluteParent);

                    intNode.Guid = Guid.Parse(nodeDto.guid);

                    SetupNode(intNode, nodeDto.position);

                    if (!String.IsNullOrEmpty(nodeDto.userStateObject))
                    {
                        intNode.Value = int.Parse(nodeDto.userStateObject);
                    }

                    dictionary.Add(intNode.Guid, intNode);

                    break;

                case nameof(FloatNode):

                    var floatNode = new FloatNode(this.absoluteParent);

                    floatNode.Guid = Guid.Parse(nodeDto.guid);

                    SetupNode(floatNode, nodeDto.position);

                    if (!String.IsNullOrEmpty(nodeDto.userStateObject))
                    {
                        floatNode.Value = float.Parse(nodeDto.userStateObject);
                    }

                    dictionary.Add(floatNode.Guid, floatNode);

                    break;

                case nameof(ComputationNode):

                    var computationNode = new ComputationNode(this.absoluteParent, selectExtraRect);

                    computationNode.Guid = Guid.Parse(nodeDto.guid);

                    SetupNode(computationNode, nodeDto.position);

                    if (!String.IsNullOrEmpty(nodeDto.userStateObject))
                    {
                        computationNode.SetOperationType(nodeDto.userStateObject);
                    }

                    dictionary.Add(computationNode.Guid, computationNode);

                    break;

                case nameof(ModuloNode):

                    var moduloNode = new ModuloNode(this.absoluteParent, selectExtraRect);

                    moduloNode.Guid = Guid.Parse(nodeDto.guid);

                    SetupNode(moduloNode, nodeDto.position);

                    dictionary.Add(moduloNode.Guid, moduloNode);

                    break;

                case nameof(DisplayNode):

                    var displayNode = new DisplayNode(this.absoluteParent);

                    displayNode.Guid = Guid.Parse(nodeDto.guid);

                    SetupNode(displayNode, nodeDto.position);

                    dictionary.Add(displayNode.Guid, displayNode);

                    break;

                case nameof(ExportNode):

                    var exportNode = new ExportNode(this.absoluteParent);

                    exportNode.Guid = Guid.Parse(nodeDto.guid);

                    SetupNode(exportNode, nodeDto.position);

                    dictionary.Add(exportNode.Guid, exportNode);

                    break;
            }
        });

        // Add connections
        graph.connections.ForEach(connectionDto =>
        {
            // find nodes
            var startNode = dictionary[Guid.Parse(connectionDto.startGuid)];
            var endNode = dictionary[Guid.Parse(connectionDto.endGuid)];

            // find start knob
            Knob startKnob;

            switch(startNode)
            {
                case IntNode node:

                    startKnob = node.GetOutputKnobs().First();

                    break;

                case FloatNode node:

                    startKnob = node.GetOutputKnobs().First();

                    break;

                case ComputationNode node:

                    startKnob = node.GetOutputKnobs().First();

                    break;

                case ModuloNode node:

                    startKnob = node.GetKnob(connectionDto.startKnobLabel);

                    break;

                case DisplayNode node:

                    throw new ArgumentException($"{nameof(DisplayNode)} cannot be a start node");

                case ExportNode node:

                    throw new ArgumentException($"{nameof(ExportNode)} cannot be a start node");

                case null:

                    throw new ArgumentNullException("Start node was null");

                default:

                    throw new ArgumentException($"Node has a unknown type ({startNode.GetType().Name})");
            }

            // find end knob
            Knob endKnob;
            switch (endNode)
            {
                case IntNode node:

                    throw new ArgumentException($"{nameof(IntNode)} cannot be a end node");

                case FloatNode node:

                    throw new ArgumentException($"{nameof(FloatNode)} cannot be a end node");

                case ComputationNode node:

                    endKnob = node.GetInputKnob();

                    break;

                case ModuloNode node:

                    endKnob = node.GetKnob(connectionDto.endKnobLabel);

                    break;

                case DisplayNode node:

                    endKnob = node.GetInputKnobs().First();

                    break;

                case ExportNode node:

                    endKnob = node.GetInputKnobs().First();

                    break;

                case null:

                    throw new ArgumentNullException("Start node was null");

                default:

                    throw new ArgumentException($"Node has a unknown type ({startNode.GetType().Name})");
            }

            // add the connection
            Connection connection = new Connection()
            {
                start = new KnobConfiguration()
                {
                    label = connectionDto.startKnobLabel,
                    valueType = connectionDto.startValueType,
                    isInput = false,
                    knob = startKnob,
                    node = startNode
                },
                end = new KnobConfiguration()
                {
                    label = connectionDto.endKnobLabel,
                    valueType = connectionDto.endValueType,
                    isInput = true,
                    knob = endKnob,
                    node = endNode
                }
            };
            Connections.Add(connection);

        });

        // we recompute
        TargetToAddElementAndClickOn.Query<ComputationNode>().ForEach(node =>
        {
            RecomputeComputationNode(node);
        });
        TargetToAddElementAndClickOn.Query<ModuloNode>().ForEach(node =>
        {
            RecomputeModuloNode(node);
        });
    }

    #endregion

    #region "Handle Nodes value changes"

    private void HandleOutputChange(BaseNode sender, ValueChange change) {
        Debug.Log(nameof(HandleOutputChange));

        var connectedNodes = FindConnections(change.knob);
        if (connectedNodes.Count == 0)
        {
            Debug.Log("No connections found");
            return;
        }

        connectedNodes.ForEach(connection =>
        {

            if (connection.end.node is ComputationNode)
            {

                RecomputeComputationNode((ComputationNode)connection.end.node, connection.end.knob);
            }
            else if (connection.end.node is ValueNode)
            {
                connection.end.node.ConnectedInputChanged(change, connection.end);
            }
            else if (connection.end.node is ModuloNode)
            {
                RecomputeModuloNode((ModuloNode)connection.end.node);
            }
            else
            {
                throw new Exception($"Node type {connection.end.node.GetType().Name} not handled");
            }

        });

    }

    private void HandleComputationInputChange(object sender, ValueChange change)
    {
        Debug.Log(nameof(HandleComputationInputChange));

        if (change.label == "Operation")
        {
            var computationNode = change.node as ComputationNode;
            if (computationNode != null)
            {
                // Info : this is not an input with a knob so we let the function search it
                RecomputeComputationNode(computationNode);
            } else
            {
                // should not happen
            }
        } else
        {
            throw new Exception($"Change for {change.label} is not handled.");
        }
    }

    #endregion

    #region "Handle Node's Changes"

    private void HandleStopDraggingNode(BaseNode sender, PointerUpEvent evt)
    {
        // Debug.Log(nameof(HandleStopDraggingNode));

        // We recompute only nodes of which the output is connected
        var outputKnobs = sender.GetOutputKnobs();

        if (outputKnobs.Count == 0)
        {
            return;
        }

        foreach(var knob in outputKnobs)
        {
            var connections = FindConnections(knob);
            connections.ForEach(RefreshConnectionEndValues);
        }
    }

    #endregion

    #region "Handle Connection events"

    public override void AddConnection(KnobConfiguration start, KnobConfiguration end)
    {
        // FIXME : provide a settings to the base class for that ?
        // Info : we limit the number of connections only for the ModuloNode
        if (end.node is ModuloNode)
        {
            // Info : this will effectively replace the current connection
            Connections.RemoveAll(x => x.end.knob == end.knob);

            var connection = new Connection()
            {
                start = start,
                end = end
            };
            Connections.Add(connection);

            RaiseOnConnectionAdded(connection);
        } 
        else
        {
            base.AddConnection(start, end);
        }
        
    }

    public void HandleOnConnectionAdded(Connection connection)
    {
        Debug.Log(nameof(HandleOnConnectionAdded));

        RefreshConnectionEndValues(connection);
    }

    public void HandleOnConnectionRemoved(Connection connection)
    {

    }

    private void HandleAllConnectionsOfKnobRemoved(KnobConfiguration knobConfiguration, List<Connection> connectionsRemoved)
    {
        Debug.Log(nameof(HandleAllConnectionsOfKnobRemoved));

        // Info : if it is a input knob, we only want to refresh this element
        if (knobConfiguration.isInput)
        {
            // We only need to handle the ComputationNode as for now
            if (knobConfiguration.node is ComputationNode)
            {
                var computationNode = (ComputationNode)knobConfiguration.node;
                RecomputeComputationNode(computationNode);
            }
        } else
        {
            // Info : if it is a output knob, we want to refresh all nodes at the end of the connections
            connectionsRemoved.ForEach(RefreshConnectionEndValues);
        }
    }

    #endregion

    #region "(Re)compute"

    public void RecomputeComputationNode(ComputationNode computationNode, Knob computationNodeInputKnob = null)
    {
        // If not provided, we search the input knob
        if (computationNodeInputKnob == null)
        {
            computationNodeInputKnob = computationNode.GetInputKnob();
            if (computationNodeInputKnob == null)
            {
                throw new Exception("ComputationNode's input knob is null");
            }
        }

        // We search for other connections to the end
        // Info : we order them so that the operation can be controlled by the user
        var otherConnections = FindConnections(computationNodeInputKnob).OrderBy(x =>
        {
            return x.start.node.worldBound.position.y;
        }).ToList();

        // We search the computation type
        var computationType = computationNode.GetOperationType();

        float result = 1;

        switch (computationType)
        {
            case ComputationNode.OperationType.multiply:

                foreach (var conn in otherConnections)
                {
                    result *= GetStartNodeValue(conn);
                }

                break;

            case ComputationNode.OperationType.divide:

                result = GetStartNodeValue(otherConnections[0]);

                for (int i = 1; i < otherConnections.Count; i++)
                {
                    var conn = otherConnections[i];

                    if (conn.start.node is ValueNode)
                    {
                        result /= ((ValueNode)conn.start.node).Value;

                    }
                    else
                    {
                        throw new Exception($"Type of {conn.start.node.GetType()} is not handled.");
                    }
                }

                break;

            case ComputationNode.OperationType.add:

                result = 0;

                foreach (var conn in otherConnections)
                {
                    result += GetStartNodeValue(conn);
                }

                break;

            case ComputationNode.OperationType.substract:

                result = GetStartNodeValue(otherConnections[0]);

                for (int i = 1; i < otherConnections.Count; i++)
                {
                    var conn = otherConnections[i];

                    result -= GetStartNodeValue(conn);
                }

                break;
        }

        computationNode.Value = result;

        // We notify other nodes
        NotifyComputationResultChanged(computationNode);

    }

    public void RecomputeModuloNode(ModuloNode moduloNode)
    {
        Debug.Log(nameof(RecomputeModuloNode));

        var firstNumberKnob = moduloNode.GetDividendInputKnob();
        var secondNumberKnob = moduloNode.GetDivisorInputKnob();

        // We search for other connections to the end
        // Info : no need to order them
        var firstNumberConnections = FindConnections(firstNumberKnob);
        var secondNumberConnections = FindConnections(secondNumberKnob);

        if (firstNumberConnections.Count == 0 || secondNumberConnections.Count == 0)
        {
            Debug.LogWarning("No connections found for first number or second number");
            return;
        }

        // FIXME : maybe do not recompute if both outputs are empty ?
        // Info : if quotient or remainder connections are empty, we recompute anyway

        if (firstNumberConnections.Count > 1)
        {
            throw new Exception("First number connections count should be exactly one");
        }
        if (secondNumberConnections.Count > 1)
        {
            throw new Exception("Second number connections count should be exactly one");
        }

        var firstValue = (int)Math.Round(GetStartNodeValue(firstNumberConnections[0]));
        var secondValue = (int)Math.Round(GetStartNodeValue(secondNumberConnections[0]));

        var quotient = firstValue / secondValue;
        var remainder = firstValue % secondValue;// it is not the actual modulus operator, so I've heard

        moduloNode.Quotient = quotient;
        moduloNode.Remainder = remainder;

        Debug.Log("First value " + firstValue);
        Debug.Log("Second value " + secondValue);

        // We notify other nodes
        NotifyComputationResultChanged(moduloNode);

    }

    static float GetStartNodeValue(Connection connection)
    {
        BaseNode node = connection.start.node;

        float value = 0;

        if (node is ValueNode)
        {
            value = ((ValueNode)node).Value;
        }
        else if (node is ModuloNode)
        {
            var secondNodeAsModuloNode = (ModuloNode)node;
            if (connection.start.label == "Quotient")
            {
                value = secondNodeAsModuloNode.Quotient;
            }
            else if (connection.start.label == "Remainder")
            {
                value = secondNodeAsModuloNode.Remainder;
            }
            else
            {
                throw new Exception($"Unknown ModuloNode label {connection.start.label}");
            }
        } else
        {
            throw new Exception($"Unknown start node type {connection.start.node.GetType().Name}");
        }

        return value;
    }

    public void NotifyComputationResultChanged(ComputationNode computationNode)
    {
        var otherConnections = FindConnections(computationNode.GetOutputKnob()).ToList();

        otherConnections.ForEach(RefreshConnectionEndValues);
    }

    public void NotifyComputationResultChanged(ModuloNode moduloNode)
    {
        var quotientConnections = FindConnections(moduloNode.GetQuotientOutputKnob());
        var remainderConnections = FindConnections(moduloNode.GetRemainderOutputKnob());

        quotientConnections.ForEach(RefreshConnectionEndValues);
        remainderConnections.ForEach(RefreshConnectionEndValues);
    }

    /// <summary>
    /// Refresh end of the connection
    /// </summary>
    /// <param name="connection"></param>
    public void RefreshConnectionEndValues(Connection connection)
    {

        var startValue = GetStartNodeValue(connection);

        // Info : here we are displaying only the last value
        // TODO : maybe add limit on amount of values or replace the connections
        if (connection.end.node is DisplayNode)
        {

            var displayNode = (DisplayNode)connection.end.node;

            displayNode.ChangeValue(startValue);

        }
        else if (connection.end.node is ExportNode)
        {
            var exportNode = (ExportNode)connection.end.node;

            exportNode.ChangeValue(startValue);
        }
        else if (connection.end.node is ComputationNode)
        {
            var computationNode = (ComputationNode)connection.end.node;

            RecomputeComputationNode(computationNode, connection.end.knob);

        } 
        else if (connection.end.node is ModuloNode)
        {
            var moduloNode = (ModuloNode)connection.end.node;

            RecomputeModuloNode(moduloNode);
        }
        else
        {
            throw new Exception($"Node type {connection.end.node.GetType().Name} is not handled");
        }
    }

    #endregion

    #region "Node Menu"

    private void ShowNodeMenu(PointerUpEvent evt, BaseNode node)
    {
        // Debug.Log(nameof(ShowNodeMenu));

        if (evt.button == (int)MouseButton.RightMouse)
        {
            // not working
            //evt.PreventDefault();
            // not tested
            //evt.StopImmediatePropagation();
            evt.StopPropagation();

            selectedNode = node;

            nodeMenu.style.top = evt.position.y - nodeMenu.layout.height / 2;
            nodeMenu.style.left = evt.position.x - nodeMenu.layout.width / 2;
            nodeMenu.style.visibility = Visibility.Visible;
        }

    }

    private void HideNodeMenu(PointerLeaveEvent evt)
    {
        HideNodeMenu();
    }

    private void HideNodeMenu()
    {
        nodeMenu.style.visibility = Visibility.Hidden;
    }

    #endregion

    protected override Color GetConnectionColor(Connection connection)
    {
        // Debug.Log(nameof(GetConnectionColor));

        if (connection.start.node is FloatNode || connection.start.node is ComputationNode)
        {
            return Color.blue;
        }
        else if (connection.start.node is IntNode)
        {
            return Color.green;
        }

        return ConnectionsColor;
    }

    Color purple = new Color(128, 0, 128);

    protected override Color GetConnectionColor(KnobConfiguration knobConfiguration)
    {
        if (knobConfiguration.isInput && (knobConfiguration.node is DisplayNode || knobConfiguration.node is ExportNode))
        {
            return purple;
        } 
        else if (!knobConfiguration.isInput && (knobConfiguration.node is FloatNode || knobConfiguration.node is ComputationNode))
        {
            return Color.blue;
        }
        else if (!knobConfiguration.isInput && (knobConfiguration.node is IntNode))
        {
            return Color.green;
        }
        return ConnectionsColor;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Pause))
        {
            UnityEditor.EditorApplication.isPaused = true;
        }
    }
}
