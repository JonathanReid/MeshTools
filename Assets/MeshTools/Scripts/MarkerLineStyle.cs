using UnityEngine;
using System.Collections;
using MeshTools;
using System.Collections.Generic;

namespace hs.games.inventor
{
	public class MarkerLineStyle : BaseLineStyle {

		private System.Random _rand;

		public override void Setup()
		{
			_rand = new System.Random(1);
		}

		public override Vector2 Style (Vector2 point, Vector2 dir, float width, int i, int l, List<Vector2> points)
		{
			float w = 0;
			w = width + ((Mathf.Sin (((i + (_rand.Next (-20, 20)) / 5f))) / 50f) - width / 4);

			if (i > l - 10) {
				w = width - ((i - (l - 10)) / 80f) - Vector3.Distance (point, points[l - 10]) / 10f;
			}
			if (i < 10) {
				w = width - (width * Mathf.Abs (i - 11) / 10f);
			}
			if (i == 0) {
				w = 0;
			}

			point = point + (dir * (w * 0.5f));

			return point;
		}
	}
}