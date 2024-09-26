using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Object = UnityEngine.Object;
using TNRD;

namespace GraphProcessor
{
    /*
    /// <summary>
    /// Custom editor of the node inspector, you can inherit from this class to customize your node inspector.
    /// </summary>
    [CustomEditor(typeof(NodeInspectorObject))]
    public class NodeInspectorObjectEditor : Editor
    {
        NodeInspectorObject inspector;
        protected VisualElement root;
        protected VisualElement selectedNodeList;
        protected VisualElement placeholder;

        protected virtual void OnEnable()
        {
            inspector = target as NodeInspectorObject;
            inspector.nodeSelectionUpdated += UpdateNodeInspectorList;
            root = new VisualElement();
            selectedNodeList = new VisualElement();
            selectedNodeList.styleSheets.Add(Resources.Load<StyleSheet>("GraphProcessorStyles/InspectorView"));
            root.Add(selectedNodeList);
            placeholder = new Label("Select a node to show it's settings in the inspector");
            placeholder.AddToClassList("PlaceHolder");
            UpdateNodeInspectorList();
        }

        protected virtual void OnDisable()
        {
            inspector.nodeSelectionUpdated -= UpdateNodeInspectorList;
        }

        public override VisualElement CreateInspectorGUI() => root;

        protected virtual void UpdateNodeInspectorList()
        {
            selectedNodeList.Clear();

            if (inspector.selectedNodes.Count == 0)
                selectedNodeList.Add(placeholder);

            foreach (var nodeView in inspector.selectedNodes)
                selectedNodeList.Add(CreateNodeBlock(nodeView));
        }

        protected VisualElement CreateNodeBlock(BaseNodeView nodeView)
        {
            var view = new VisualElement();

            view.Add(new Label(nodeView.nodeTarget.name));

            var tmp = nodeView.controlsContainer;
            nodeView.controlsContainer = view;
            nodeView.Enable(true);
            nodeView.controlsContainer.AddToClassList("NodeControls");
            var block = nodeView.controlsContainer;
            nodeView.controlsContainer = tmp;
            
            return block;
        }
    }

    /// <summary>
    /// Node inspector object, you can inherit from this class to customize your node inspector.
    /// </summary>
    public class NodeInspectorObject : SerializedScriptableObject
    {
        /// <summary>Previously selected object by the inspector</summary>
        public Object previouslySelectedObject;
        /// <summary>List of currently selected nodes</summary>
        public HashSet<BaseNodeView> selectedNodes { get; private set; } = new HashSet<BaseNodeView>();

        /// <summary>Triggered when the selection is updated</summary>
        public event Action nodeSelectionUpdated;

        /// <summary>Updates the selection from the graph</summary>
        public virtual void UpdateSelectedNodes(HashSet<BaseNodeView> views)
        {
            selectedNodes = views;
            nodeSelectionUpdated?.Invoke();
        }

        public virtual void RefreshNodes() => nodeSelectionUpdated?.Invoke();

        public virtual void NodeViewRemoved(BaseNodeView view)
        {
            selectedNodes.Remove(view);
            nodeSelectionUpdated?.Invoke();
        }
    }
    */


using System.Collections.Generic;
using GraphProcessor;
using Sirenix.OdinInspector;
using UnityEditor;

[Serializable]
public class HogeNode : BaseNode
{
    [SerializeField] public SerializableInterface<IComparable> sadsa;
}

/// <summary>
    /// Node inspector object, you can inherit from this class to customize your node inspector.
    /// </summary>
    public class NodeInspectorObject : SerializedScriptableObject
    {
        //public HashSet<BaseNodeView> selectedNodeViews = new HashSet<BaseNodeView>();
        //public HashSet<BaseNode> selectedNodes = new HashSet<BaseNode>();
#if original
        public HashSet<BaseNodeView> selectedNodeViews = new HashSet<BaseNodeView>();
#endif
        //view でもnodeでもどっちでもあかんかった。
        public BaseNode selectedNode;
        
        public List<BaseNode> asdsad = new List<BaseNode>()
        {
            new HogeNode()
        };
        
        //なんかわからんが、Listの中に入ってるとSerializableInterfaceのCustomPropertyDrawerがうまくできねえらしい
        
        [SerializeField]
        private HogeNode sada;
        
        [SerializeField]
        public SerializableInterface<IComparable> selectedInterface = new();


        public virtual void NodeViewRemoved(BaseNode view)
        {
#if original
            selectedNodeViews.Remove(view);
#endif
            selectedNode = null;
        }
        
        //public virtual void RefreshNodes() => nodeSelectionUpdated?.Invoke();
    }
    
    /// <summary>
    /// 手动清理NodeInspectorObject，防止出现编辑已经非法的数据，因为NodeView是每次重新编译/PlayMode后重新构建的
    /// 所以如果这里不做清理在重新编译/PlayMode后编辑的就是已经失效的数据
    /// /// Manually clean up NodeInspectorObject to prevent editing invalid data, 
    /// because NodeView is rebuilt every time it is recompiled or enters PlayMode.
    /// If this cleaning is not done, you will be editing invalid data after recompilation or entering PlayMode.
    /// </summary>
    public class ResetSelectNodeInfo : Editor
    {
        [InitializeOnLoadMethod]
        public static void _ResetSelectNodeInfo()
        {
            if (Selection.activeObject is NodeInspectorObject nodeInspectorObject)
            {
#if original
                nodeInspectorObject.selectedNodes.Clear();
#endif
                nodeInspectorObject.selectedNode = null;
            }
        }
    }
}