using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MeshTools
{
	public class Line2D : MonoBehaviour {

		private static Line2D _instance;
		private Material _material;
		private Material _basicMaterial;

		public static Line2D Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = GameObject.FindObjectOfType<Line2D>();
					if (_instance == null)
					{
						GameObject go = new GameObject();
						go.name = "Line2D";
						_instance = go.AddComponent<Line2D>();
					}
				}
				
				return _instance;
			}
		}

		public Shape Build(List<Vector2> points, float width, Color color)
		{
			GameObject go = new GameObject();
			Mesh mesh = new Mesh();
			go.AddComponent<MeshFilter>().mesh = mesh;
			MeshRenderer mr = go.AddComponent<MeshRenderer>();

			Material material = mr.material;

			if(_basicMaterial == null)
			{
				_basicMaterial = Resources.Load("Basic2DMaterial") as Material;
			}
			mr.material = _basicMaterial;

			BuildUnlinkedMesh(mesh,points,color,width);
			Shape s = go.AddComponent<Shape>();
			s.Col = color;
			s.Points = points.ToArray();
			s.BuiltGameObject = go;

			return s;
		}

		public void Rebuild(Shape shape, List<Vector2> points, float width)
		{
			Mesh m = shape.BuiltGameObject.GetComponent<MeshFilter>().mesh;
			BuildUnlinkedMesh(m,points,shape.Col,width);
			shape.Points = points.ToArray();
		}

		public void Rebuild(Shape shape, List<Vector2> points, float width, Color color)
		{
			Mesh m = shape.BuiltGameObject.GetComponent<MeshFilter>().mesh;

			BuildUnlinkedMesh(m,points,color,width);
			shape.Col = color;
			shape.Points = points.ToArray();
		}

		private void BuildUnlinkedMesh(Mesh mesh, List<Vector2> points, Color color, float width)
		{
			mesh.Clear();

			List<Vector3> vertices = new List<Vector3>();
			List<Vector2> uvs = new List<Vector2>();
			List<int> tris = new List<int>();
			List<Color> colours = new List<Color>();

			int i = 1, l = points.Count;
			for(;i<l;++i)
			{
				Vector3 point = points[i];
				Vector3 prevPoint = points[i-1];

				Vector3 normal = new Vector2(-(point.y-prevPoint.y),(point.x-prevPoint.x));
		        Vector3 d = normal;
				point.z = 0;
				prevPoint.z = 0;
				d.Normalize();
				int t = vertices.Count;
				tris.Add(t);
				tris.Add(t+1);
				tris.Add(t+2);
				tris.Add(t+1);
				tris.Add(t+3);
				tris.Add(t+2);
				
				vertices.Add(point - d * (width/2f));
				vertices.Add(prevPoint - d* (width/2f));
				vertices.Add(point + d * (width/2f));
				vertices.Add(prevPoint + d * (width/2f));

				uvs.Add(new Vector2(1,0));
				uvs.Add(new Vector2(0,1));
				uvs.Add(new Vector2(1,1));
				uvs.Add(new Vector2(0,0));

				colours.Add(color);
				colours.Add(color);
				colours.Add(color);
				colours.Add(color);
			}

			mesh.vertices = vertices.ToArray();
			mesh.uv = uvs.ToArray();
			mesh.triangles = tris.ToArray();
			mesh.colors = colours.ToArray();
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
		}

		private void BuildLineMesh(List<Vector2> points, Color color, Mesh mesh, float offsetTop, float offsetBottom, bool glow, bool glowTop, bool loop = false, bool trail = false, bool endCaps = false)
		{
			mesh.Clear();
			Vector3 _lastDir = Vector3.up;
			
			int totalVertices = points.Count * 2;
			
			Vector3[] vertices = new Vector3[totalVertices];
			Color32[] colors = new Color32[totalVertices];
			int l = 0;
			
			for (int i = 0; i < totalVertices; i+=2)
			{
				Vector3 p = points[l];
				p.z = 0;

				Vector3 dir = Vector3.right;
				if (i > 0)
				{
					Vector3 prevP = points[l-1];
					prevP.z = 0;
					dir = Vector3.Cross(prevP,p);
					dir.Normalize();
					if (dir == Vector3.zero)
					{
						dir = _lastDir;
					}
					else
					{
						_lastDir = dir;
					}
				}
				else
				{
					if(points.Count > 1)
					{
						Vector3 prevP = points[1];
						prevP.z = 0;
						dir = Vector3.Cross(prevP,p);
						dir.Normalize();
					}
				}



				float oT = offsetTop;// * (d);
				
				float oB = offsetBottom;// * (d);
				
				if(trail)
				{
					oT = oT * ((float)i/(float)points.Count);
					oB = oB * ((float)i/(float)points.Count);
				}

				vertices[i] = p + dir * oT;

				vertices[i+1] = p - dir * oB;
				
				l++;
			}
			
			if(loop)
			{
				vertices[totalVertices-1] = vertices[1];
				vertices[totalVertices-2] = vertices[0];
			}
			else
			{
				// end caps! ...ish
				if(endCaps)
				{
					if(!glow)
					{
						vertices[0] = glowTop? vertices[1] : vertices[0];
						vertices[1] = vertices[0];
						
						vertices[totalVertices-2] = glowTop? vertices[totalVertices-1] : vertices[totalVertices-2];
						vertices[totalVertices-1] = vertices[totalVertices-2];
					}
					else
					{
						vertices[0] = glowTop? vertices[1] : vertices[0];
						vertices[1] = vertices[0];
						
						if(vertices.Length > 2)
						{
							vertices[2] = glowTop? vertices[3] : vertices[2];
							vertices[3] = vertices[2];
							
							vertices[totalVertices-2] = glowTop? vertices[totalVertices-1] : vertices[totalVertices-2];
							vertices[totalVertices-1] = vertices[totalVertices-2];
							
							vertices[totalVertices-4] = glowTop? vertices[totalVertices-3] : vertices[totalVertices-4];
							vertices[totalVertices-3] = vertices[totalVertices-4];
						}
					}
				}
				else
				{
					if(points.Count > 2)
					{
						if(glowTop)
						{
							vertices[0] = Vector3.Lerp(vertices[0],vertices[1], glowTop?0.65f:0.35f);
							vertices[1] = vertices[0];
							
							vertices[totalVertices-2] = Vector3.Lerp(vertices[totalVertices-2],vertices[totalVertices-1], glowTop?0.65f:0.35f);
							vertices[totalVertices-1] = vertices[totalVertices-2];
						}
						else
						{
							vertices[1] = Vector3.Lerp(vertices[0],vertices[1], glowTop?0.65f:0.35f);
							vertices[0] = vertices[1];
							
							vertices[totalVertices-2] = Vector3.Lerp(vertices[totalVertices-2],vertices[totalVertices-1], glowTop?0.65f:0.35f);
							vertices[totalVertices-1] = vertices[totalVertices-2];
						}
						//                vertices[1] = vertices[0];
					}
				}
			}
			
			for (int i = 0; i < totalVertices; i++)
			{
				colors[i] = color;
				
				if(glow)
				{
					if(i%2 == 0 && glowTop)
					{
						Color c = color;
						c.a = 0;
						colors[i] = c;
					}
					else if (i%2 != 0 && !glowTop)
					{
						Color c = color;
						c.a = 0;
						colors[i] = c;
					}
					
				}
			}
			
			if(trail)
			{
				int i = 1; l = totalVertices <10 ? totalVertices : 10;
				for(;i<l;++i)
				{
					Color c = colors[totalVertices-i];
					c.a = 0;
					colors[totalVertices-i] = c;
				}
			}
			if(!loop && glow)
			{
				colors[0] = Color.clear;
				colors[1] = Color.clear;
				
				colors[totalVertices-1] = Color.clear;
				colors[totalVertices-2] = Color.clear;
			}
			mesh.vertices = vertices;
			mesh.colors32 = colors;
			
			Vector2[] uv = new Vector2[totalVertices];
			for (int i = 0; i < totalVertices; i++)
			{
				if (i % 2 == 0)
				{
					uv[i] = new Vector2(i * 0.5f, 0);
				}
				else
				{
					uv[i] = new Vector2((i - 1) * 0.5f, 1);
				}
			}
			mesh.uv = uv;
			
			int triangleCount = totalVertices - 2;
			int[] triangles = new int[triangleCount * 3];
			int triangleIndex = 0;
			for (int t = 0; t < triangleCount; t++)
			{           
				if (t % 4 == 0)
				{
					triangles[triangleIndex] = t;
					triangles[triangleIndex + 1] = t + 2;
					triangles[triangleIndex + 2] = t + 1;
				}
				else
				{
					triangles[triangleIndex] = t;
					triangles[triangleIndex + 1] = t + 1;
					triangles[triangleIndex + 2] = t + 2;
				}
				triangleIndex += 3;
			}
			mesh.triangles = triangles;
		}
	}
}