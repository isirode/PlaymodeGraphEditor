using Assets.Isirode.UIToolkitExtension.UI.Elements;
using Isirode.PlaymodeGraphEditor.Playmode.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static Isirode.PlaymodeGraphEditor.Playmode.Nodes.BaseNode;
using static NodeEditorGUI;

namespace Isirode.PlaymodeGraphEditor.Playmode
{

    public class CanvasController
    {
        protected VisualElement root;
        protected VisualElement canvas;
        protected ScrollView scrollView;

        [NonSerialized]
        public VisualElement fitter;

        [NonSerialized]
        public VisualElement menu;

        [NonSerialized]
        public VisualElement absoluteParent;
        public Vector2 selectExtraRect = new Vector2(0, -3);

        // TODO ? : allow to know the target and change it here, maybe using a getter
        public VisualElement TargetToAddElementAndClickOn
        {
            get
            {
                return fitter;
            }
        }

        [NonSerialized]
        public List<BaseNode> baseNodes = new List<BaseNode>();

        [NonSerialized]
        public List<MenuNodeDefinition> menuDefinitionList;

        [NonSerialized]
        public List<MenuNode> menuNodes = new List<MenuNode>();

        // Input node being cloned
        // public VisualTreeAsset inputAsset;

        public class Connection
        {
            public Connection()
            {

            }

            public KnobConfiguration start;
            public KnobConfiguration end;
        }
        public List<Connection> Connections { get; set; } = new List<Connection>();

        public float scrollDirection = -1f;
        public Vector2 manualScroll = new Vector2(0, 0);
        public Vector2 scrollSpeed = new Vector2(5f, 5f);

        public float scaleDirection = -1f;
        private Vector3 scaleSpeed = new Vector3(0.1f, 0.1f, 0.1f);
        public Vector3 scale = new Vector3(1f, 1f, 1f);

        public bool useFixedColor = false;
        public Color ConnectionsColor = Color.blue;

        public CanvasController(VisualElement root)
        {
            this.root = root;
        }

        public virtual void Init()
        {
            canvas = root.Query<VisualElement>("Canvas");
            fitter = root.Query<VisualElement>("Fitter");
            scrollView = (ScrollView)root.Query<VisualElement>("ContainerScroll");

            // Do not fire
            scrollView.RegisterCallback<WheelEvent>(PreventDefaultWheelEvent);
            scrollView.RegisterCallback<WheelEvent>(Zoom);

            // Do not fire
            //fitter.RegisterCallback<WheelEvent>(OnWheelEvent);

            // Fire when you are over it, do not prevent scrolling through preventDefault
            // scrollView.verticalScroller.RegisterCallback<WheelEvent>(OnWheelEvent2);
            //scrollView.horizontalScroller.RegisterCallback<WheelEvent>(OnWheelEvent2);

            // Fire, do not prevent scrolling through preventDefault
            //scrollView.contentContainer.RegisterCallback<WheelEvent>(OnWheelEvent2);

            // Do not fire
            // scrollView.Children().First().RegisterCallback<WheelEvent>(OnWheelEvent2);

            // Do not fire in most regions, do not prevent scrolling through preventDefault
            // canvas.RegisterCallback<WheelEvent>(OnWheelEvent2);

            // Do not prevent scrolling
            scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;

            // Do not prevent either mode, do nothing
            // scrollView.mode = ScrollViewMode.Horizontal;

            // Fire event, do nothing
            // scrollView.Children().ToList().ForEach(x => x.RegisterCallback<WheelEvent>(OnWheelEvent2));

            // This is working, we are manually setting the offset
            manualScroll = scrollView.scrollOffset;

            // FIXME : probably want to instantiated it
            menu = root.Query<VisualElement>("Menu");
            menu.style.visibility = Visibility.Hidden;
            // menu.style.display = DisplayStyle.None;
            menu.RegisterCallback<PointerLeaveEvent>(HideMenu);

            absoluteParent = root.Query<VisualElement>("AbsoluteParent");
            if (absoluteParent == null)
            {
                throw new Exception("A visual element named 'AbsoluteParent' is required on top of the root's hierarchy for the Select's of the Nodes to work");
            }

            // containerScroll.contentContainer.RegisterCallback<PointerUpEvent>(ShowMenu);
            TargetToAddElementAndClickOn.RegisterCallback<PointerUpEvent>(ShowMenu);
            TargetToAddElementAndClickOn.RegisterCallback<PointerUpEvent>(StopDraggingLink);
            canvas.RegisterCallback<PointerDownEvent>(StartScrolling);
            canvas.RegisterCallback<PointerUpEvent>(EndScrolling);
            canvas.RegisterCallback<PointerMoveEvent>(Scroll);

            canvas.RegisterCallback<PointerLeaveEvent>(OnPointerLeaveCanvas);

            // Init GUIScaleUtility. This fetches reflected calls and might throw a message notifying about incompability.
            GUIScaleUtility.CheckInit();
        }

