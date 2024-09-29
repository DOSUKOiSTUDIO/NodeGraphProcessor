using System.Linq;
using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using Unity.EditorCoroutines.Editor;

namespace GraphProcessor
{
	[System.Serializable]
	public abstract class BaseGraphWindow : EditorWindow
	{
		protected VisualElement		rootView;
		//DOSUKOI
		public BaseGraphView		graphView;

		[SerializeField]
		protected BaseGraph			graph;

		readonly string				graphWindowStyle = "GraphProcessorStyles/BaseGraphView";

		public bool					isGraphLoaded
		{
			get { return graphView != null && graphView.graph != null; }
		}

		bool						reloadWorkaround = false;

		public event Action< BaseGraph >	graphLoaded;
		public event Action< BaseGraph >	graphUnloaded;

		/// <summary>
		/// 上一帧记录时间点
		/// </summary>
		public static double LastTimePoint = 0;
		
		/// <summary>
		/// 加载Views单次Tick最长耗时，单位为ms，用于分帧加载
		/// </summary>
		public static double LoadViewsMaxLimitTime = 33;
		
		/// <summary>
		/// Called by Unity when the window is enabled / opened
		/// 只会在EditorWindow初次打开/重新编译/进入PlayMode的时候才会执行一次
		/// </summary>
		protected virtual void OnEnable()
		{
			InitializeRootView();
			
			graphLoaded = baseGraph => { baseGraph?.OnGraphEnable(); }; 
			graphUnloaded = baseGraph => { baseGraph?.OnGraphDisable(); };
			//注意，一定不能在EditorWindow的OnEnable中进行一些序列化相关的操作，因为执行完OnEnable后Unity内部就会进行GC（依照Unity的规则）
			//所以这里GraphView相关数据的操作就统一放到OnEnable之后去执行，防止一些数据刚组装好，就直接GC了
			// 注意：絶対にEditorWindowのOnEnableの中でシリアル化に関連する操作を行わないでください
			// // なぜなら、OnEnableの実行後、Unityは内部でGCを行うためです（Unityの規則に従っています）。
// そのため、ここでGraphViewに関連するデータの操作はOnEnableの後に実行するように統一しています。
// こうすることで、一部のデータが剛組み立てられたばかりでGCされてしまうのを防ぎます。
			reloadWorkaround = true;
		}

		protected virtual void Update()
		{
			// Workaround for the Refresh option of the editor window:
			// When Refresh is clicked, OnEnable is called before the serialized data in the
			// editor window is deserialized, causing the graph view to not be loaded
			if (reloadWorkaround && graph != null)
			{
				InitializeGraph(graph);
				reloadWorkaround = false;
			}

			LastTimePoint = EditorApplication.timeSinceStartup;
		}
		
		/// <summary>
		/// Called by Unity when the window is disabled (happens on domain reload)
		/// </summary>
		protected virtual void OnDisable()
		{
			if (graph != null && graphView != null)
			{
				graphView.SaveGraphToDisk();
				// Unload the graph
				graphUnloaded?.Invoke(this.graph);
			}
		}

		/// <summary>
		/// Called by Unity when the window is closed
		/// </summary>
		protected virtual void OnDestroy()
		{
			graphView?.Dispose();
		}

		void InitializeRootView()
		{
			rootView = base.rootVisualElement;

			rootView.name = "graphRootView";

			rootView.styleSheets.Add(Resources.Load<StyleSheet>(graphWindowStyle));
		}

		/// この関数が呼び出されるのは2つのケースのみです
		/// 1. GraphEditorWindowを開く時（初めて開く場合または既に開いている状態でGraphAssetを変更する場合）
		/// 2. コンパイル/PlayModeに入ることでGraphEditorWindowが再読み込みされた時
		
		/// <summary>
		/// この関数が呼び出されるのは以下の2つの場合のみです
		/// 1.GraphEditorWindowを開いたとき（初めて開く場合または既に開いている状態でGraphAssetを変更した場合）
		/// 2.コンパイル/PlayModeに入ることによってGraphEditorWindowが再読み込みされる場合
		/// </summary>
		/// <param name="graph"></param>
		public void InitializeGraph(BaseGraph graph)
		{
			if (this.graph != null && graph != this.graph)
			{
				// Save the graph to the disk
				GraphCreateAndSaveHelper.SaveGraphToDisk(this.graph);
				// Unload the graph
				graphUnloaded?.Invoke(this.graph);
			}

			graphLoaded?.Invoke(graph);
			this.graph = graph;

			if (graphView != null)
			{
				rootView.Remove(graphView);
			}

			//Initialize will provide the BaseGraphView
			InitializeWindow(graph);
			rootView.Add(graphView);
			
			graphView = rootView.Children().FirstOrDefault(e => e is BaseGraphView) as BaseGraphView;

			if (graphView == null)
			{
				Debug.LogError("GraphView has not been added to the BaseGraph root view !");
				return ;
			}

			EditorCoroutineUtility.StartCoroutine(graphView.Initialize(graph), this);

			InitializeGraphView(graphView);

			// TOOD: onSceneLinked...

			if (graph.IsLinkedToScene())
				LinkGraphWindowToScene(graph.GetLinkedScene());
			else
				graph.onSceneLinked += LinkGraphWindowToScene;
			//防止在外部调用InitializeGraph时重复执行InitializeGraph
			// InitializeGraphを外部から呼び出す際に、InitializeGraphが繰り返し実行されるのを防ぐ
			reloadWorkaround = false;
		}

		/// <summary>
		/// 主动刷新EditorWindow
		/// </summary>
		public void RefreshWindow()
		{
			reloadWorkaround = true;
		}
		
		void LinkGraphWindowToScene(Scene scene)
		{
			EditorSceneManager.sceneClosed += CloseWindowWhenSceneIsClosed;

			void CloseWindowWhenSceneIsClosed(Scene closedScene)
			{
				if (scene == closedScene)
				{
					Close();
					EditorSceneManager.sceneClosed -= CloseWindowWhenSceneIsClosed;
				}
			}
		}

		public virtual void OnGraphDeleted()
		{
			if (graph != null && graphView != null)
				rootView.Remove(graphView);

			graphView = null;
		}

		protected abstract void	InitializeWindow(BaseGraph graph);
		protected virtual void InitializeGraphView(BaseGraphView view) {}
	}
}