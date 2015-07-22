using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace MeshTools
{
    public class Circle2D : MonoBehaviour
    {

        private Material _material;
        private Material _basicMaterial;
        private Color32 _vertexColor;
        private static Circle2D _instance;

        public static Circle2D Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<Circle2D>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject();
                        go.name = "Circle2D";
                        _instance = go.AddComponent<Circle2D>();
                    }
                }
                return _instance;
            }
        }

        public Shape Build(float radius, int segments)
        {
            return BuildCircle(GeneratePointList(radius, segments), radius, Color.white, null);
        }

        public Shape Build(float radius, int segments, Color color)
        {
            return BuildCircle(GeneratePointList(radius, segments), radius, color, null);
        }

        public Shape Build(float radius, int segments, Color color, Material material)
        {
            return BuildCircle(GeneratePointList(radius, segments), radius, color, material);
        }

        public Shape Build(float radius, int segments, float percentFilled)
        {
            return BuildCircle(GeneratePointList(radius, segments, false, 0, percentFilled), radius, Color.white, null);
        }

        public Shape Build(float radius, int segments, Color color, float percentFilled)
        {
            return BuildCircle(GeneratePointList(radius, segments, false, 0, percentFilled), radius, color, null);
        }

        public Shape Build(float radius, int segments, Color color, Material material, float percentFilled)
        {
            return BuildCircle(GeneratePointList(radius, segments, false, 0, percentFilled), radius, color, material);
        }
        
        public Shape Build(float radius, int segments, float innerHoleRadius, float percentFilled)
        {
            return BuildCircle(GeneratePointList(radius, segments, true, innerHoleRadius, percentFilled), radius, Color.white, null);
        }

        public Shape Build(float radius, int segments, Color color, float innerHoleRadius, float percentFilled)
        {
            return BuildCircle(GeneratePointList(radius, segments, true, innerHoleRadius, percentFilled), radius, color, null);
        }

        public Shape Build(float radius, int segments, Color color, Material material, float innerHoleRadius, float percentFilled)
        {
            return BuildCircle(GeneratePointList(radius, segments, true, innerHoleRadius, percentFilled), radius, color, material);
        }

        //rebuild an existing mesh
        public void Rebuild(GameObject obj, float radius, int segments)
        {
            RebuildCircle(obj, GeneratePointList(radius, segments), radius);
        }
        
        public void Rebuild(GameObject obj, float radius, int segments, float percentFilled)
        {
            RebuildCircle(obj, GeneratePointList(radius, segments, false, 0, percentFilled), radius);
        }

        public void Rebuild(GameObject obj, float radius, int segments, float innerHoleRadius, float percentFilled)
        {
            RebuildCircle(obj, GeneratePointList(radius, segments, true, innerHoleRadius, percentFilled), radius);
        }

        private List<Vector2> GeneratePointList(float rad, int seg, bool construcInnerHole = false, float innerHoleRadius = 0, float percentFilled = 100)
        {
            float radius = innerHoleRadius;
            
            float radius1 = rad;
            int segments = seg;
            
            List<Vector2> points = new List<Vector2>();
            float angle = (3.6f * percentFilled) / segments;
            float a = angle;
            
            int i = 0, l = segments;
            
            for (; i<l; ++i)
            {
                Vector2 pos = Vector2.zero;
                
                if (construcInnerHole)
                {
                    pos = new Vector2(0 + radius * Mathf.Sin(angle * Mathf.Deg2Rad), 0 + radius * Mathf.Cos(angle * Mathf.Deg2Rad));
                    points.Add(pos);
                } else
                {
                    points.Add(pos);
                }
                
                
                pos = new Vector2(0 + radius1 * Mathf.Sin(angle * Mathf.Deg2Rad), 0 + radius1 * Mathf.Cos(angle * Mathf.Deg2Rad));
                points.Add(pos);
                
                angle += a;
            }
            if (percentFilled == 100)
            {
                points.Add(points [0]);
                points.Add(points [1]);
            }
            
            return points;
        }
        
        private List<Vector2> ExtendPoints(int startVal, List<Vector2> points, float radius, int dir)
        {
            int i = 0, l = points.Count;
            List<Vector3> p = new List<Vector3>();
            for (; i<l; i++)
            {
                p.Add(points [i]);
            }
            return ExtendPoints(startVal, p, radius, dir);
        }
        
        private List<Vector2> ExtendPoints(int startVal, List<Vector3> points, float radius, int dir)
        {
            int i = startVal, l = points.Count;
            List<Vector2> p = new List<Vector2>();
            Vector2 pos = Vector2.zero;
            for (; i<l; i+=2)
            {
                pos = points [i];
                if (dir > 0)
                {
                    pos += (pos - Vector2.zero).normalized * (radius / 60);
                } else
                {
                    pos -= (pos - Vector2.zero).normalized * (radius / 60);
                }
                
                p.Add(pos);
                p.Add(points [i]);
            }
            return p;
        }

        private Shape BuildCircle(List<Vector2> _points, float rad, Color color, Material material)
        {
            _vertexColor = color;
            if (material == null)
            {
                if(_basicMaterial == null)
                {
                    _basicMaterial = Resources.Load("Basic2DMaterial") as Material;
                }

                _material = _basicMaterial;
            } 
            else
            {
                _material = material;
            }

            GameObject outer = BuildNewMesh(ExtendPoints(1, _points, rad, 1), true);
            outer.name = "CircleOuter";
            GameObject inner = BuildNewMesh(ExtendPoints(0, _points, rad, -1), true);
            inner.name = "CircleInner";
            GameObject go = BuildNewMesh(_points);
            outer.transform.parent = go.transform;
            inner.transform.parent = go.transform;
            
            Shape shape = go.AddComponent<Shape>();
            shape.BuiltGameObject = go;
            
            Vector2 b = AssignBoundsToPolygon(go);
            shape.BoundingBox = b;
            shape.Area = b.x * b.y;
            shape.Col = _vertexColor;
            
            List<Vector2> points = new List<Vector2>();
            int l = _points.Count;
            for (int i = 0; i<l; ++i)
            {
                if (i % 2 != 0)
                    points.Add(_points [i]);
            }
            
            shape.Points = points.ToArray();
            
            return shape;
        }
        
        private void RebuildCircle(GameObject obj, List<Vector2> _points, float rad)
        {
            Mesh outerMesh = obj.transform.FindChild("CircleOuter").GetComponent<MeshFilter>().mesh;
            Mesh innerMesh = obj.transform.FindChild("CircleInner").GetComponent<MeshFilter>().mesh;
            Mesh mesh = obj.GetComponent<MeshFilter>().mesh;
			Shape shape =  obj.GetComponent<Shape>();
            
			_vertexColor = shape.Col;

            outerMesh = UpdateMesh(outerMesh, ExtendPoints(1, _points, rad, 1), true);
            innerMesh = UpdateMesh(innerMesh, ExtendPoints(1, _points, rad, -1), true);
            mesh = UpdateMesh(mesh, _points);


			List<Vector2> points = new List<Vector2>();
			int l = _points.Count;
			for (int i = 0; i<l; ++i)
			{
				if (i % 2 != 0)
					points.Add(_points [i]);
			}

			shape.Points = points.ToArray();
		}
		
		private GameObject BuildNewMesh(List<Vector2> _points, bool fade = false)
        {
            Mesh mesh = new Mesh();
            
            UpdateMesh(mesh, _points, fade);
            
            GameObject go = new GameObject();
            go.AddComponent<MeshFilter>().mesh = mesh;
            go.AddComponent<MeshRenderer>().material = _material;
            go.name = "Circle";
            
            return go;
        }
        
        private Mesh UpdateMesh(Mesh mesh, List<Vector2> _points, bool fade = false)
        {
            mesh.Clear();
            int totalVertices = _points.Count;
            
            Vector3[] vertices = new Vector3[totalVertices];
            Color32[] colors = new Color32[totalVertices];
            Vector3[] normals = new Vector3[totalVertices];
            
            for (int i = 0; i < totalVertices; i++)
            {
                vertices [i] = _points [i];
                if (fade && i % 2 == 0)
                {
                    Color32 c = _vertexColor;
                    c.a = 0;
                    colors [i] = c;
                } else
                {
                    colors [i] = _vertexColor;
                }
                normals [i] = vertices [i].normalized;
            }
            
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.colors32 = colors;
            
            Vector2[] uv = new Vector2[totalVertices];
            for (int i = 0; i < totalVertices; i++)
            {
                if (i % 2 == 0)
                {
                    uv [i] = new Vector2(i * 0.5f, 0);
                } else
                {
                    uv [i] = new Vector2((i - 1) * 0.5f, 1);
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
                    triangles [triangleIndex] = t;
                    triangles [triangleIndex + 1] = t + 2;
                    triangles [triangleIndex + 2] = t + 1;
                } else
                {
                    triangles [triangleIndex] = t;
                    triangles [triangleIndex + 1] = t + 1;
                    triangles [triangleIndex + 2] = t + 2;
                }
                triangleIndex += 3;
            }
            mesh.triangles = triangles;
            
            return mesh;
        }
        
        private Vector2 AssignBoundsToPolygon(GameObject obj)
        {
            Renderer rend = obj.GetComponent<Renderer>();
            Vector2 lowerBound = new Vector2((float)rend.bounds.min.x, (float)rend.bounds.min.y);
            Vector2 upperBound = new Vector2((float)rend.bounds.max.x, (float)rend.bounds.max.y);
            
            Vector2 boundingBox = upperBound - lowerBound;
            
            return boundingBox;
        }
    }
}