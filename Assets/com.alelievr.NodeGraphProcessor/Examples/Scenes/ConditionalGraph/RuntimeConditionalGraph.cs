using System;
using UnityEngine;
using GraphProcessor;
using NodeGraphProcessor.Examples;

public class RuntimeConditionalGraph : MonoBehaviour
{
	[Header("Graph to Run on Start")]
	public BaseGraph graph;

	private ConditionalProcessor processor;
	
	[SerializeField] private bool _runOnUpdate = true;

	private void Start()
	{
		if(graph != null)
			processor = new ConditionalProcessor(graph);

		processor.Run();
	}

	private void Update()
	{
		if (_runOnUpdate)
			processor?.Run();
	}
}