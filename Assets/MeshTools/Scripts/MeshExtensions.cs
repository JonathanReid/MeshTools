using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using ClipperLib;
using System.Linq;

namespace MeshTools
{
	public static class MeshExtensions {

		public static List<Vector2> Add (this Shape shape, Shape secondShape, Action<Shape> completed)
		{
			List<Vector2> points = new List<Vector2>();
			
			Clipper c = new Clipper();
			
			List<List<IntPoint>> subj = new List<List<IntPoint>>();
			List<List<IntPoint>> clip = new List<List<IntPoint>>();
			List<List<IntPoint>> solution = new List<List<IntPoint>>();
			List<IntPoint> p1 = new List<IntPoint>();
			List<IntPoint> p2 = new List<IntPoint>();
			int i = 0, l = shape.Points.Length;
			Vector2 pos = shape.BuiltGameObject.transform.position;
			for(;i<l;++i)
			{
				IntPoint ip = new IntPoint(shape.Points[i].x + pos.x,shape.Points[i].y + pos.y);
				p1.Add(ip);
			}
			p1.Add(p1[0]);
			
			pos = secondShape.BuiltGameObject.transform.position;
			i = 0; l = secondShape.Points.Length;
			for(;i<l;++i)
			{
				IntPoint ip = new IntPoint(secondShape.Points[i].x + pos.x,secondShape.Points[i].y + pos.y);
				p2.Add(ip);
			}
			p2.Add(p2[0]);
			
			subj.Add(p1);
			clip.Add(p2);
			
			
			c.AddPaths(subj,PolyType.ptSubject,true);
			c.AddPaths(clip,PolyType.ptClip,true);
			c.Execute(ClipType.ctUnion,solution);


			i = 0; l = solution[0].Count;
			for(;i<l;++i)
			{
				float x = System.Convert.ToSingle(solution[0][i].X);
				float y = System.Convert.ToSingle(solution[0][i].Y);
				points.Add(new Vector2(x,y));
			}
			
			Mesh2D.Instance.ReBuild(shape.BuiltGameObject,points,completed,shape.Col);
			return points;
		}

		public static List<Vector2> Subtract (this Shape shape, Shape secondShape, Action<Shape> completed)
		{
		
			List<Vector2> points = new List<Vector2>();

			Clipper c = new Clipper();

			List<List<IntPoint>> subj = new List<List<IntPoint>>();
			List<List<IntPoint>> clip = new List<List<IntPoint>>();
			List<List<IntPoint>> solution = new List<List<IntPoint>>();
			List<IntPoint> p1 = new List<IntPoint>();
			List<IntPoint> p2 = new List<IntPoint>();
			int i = 0, l = shape.Points.Length;
			Vector2 pos = shape.BuiltGameObject.transform.position;
			for(;i<l;++i)
			{
				IntPoint ip = new IntPoint(shape.Points[i].x + pos.x,shape.Points[i].y + pos.y);
				p1.Add(ip);
			}
			p1.Add(p1[0]);

			pos = secondShape.BuiltGameObject.transform.position;
			i = 0; l = secondShape.Points.Length;
			for(;i<l;++i)
			{
				IntPoint ip = new IntPoint(secondShape.Points[i].x + pos.x,secondShape.Points[i].y + pos.y);
				p2.Add(ip);
			}
			p2.Add(p2[0]);

			subj.Add(p1);
			clip.Add(p2);


			c.AddPaths(subj,PolyType.ptSubject,true);
			c.AddPaths(clip,PolyType.ptClip,true);
			c.Execute(ClipType.ctDifference,solution);


			if(solution.Count == 0)
			{
				MonoBehaviour.Destroy(shape.BuiltGameObject);
			}

			int j = 0, k = solution.Count;
			for(;j<k;++j)
			{
				points = new List<Vector2>();
				i = 0; l = solution[j].Count;
				for(;i<l;++i)
				{
					points.Add(new Vector2(solution[j][i].X,solution[j][i].Y));
				}

				if(j == 0)
				{
					Mesh2D.Instance.ReBuild(shape.BuiltGameObject,points,completed,shape.Col);
				}
				else
				{
					Mesh2D.Instance.Build(points,completed,shape.Col);
				}
			}
			return points;
		}
	}
}