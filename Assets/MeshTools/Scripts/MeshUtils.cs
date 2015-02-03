using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
}

