using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MeshTools
{

	public class BaseLineStyle : ScriptableObject {

		public virtual void Setup()
		{
		}

		public virtual Vector2 Style(Vector2 point, Vector2 dir, float width, int i, int l, List<Vector2> points)
		{
			point = point + (dir * (width * 0.5f));

			return point;
		}
	}

}