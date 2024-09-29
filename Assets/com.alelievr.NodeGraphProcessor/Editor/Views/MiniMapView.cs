using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace GraphProcessor
{
	public class MiniMapView : MiniMap
	{
		new BaseGraphView	graphView;
		Vector2				size;

		public MiniMapView(BaseGraphView baseGraphView)
		{
			this.graphView = baseGraphView;
			SetPosition(new Rect(0, 0, 100, 100));
			size = new Vector2(100, 100);
		}
	}
}