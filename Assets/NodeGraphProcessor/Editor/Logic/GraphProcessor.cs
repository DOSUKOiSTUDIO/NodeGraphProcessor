﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Jobs;
using Unity.Collections;

namespace GraphProcessor
{
	class GraphScheduleList
	{
		public BaseNode			node;
		public BaseNode[]		dependencies;

		public GraphScheduleList(BaseNode node)
		{
			this.node = node;
		}
	}

	public struct Test : IJobParallelFor
	{
		public void Execute(int index)
		{
			throw new System.NotImplementedException();
		}
	}

	public class BaseGraphProcessor
	{
		BaseGraph					graph;
		GraphScheduleList[]			scheduleList;

		public BaseGraphProcessor(BaseGraph graph)
		{
			this.graph = graph;

			UpdateComputeOrder();
		}

		void UpdateComputeOrder()
		{
			scheduleList = graph.nodes.OrderBy(n => n.computeOrder).Select(n => {
				GraphScheduleList gsl = new GraphScheduleList(n);
				gsl.dependencies = n.GetInputNodes().ToArray();
				return gsl;
			}).ToArray();
		}

		public void ScheduleJobs()
		{
			int count = scheduleList.Length;
			var scheduledHandles = new Dictionary< BaseNode, JobHandle >();

			for (int i = 0; i < count; i++)
			{
				var schedule = scheduleList[i];
				JobHandle currentJob = schedule.node.Schedule();
				int dependenciesCount = schedule.dependencies.Length;

				scheduledHandles[schedule.node] = currentJob;

				for (int j = 0; j < dependenciesCount; j++)
					JobHandle.CombineDependencies(currentJob, scheduledHandles[schedule.dependencies[j]]);
			}

			JobHandle.ScheduleBatchedJobs();
		}
	}
}
