using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine.Pool;
using Debug = UnityEngine.Debug;

namespace NodeGraphProcessor.Examples
{
    public class ConditionalProcessor : BaseGraphProcessor
    {
        List< BaseNode >		processList;
        List< StartNode >		startNodeList;

        Dictionary<BaseNode, List<BaseNode>>    nonConditionalDependenciesCache = new Dictionary<BaseNode, List<BaseNode>>();

        public bool             pause;

        public IEnumerator<BaseNode> currentGraphExecution { get; private set; } = null;

        // static readonly float   maxExecutionTimeMS = 100; // 100 ms max execution time to avoid infinite loops

        /// <summary>
        /// Manage graph scheduling and processing
        /// </summary>
        /// <param name="graph">Graph to be processed</param>
        public ConditionalProcessor(BaseGraph graph) : base(graph) {}

        public override void UpdateComputeOrder()
        {
            // Gather start nodes:
            startNodeList = graph.nodes.Where(n => n is StartNode).Select(n => n as StartNode).ToList();

            // In case there is no start node, we process the graph like usual
            if (startNodeList.Count == 0)
            {
                processList = graph.nodes.OrderBy(n => n.computeOrder).ToList();
            }
            else
            {
                nonConditionalDependenciesCache.Clear();
                // Prepare the cache of non-conditional node execution
            }
        }

        public override void Run()
        {
            IEnumerator<BaseNode> enumerator;

            if(startNodeList.Count == 0)
            {
                enumerator = RunTheGraph();
            }
            else
            {
                Stack<BaseNode> nodeToExecute = new Stack<BaseNode>();
                // Add all the start nodes to the execution stack
                startNodeList.ForEach(s => nodeToExecute.Push(s));
                // Execute the whole graph:
                enumerator = RunTheGraph(nodeToExecute);
            }

            while(enumerator.MoveNext())
                ;
        }
        
        private void WaitedRun(Stack<BaseNode> nodesToRun)
        {
            // Execute the waitable node:
            var enumerator = RunTheGraph(nodesToRun);

            while(enumerator.MoveNext())
                ;
        }
        
        private Stack<BaseNode> _stackTemp = new Stack<BaseNode>();
        void GatherNonConditionalDependencies(BaseNode node,List<BaseNode> toAddCache)
        {
	        _stackTemp.Clear();

	        _stackTemp.Push(node);
        
            while (_stackTemp.Count > 0)
            {
                var dependency = _stackTemp.Pop();
                using (ListPool<BaseNode>.Get(out var list))
                {
	                list.Clear();
	                dependency.GetInputNodes(list);
	                foreach (var de in list)
	                {
		                if (de is not IConditionalNode)
			                _stackTemp.Push(de);
	                }
                }

                if (dependency != node)
                    toAddCache.Add(dependency); 
            }
        }
        
        private IEnumerator<BaseNode> RunTheGraph()
        {
            int count = processList.Count;

            for(int i = 0; i < count; i++)
            {
                processList[i].OnProcess();
                yield return processList[i];
            }
        }
        
	    private IEnumerator<BaseNode> RunTheGraph(Stack<BaseNode> nodeToExecute)
		{
			HashSet<BaseNode> nodeDependenciesGathered = new HashSet<BaseNode>();
			HashSet<BaseNode> skipConditionalHandling  = new HashSet<BaseNode>();
	
			while(nodeToExecute.Count > 0)
			{
				var node = nodeToExecute.Pop();
				// TODO: maxExecutionTimeMS
	
				// In case the node is conditional, then we need to execute it's non-conditional dependencies first
				if(node is IConditionalNode && !skipConditionalHandling.Contains(node))
				{
					// Gather non-conditional deps: TODO, move to the cache:
					if(nodeDependenciesGathered.Contains(node))
					{
						// Execute the conditional node:
						node.OnProcess();
						yield return node;
	
						// And select the next nodes to execute:
						switch(node)
						{
							// special code path for the loop node as it will execute multiple times the same nodes
							case ForLoopNode forLoopNode:
								forLoopNode.index = forLoopNode.start - 1; // Initialize the start index
								foreach(var n in forLoopNode.GetExecutedNodesLoopCompleted())
									nodeToExecute.Push(n);
								for(int i = forLoopNode.start; i < forLoopNode.end; i++)
								{
									foreach(var n in forLoopNode.GetExecutedNodesLoopBody())
										nodeToExecute.Push(n);
	
									nodeToExecute.Push(node); // Increment the counter
								}
	
								skipConditionalHandling.Add(node);
								break;
							// another special case for waitable nodes, like "wait for a coroutine", wait x seconds", etc.
							case WaitableNode waitableNode:
								foreach(var n in waitableNode.GetExecutedNodes())
									nodeToExecute.Push(n);
	
								waitableNode.onProcessFinished += (waitedNode) =>
								{
									Stack<BaseNode> waitedNodes = new Stack<BaseNode>();
									foreach(var n in waitedNode.GetExecuteAfterNodes())
										waitedNodes.Push(n);
									WaitedRun(waitedNodes);
									waitableNode.onProcessFinished = null;
								};
								break;
							case IConditionalNode cNode:
								foreach(var n in cNode.GetExecutedNodes())
									nodeToExecute.Push(n);
								break;
							default:
								Debug.LogError($"Conditional node {node} not handled");
								break;
						}
	
						nodeDependenciesGathered.Remove(node);
					}
					else
					{
						nodeToExecute.Push(node);
						nodeDependenciesGathered.Add(node);
						using (ListPool<BaseNode>.Get(out var l))
						{
							l.Clear();
							GatherNonConditionalDependencies(node,l);
							foreach(var nonConditionalNode in l)
							{
								nodeToExecute.Push(nonConditionalNode);
							}
						}
					}
				}
				else
				{
					node.OnProcess();
					yield return node;
				}
			}
		}

        // Advance the execution of the graph of one node, mostly for debug. Doesn't work for WaitableNode's executeAfter port.
        public void Step()
        {
            if (currentGraphExecution == null)
            {
	            Stack<BaseNode> nodeToExecute = new Stack<BaseNode>();
	            if(startNodeList.Count > 0)
		            startNodeList.ForEach(s => nodeToExecute.Push(s));

	            currentGraphExecution = startNodeList.Count == 0 ? RunTheGraph() : RunTheGraph(nodeToExecute);
	            currentGraphExecution.MoveNext(); // Advance to the first node
            }
            else
            if (!currentGraphExecution.MoveNext())
                currentGraphExecution = null;
        }
    }
}
