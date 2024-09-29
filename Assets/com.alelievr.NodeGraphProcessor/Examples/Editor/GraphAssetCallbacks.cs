using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GraphProcessor;
using UnityEditor.Callbacks;
using System.IO;
using Examples.Editor;

public class GraphAssetCallbacks
{
	[MenuItem("Assets/Create/GraphProcessor", false, 10)]
	public static void CreateGraphPorcessor()
	{
		var graph = ScriptableObject.CreateInstance< BaseGraph >();
		ProjectWindowUtil.CreateAsset(graph, "GraphProcessor.asset");
	}

	[OnOpenAsset(0)]
	public static bool OnBaseGraphOpened(int instanceID, int line)
	{
		var asset = EditorUtility.InstanceIDToObject(instanceID) as BaseGraph;
		var baseGraph = EditorUtility.InstanceIDToObject(instanceID) as BaseGraph;
		return InitializeGraph(baseGraph);
		
		if (asset != null && AssetDatabase.GetAssetPath(asset).Contains("Examples"))
		{
			EditorWindow.GetWindow<AllGraphWindow>().InitializeGraph(asset as BaseGraph);
			return true;
		}
		return false;
	}
	
	public static bool InitializeGraph(BaseGraph baseGraph)
	{
		if (baseGraph == null) return false;
		
		switch (baseGraph)
		{
			//case SkillGraph skillGraph:
			//	EditorWindow.GetWindow<SkillGraphWindow>().InitializeGraph(skillGraph);
			//	break;
			//case NPBehaveGraph npBehaveGraph:
			//	EditorWindow.GetWindow<NPBehaveGraphWindow>().InitializeGraph(npBehaveGraph);
			//	break;
			default:
				EditorWindow.GetWindow<FallbackGraphWindow>().InitializeGraph(baseGraph);
				break;
		}

		return true;
	}
}