        #region "Menu"

        private void ShowMenu(PointerUpEvent evt)
        {
            // Debug.Log(nameof(ShowMenu));

            if (evt.button != ((int)MouseButton.RightMouse))
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
            // Debug.Log(nameof(HideMenu));

            if (this.menuNodes.Exists(x => x.SelfOrChildrenContains(evt.position)))
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

        #endregion

        #region "Zoom"

        // TODO : zooming in or out cause textfield to not display their values, maybe try to debug it or ask Unity
        // It actually do not display the text if the scale is not exactly 1

        private void PreventDefaultWheelEvent(WheelEvent evt)
        {
            // Debug.Log(nameof(PreventDefaultWheelEvent));
            // not working
            evt.PreventDefault();

            // Info (important) : work
            scrollView.scrollOffset = manualScroll;
        }

        private void Zoom(WheelEvent evt)
        {
            // Debug.Log(nameof(Zoom));

            var tempScale = scale += evt.delta.y * scaleSpeed * scaleDirection;
            scale = new Vector3(
                RoundTo1(Mathf.Clamp(tempScale.x, 0.2f, 2f)),
                RoundTo1(Mathf.Clamp(tempScale.y, 0.2f, 2f)),
                RoundTo1(Mathf.Clamp(tempScale.z, 0.2f, 2f))
            );
            // Note : we have to do this because Unity wont display the content of TextFields unless the scale is exactly 1
            // So if the scale speed does not return to 1, it wont never display the content ever again
            static float RoundTo1(float value)
            {
                if (value >= 0.7 & value <= 1.2) return 1;
                return value;
            }
            // Not working
            // containerScroll.style.scale = new StyleScale(new Scale(scale));

            // Not working correctly, reduce size, but cause to see part of the container on which we cannot add elements
            scrollView.contentContainer.style.scale = new StyleScale(new Scale(scale));

            // Not working very well
            // Info : we divide, it work this way
            /*
            var containerWidth = Mathf.Max(
                containerScroll.contentContainer.contentRect.width,
                Mathf.Min(
                    containerScroll.contentContainer.contentRect.width / scale.x,
                    2000
                )
            );
            containerScroll.contentContainer.style.width = new StyleLength(new Length(containerWidth, LengthUnit.Pixel));
            var containerHeigth = Mathf.Max(
                containerScroll.contentContainer.contentRect.height,
                Mathf.Min(
                    containerScroll.contentContainer.contentRect.height / scale.y,
                    2000
                )
            );
            containerScroll.contentContainer.style.height = new StyleLength(new Length(containerHeigth, LengthUnit.Pixel));
            Debug.Log("scale");
            Debug.Log(scale);
            Debug.Log("Width");
            Debug.Log(containerScroll.contentContainer.style.width);
            */

            // Do nothing
            /*
            containerScroll.contentContainer.style.position = Position.Absolute;
            containerScroll.contentContainer.style.top = -100;
            containerScroll.contentContainer.style.left = - 100;
            */

            // Not working correctly, cause the visual distance to change along the scale, they can be very far away and occupy full screen after zooming
            /*
            baseNodes.ForEach(x =>
            {
                x.style.scale = new StyleScale(new Scale(scale));
            });
            */

            // Not working
            //containerScroll.ScrollTo(fitter);
        }

        public void ResetScaleToOne()
        {
            scale = new Vector3(1f, 1f, 1f);
            scrollView.contentContainer.style.scale = new StyleScale(new Scale(scale));
        }

        #endregion

        #region "Scrolling"

        // FIXME : I probably can remove this now
        // Info : I used this because there were an offset between the output computed position and the his position when drawing the connections
        public Vector2 GetScaledMagicOffset()
        {
            return new Vector2(55, 12) * new Vector2(scale.x, scale.y);
        }

        public bool isScrolling = false;

        private void StartScrolling(PointerDownEvent evt)
        {
            // Debug.Log(nameof(StartScrolling));

            if (evt.button == ((int)MouseButton.RightMouse))
            {
                return;
            }
            // Warn : might even some race issue here
            if (isDraggingANode) return;

            isScrolling = true;
        }

        private void EndScrolling(PointerUpEvent evt)
        {
            // Debug.Log(nameof(EndScrolling));

            if (evt.button == ((int)MouseButton.RightMouse))
            {
                return;
            }
            // Info : we do not check isDraggingANode here

            isScrolling = false;
        }

        private void Scroll(PointerMoveEvent evt)
        {
            if (!isScrolling) return;

            // Warn : might even some race issue here
            if (isDraggingANode) return;

            var tempScroll = manualScroll + new Vector2(evt.deltaPosition.x, evt.deltaPosition.y) * scrollSpeed * scrollDirection;

            // Warn : we Clamp because it can be negative otherwise
            var scroll = new Vector2(
                Mathf.Clamp(tempScroll.x, scrollView.horizontalScroller.lowValue, scrollView.horizontalScroller.highValue),
                Mathf.Clamp(tempScroll.y, scrollView.verticalScroller.lowValue, scrollView.verticalScroller.highValue)
            );

            manualScroll = scroll;
            scrollView.scrollOffset = scroll;
        }

        #endregion

        public void SetupKnobCallbacks(BaseNode baseNode)
        {
            baseNode.OnPointerDownOnKnob += OnPointerDownOnKnob;
            baseNode.OnPointerUpOnKnob += OnPointerUpOnKnob;
        }

        public void SetupNodeDraggingCallbacks(BaseNode baseNode)
        {
            baseNode.OnStartDraggingNode += OnStartDraggingNode;
            baseNode.OnStopDraggingNode += OnStopDraggingNode;
        }

        #region "Dragging node"

        private bool isDraggingANode = false;

        private void OnStartDraggingNode(object sender, PointerDownEvent evt)
        {
            // Debug.Log(nameof(OnStartDraggingNode));

            isDraggingANode = true;
        }

        private void OnStopDraggingNode(object sender, PointerUpEvent evt)
        {
            // Debug.Log(nameof(OnStopDraggingNode));

            isDraggingANode = false;
        }

        #endregion

        #region "Dragging link"

        private void StopDraggingLink(PointerUpEvent evt)
        {
            if (isDraggingLink)
            {
                isDraggingLink = false;
            }
        }

        private void OnPointerLeaveCanvas(PointerLeaveEvent evt)
        {
            // Debug.Log(nameof(OnPointerLeaveCanvas));

            if (isDraggingLink)
            {
                isDraggingLink = false;
            }
        }

        private void OnPointerDownOnKnob(object sender, PointerDownEvent evt, KnobConfiguration knobConfiguration)
        {
            // Debug.Log(nameof(OnPointerDownOnKnob));

            if (evt.button == ((int)MouseButton.LeftMouse))
            {
                isDraggingLink = true;
                currentKnobConfiguration = knobConfiguration;
            }
        }

        private void OnPointerUpOnKnob(object sender, PointerUpEvent evt, KnobConfiguration knobConfiguration)
        {
            // Debug.Log(nameof(OnPointerUpOnKnob));

            if (evt.button == ((int)MouseButton.LeftMouse))
            {
                isDraggingLink = false;

                // TODO : maybe allow recursive configuration (output to input) somehow
                if (currentKnobConfiguration.node == knobConfiguration.node)
                {
                    return;
                }

                // Info : input is the end
                if (currentKnobConfiguration.isInput && !knobConfiguration.isInput)
                {
                    AddConnection(knobConfiguration, currentKnobConfiguration);
                }
                if (knobConfiguration.isInput && !currentKnobConfiguration.isInput)
                {
                    AddConnection(currentKnobConfiguration, knobConfiguration);
                }
            }
            else if (evt.button == ((int)MouseButton.RightMouse))
            {
                List<Connection> connectionsToBeRemoved = Connections.FindAll(RemoveAllPredicate(knobConfiguration)).ToList();

                Connections.RemoveAll(RemoveAllPredicate(knobConfiguration));

                OnAllConnectionsOfKnobRemoved?.Invoke(knobConfiguration, connectionsToBeRemoved);
            }

            static Predicate<Connection> RemoveAllPredicate(KnobConfiguration knobConfiguration)
            {
                return x => x.start.knob == knobConfiguration.knob || x.end.knob == knobConfiguration.knob;
            }
        }

        public delegate void OnAllConnectionsOfKnobRemovedEvent(KnobConfiguration knobConfiguration, List<Connection> connectionsRemoved = null);
        public event OnAllConnectionsOfKnobRemovedEvent OnAllConnectionsOfKnobRemoved;

        public delegate void OnConnectionAddedEvent(Connection connection);
        public event OnConnectionAddedEvent OnConnectionAdded;

        public virtual void AddConnection(KnobConfiguration start, KnobConfiguration end)
        {
            var connection = new Connection()
            {
                start = start,
                end = end
            };
            Connections.Add(connection);

            OnConnectionAdded?.Invoke(connection);
        }

        protected void RaiseOnConnectionAdded(Connection connection)
        {
            OnConnectionAdded?.Invoke(connection);
        }

        KnobConfiguration currentKnobConfiguration;
        private bool isDraggingLink = false;

        #endregion

        // FIXME : put in OnPostRender (not working but recommanded by Unity) ?
        public void OnGUI()
        {
            Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
            Rect rect = canvas.layout;

            // This is restricting the GL drawing, but modify the positions
            GUI.BeginGroup(screenRect);
            GUILayout.BeginArea(rect);

            if (isDraggingLink)
            {
                DrawConnection(Event.current.mousePosition);
            }

            foreach (var connection in Connections)
            {
                // Info : this is not correctly displayed
                // var start = connection.start.knob.LocalToWorld(connection.start.knob.layout.center) - canvas.layout.position;
                var start = connection.start.knob.OuterElement.worldBound.center - canvas.layout.position;
                var startDirection = Vector2.right;

                // Info : this is not correctly displayed
                // var end = connection.end.knob.LocalToWorld(connection.end.knob.layout.center) - canvas.layout.position;
                var end = connection.end.knob.OuterElement.worldBound.center - canvas.layout.position;

                // end -= GetScaledMagicOffset();
                var endDirection = Vector2.left;

                if (!useFixedColor)
                {
                    NodeEditorGUI.DrawConnection(start, startDirection, end, endDirection, GetConnectionColor(connection));
                }
                else
                {
                    NodeEditorGUI.DrawConnection(start, startDirection, end, endDirection, ConnectionsColor);
                }
            }

            GUILayout.EndArea();
            GUI.EndGroup();
        }

        /// <summary>
        /// Draws a connection line from the current knob to the specified position
        /// </summary>
        public virtual void DrawConnection(Vector2 endPos)
        {

            // currentKnob.knob.LocalToWorld works if we are not using GUI.BeginGroup
            // Info : this is not working for the end knob
            // var position = knobConfiguration1.knob.LocalToWorld(knobConfiguration1.knob.layout.center) - canvas.layout.position;
            /*if (!knobConfiguration1.isInput)
            {
                position -= GetScaledMagicOffset();// new Vector2(55, 12);
            }*/
            var position = currentKnobConfiguration.knob.OuterElement.worldBound.center - canvas.layout.position;

            if (!useFixedColor)
            {
                // Warn : I wanted to use the OuterElement's border color but Unity return 0, 0, 0 every time & keyword is null (correctly indicated in the Debugger)
                /*
                Debug.Log("Colors");
                Debug.Log(currentKnobConfiguration.knob.OuterElement.style.borderTopColor.keyword);
                Debug.Log(currentKnobConfiguration.knob.OuterElement.style.borderTopColor.value);
                Debug.Log(currentKnobConfiguration.knob.OuterElement.style.borderBottomColor.value);
                Debug.Log(currentKnobConfiguration.knob.OuterElement.style.borderLeftColor.value);
                Debug.Log(currentKnobConfiguration.knob.OuterElement.style.borderRightColor.value);
                Debug.Log(currentKnobConfiguration.knob.style.borderTopColor.value);
                Debug.Log(currentKnobConfiguration.knob.InnnerElement.style.borderTopColor.value);
                */
                NodeEditorGUI.DrawConnection(position, endPos, ConnectionDrawMethod.StraightLine, GetConnectionColor(currentKnobConfiguration));
            }
            else
            {
                NodeEditorGUI.DrawConnection(position, endPos, ConnectionDrawMethod.StraightLine, ConnectionsColor);
            }
        }

        protected virtual Color GetConnectionColor(Connection connection)
        {
            return ConnectionsColor;
        }

        protected virtual Color GetConnectionColor(KnobConfiguration knobConfiguration)
        {
            return ConnectionsColor;
        }

        public List<Connection> FindConnections(Knob knob)
        {
            return Connections.Where(x =>
            {
                return (x.start.knob == knob || x.end.knob == knob);
            }).ToList();
        }
    }
}