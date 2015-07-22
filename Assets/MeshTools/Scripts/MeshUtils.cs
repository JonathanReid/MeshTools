using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MeshUtils  {

	public static bool LineIntersection(Vector2 linePoint1, Vector2 linePoint2, Vector2 linePoint3, Vector2 linePoint4, ref Vector2 intersection)
	{
		Vector2 a = linePoint2 - linePoint1;
		Vector2 b = linePoint3 - linePoint4;
		Vector2 c = linePoint1 - linePoint3;
		
		float alphaNumerator = b.y * c.x - b.x * c.y;
		float alphaDenominator = a.y * b.x - a.x * b.y;
		float betaNumerator = a.x * c.y - a.y * c.x;
		float betaDenominator = alphaDenominator;
		
		bool doIntersect = true;
		
		if (alphaDenominator == 0 || betaDenominator == 0)
		{
			doIntersect = false;
		}
		else
		{
			
			if (alphaDenominator > 0)
			{
				if (alphaNumerator < 0 || alphaNumerator > alphaDenominator)
				{
					doIntersect = false;
				}
			}
			else if (alphaNumerator > 0 || alphaNumerator < alphaDenominator)
			{
				doIntersect = false;
			}
			
			if (doIntersect && betaDenominator > 0)
			{
				if (betaNumerator < 0 || betaNumerator > betaDenominator)
				{
					doIntersect = false;
				}
			}
			else if (betaNumerator > 0 || betaNumerator < betaDenominator)
			{
				doIntersect = false;
			}
		}
		
		float Ax, Ay, f, num;
		
		Ax = linePoint2.x - linePoint1.x;
		Ay = linePoint2.y - linePoint1.y;
		
		num = alphaNumerator * Ax; // numerator //
		f = alphaDenominator;
		intersection.x = linePoint1.x + num / f;
		
		num = alphaNumerator * Ay;
		intersection.y = linePoint1.y + num / f;
		return doIntersect;
	}

	public static bool IsPointInPolygon(Vector2[] polygon, Vector2 point)
	{
		bool isInside = false;
		for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
		{
			if (((polygon[i].y > point.y) != (polygon[j].y > point.y)) &&
			    (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
			{
				isInside = !isInside;
			}
		}
		return isInside;
	}

	public static Vector3 CatmullRomSpline(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
	{
		Vector3 result = new Vector3();
		
		float x = 0.5f * ((          2*p1.x) +
		                  t * (( -p0.x           +p2.x) +
		     t * ((2*p0.x -5*p1.x +4*p2.x -p3.x) +
		     t * (  -p0.x +3*p1.x -3*p2.x +p3.x))));
		
		float y = 0.5f * ((          2*p1.y) +
		                  t * (( -p0.y           +p2.y) +
		     t * ((2*p0.y -5*p1.y +4*p2.y -p3.y) +
		     t * (  -p0.y +3*p1.y -3*p2.y +p3.y))));
		
		result.x = x; 
		result.y = y;
		
		return result;
	}

	public static List<Vector3> Vector2ToVector3(List<Vector2> list)
	{
		List<Vector3> vector3List = new List<Vector3>();
		foreach(Vector2 v in list)
		{
			vector3List.Add(v);
		}
		return vector3List;
	}

	public static List<Vector2> Vector3ToVector2(List<Vector3> list)
	{
		List<Vector2> vector2List = new List<Vector2>();
		foreach(Vector3 v in list)
		{
			vector2List.Add(v);
		}
		return vector2List;
	}

	public static List<Vector3> CatmullRom(List<Vector3> points)
	{
		return NewCatmullRom<Vector3>(points, Identity, 10, false).ToList();
	}

	private static Vector3 Identity(Vector3 v) {
		return v;
	}

	private static IEnumerable<Vector3> NewCatmullRom<T>(IList nodes, ToVector3<T> toVector3, int slices, bool loop) {
		// need at least two nodes to spline between
		if (nodes.Count >= 2) {

			// yield the first point explicitly, if looping the first point
			// will be generated again in the step for loop when interpolating
			// from last point back to the first point
			yield return toVector3((T)nodes[0]);

			int last = nodes.Count - 1;
			for (int current = 0; loop || current < last; current++) {
				// wrap around when looping
				if (loop && current > last) {
					current = 0;
				}
				// handle edge cases for looping and non-looping scenarios
				// when looping we wrap around, when not looping use start for previous
				// and end for next when you at the ends of the nodes array
				int previous = (current == 0) ? ((loop) ? last : current) : current - 1;
				int start = current;
				int end = (current == last) ? ((loop) ? 0 : current) : current + 1;
				int next = (end == last) ? ((loop) ? 0 : end) : end + 1;

				// adding one guarantees yielding at least the end point
				int stepCount = slices + 1;
				for (int step = 1; step <= stepCount; step++) {
					yield return CatmullRom(toVector3((T)nodes[previous]),
						toVector3((T)nodes[start]),
						toVector3((T)nodes[end]),
						toVector3((T)nodes[next]),
						step, stepCount);
				}
			}
		}
	}

	private static Vector3 CatmullRom(Vector3 previous, Vector3 start, Vector3 end, Vector3 next, 
		float elapsedTime, float duration) {
		// References used:
		// p.266 GemsV1
		//
		// tension is often set to 0.5 but you can use any reasonable value:
		// http://www.cs.cmu.edu/~462/projects/assn2/assn2/catmullRom.pdf
		//
		// bias and tension controls:
		// http://local.wasp.uwa.edu.au/~pbourke/miscellaneous/interpolation/

		float percentComplete = elapsedTime / duration;
		float percentCompleteSquared = percentComplete * percentComplete;
		float percentCompleteCubed = percentCompleteSquared * percentComplete;

		return previous * (-0.5f * percentCompleteCubed +
			percentCompleteSquared -
			0.5f * percentComplete) +
			start   * ( 1.5f * percentCompleteCubed +
				-2.5f * percentCompleteSquared + 1.0f) +
			end     * (-1.5f * percentCompleteCubed +
				2.0f * percentCompleteSquared +
				0.5f * percentComplete) +
			next    * ( 0.5f * percentCompleteCubed -
				0.5f * percentCompleteSquared);
	}


	public delegate Vector3 ToVector3<T>(T v);
	public delegate float Function(float a, float b, float c, float d);
}

