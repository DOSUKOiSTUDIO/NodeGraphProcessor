using System;
using System.IO;
using GraphProcessor;
using UnityEditor;
using UnityEngine;

public static class GraphCreateAndSaveHelper
{
    /// <summary>
    /// NodeGraphProcessor路径前缀
    /// </summary>
    public const string NodeGraphProcessorPathPrefix = "Assets/Plugins/NodeGraphProcessor";

    public static BaseGraph CreateGraph(Type graphType)
    {
        BaseGraph baseGraph = ScriptableObject.CreateInstance(graphType) as BaseGraph;
        string panelPath = $"{NodeGraphProcessorPathPrefix}/Examples/Saves/";
        Directory.CreateDirectory(panelPath);
        string panelFileName = "Graph";
        string path = EditorUtility.SaveFilePanelInProject("Save Graph Asset", panelFileName, "asset", "", panelPath);
        if (string.IsNullOrEmpty(path))
        {
            Debug.Log("创建graph已取消");
            return null;
        }
        AssetDatabase.CreateAsset(baseGraph, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return baseGraph;
    }
        
    public static void SaveGraphToDisk(BaseGraph baseGraphToSave)
    {
        EditorUtility.SetDirty(baseGraphToSave);
        AssetDatabase.SaveAssets();
    }
}