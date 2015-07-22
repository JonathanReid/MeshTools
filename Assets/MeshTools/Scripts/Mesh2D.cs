using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Poly2Tri;

namespace MeshTools
{
    public class Mesh2D : MonoBehaviour
    {
        private const int MINIMUM_POINTS_FOR_MESH = 4;
        private const int MINIMUM_ALLOWED_VERTICES = 4;
        private static Mesh2D _instance;
        private Material _material;
        private Material _basicMaterial;
        private Color _vertexColor;
        private Shape _lastCreatedObject;
        private Polygon _polygon;
        private Vector2 _lowerBound;
        private Vector2 _upperBound;
        private Vector2 _boundingBox;
        private Vector2 _uvBounds;
        private Vector3[] _vertices;
        private Vector2[] _uvs;
        private Vector3[] _normals;
        private Color32[] _colors;
        private int[] _tris;
        private List<Vector2> _points;
        private Action<Shape> _finishedAction;
        private bool _useThread;
        private List<MeshBuilderVO> _queue;
        private bool _running;
        private object _threadID;
		private Mesh _reusedMesh;
		private Shape _reusedShape;
		private bool _rebuild;

        public static Mesh2D Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<Mesh2D>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject();
                        go.name = "Mesh2D";
                        _instance = go.AddComponent<Mesh2D>();
                    }
                }

                return _instance;
            }
        }

        private void Build(List<Vector2> points, Action<Shape> completedHandler)
        {
            Build(points, completedHandler, Color.white);
        }

        private void Build(List<Vector2> points, Action<Shape> completedHandler, bool useThread)
        {
			Build(points, completedHandler, Color.white, null, useThread, false);
        }

        public void Build(List<Vector2> points, Action<Shape> completedHandler, Color color)
        {
			Build(points, completedHandler, color, null, false, false);
        }

        public void Build(List<Vector2> points, Action<Shape> completedHandler, Color color, bool useThread)
        {
			Build(points, completedHandler, color, null, useThread, false);
        }

        public void Build(List<Vector2> points, Action<Shape> completedHandler, Color color, Material material)
        {
            Build(points, completedHandler, color, material, false, false);
        }

		public void ReBuild(GameObject obj, List<Vector2> points, Action<Shape> completedHandler)
		{
			Build(points, completedHandler, Color.white,null,false,true,obj);
		}

		private void ReBuild(GameObject obj, List<Vector2> points, Action<Shape> completedHandler, bool useThread)
		{
			Build(points, completedHandler, Color.white, null, useThread, true,obj);
		}
		
		public void ReBuild(GameObject obj, List<Vector2> points, Action<Shape> completedHandler, Color color)
		{
			Build(points, completedHandler, color, null, false, true,obj);
		}
		
		public void ReBuild(GameObject obj, List<Vector2> points, Action<Shape> completedHandler, Color color, bool useThread)
		{
			Build(points, completedHandler, color, null, useThread, true,obj);
		}
		
		public void ReBuild(GameObject obj, List<Vector2> points, Action<Shape> completedHandler, Color color, Material material)
		{
			Build(points, completedHandler, color, material, false, true,obj);
		}

        public void Build(List<Vector2> points, Action<Shape> completedHandler, Color color, Material material, bool useThread, bool rebuild, GameObject obj = null)
        {
            if (_queue == null)
            {
                _queue = new List<MeshBuilderVO>();
            }

            MeshBuilderVO vo = new MeshBuilderVO();

            vo.Points = points;
            vo.CompletedHandler = completedHandler;
            vo.Thread = useThread;
            vo.Col = color;
			vo.Rebuild = rebuild;
			if(rebuild)
			{
				vo.ReusedShape = obj.GetComponent<Shape>();
				vo.ReusedMesh = obj.GetComponent<MeshFilter>().sharedMesh;
			}

            if (material == null)
            {
                if(_basicMaterial == null)
                {
                    _basicMaterial = Resources.Load("Basic2DMaterial") as Material;
                }
                vo.Mat = _basicMaterial;
            } 
            else
            {
                vo.Mat = material;
            }
            _queue.Add(vo);
            if (!_running)
            {
                ProcessQueue();
            }
        }

        private void ProcessQueue()
        {
            if (_queue.Count > 0 && !_running)
            {
                MeshBuilderVO vo = _queue [0];

                _running = true;
                _finishedAction = vo.CompletedHandler;
                _useThread = vo.Thread;
                _vertexColor = vo.Col;
                _material = vo.Mat;
				_rebuild = vo.Rebuild;
				_reusedMesh = vo.ReusedMesh;
				_reusedShape = vo.ReusedShape;
                Build(vo.Points);
            }
        }

        private void Build(List<Vector2> points)
        {

            _lastCreatedObject = null;

            Vector2[] temp = new Vector2[points.Count];
            points.CopyTo(temp);
            _points = new List<Vector2>(temp);

            if (_points.Count < MINIMUM_ALLOWED_VERTICES)
            {
                InvalidShapeCreated();
                return;
            }

            if (_useThread)
            {
                StartCoroutine(ThreadManager.Start(StartConstruction, MeshConstructionComplete, 0));
            } else
            {
                StartConstruction(-1);
            }
        }

        private void StartConstruction(object threadID)
        {
            _threadID = threadID;

            ConstructPolygon();
        }

        private void ConstructPolygon()
        {
            List<PolygonPoint> p2 = new List<PolygonPoint>();
            int i = 0, l = _points.Count;
            for (; i < l; i += 1)
            {
                p2.Add(new PolygonPoint(_points [i].x, _points [i].y));
            }
        
            _polygon = new Polygon(p2);
			_polygon.Simplify();
            P2T.Triangulate(_polygon);

            ContinueCreatingShape();
        }

        private void ContinueCreatingShape()
        {
            AssignBoundsToPolygon();

            ThreadManager.Stop((int)_threadID);
            if (_useThread)
            {

            } else
            {
                MeshConstructionComplete();
            }
        }

        private void MeshConstructionComplete()
        {
            _useThread = false;
            ConstructMeshData();

			if(_rebuild)
			{
				ReassignDataToMesh();
			}
			else
			{
            	AssignDataToMesh();
			}
        
            if (_points.Count < MINIMUM_POINTS_FOR_MESH)
            {
                InvalidShapeCreated();
            }

            _queue [0] = null;
            _queue.RemoveAt(0);
            _running = false;

			if(_finishedAction != null)
			{
            	_finishedAction(_lastCreatedObject);
			}

            ProcessQueue();
        }

        private void InvalidShapeCreated()
        {
            if (_lastCreatedObject != null)
            {
                Destroy(_lastCreatedObject.BuiltGameObject);
            }
            _lastCreatedObject = null;
            FinishQueue();
//            throw new System.InvalidOperationException("Constructed points were invalid.");
        }

        private void FinishQueue()
        {
            _queue [0] = null;
            _queue.RemoveAt(0);
            _running = false;
            ProcessQueue();
        }


		public static Vector2 UVBounds;
		public static bool UseBoundingBoxUVs = true;
        private void AssignBoundsToPolygon()
        {
            _lowerBound = new Vector2((float)_polygon.MinX, (float)_polygon.MinY);
            _upperBound = new Vector2((float)_polygon.MaxX, (float)_polygon.MaxY);

            _boundingBox = _upperBound - _lowerBound;

			_uvBounds.x = UVBounds.x;
			_uvBounds.y = UVBounds.y;
            
        }

        private void ConstructMeshData()
        {
            int vertCount = (_polygon.Triangles.Count * 3);
            int triCount = (_polygon.Triangles.Count * 3);

            _vertices = new Vector3[vertCount];
            _colors = new Color32[_vertices.Length];
            _uvs = new Vector2[_vertices.Length];
            _tris = new int[triCount];
            int i = 0;
            int j = (_polygon.Triangles.Count * 3);


            foreach (DelaunayTriangle triangle in _polygon.Triangles)
            {
                foreach (TriangulationPoint tp in triangle.Points)
                {
                    _vertices [i] = new Vector3(tp.Xf, tp.Yf, 0);
               
                    _colors [i] = _vertexColor;

                    Vector2 relativePoint = new Vector2(tp.Xf, tp.Yf) - _lowerBound;
                    relativePoint = transform.TransformPoint(relativePoint);

					Vector2 b = UseBoundingBoxUVs ? _boundingBox : _uvBounds;

                    _uvs [i] = new Vector2(relativePoint.x / b.x, relativePoint.y / b.y);

                    i++;
                    j++;
                }
            }
            i = 0;
            j = 0;
            int l = _polygon.Triangles.Count;

            //building front and back faces.
            for (; i<l; ++i)
            {
                _tris [j] = j + 2;
                _tris [j + 1] = j + 1;
                _tris [j + 2] = j;

                j += 3;
            }

        }

        private void AssignDataToMesh()
        {
            Mesh msh = new Mesh();
            msh.vertices = _vertices;
            msh.uv = _uvs;
            msh.colors32 = _colors;
            msh.triangles = _tris;
            msh.RecalculateNormals();
            msh.Optimize();
            msh.RecalculateBounds();
            GameObject go = new GameObject();
            MeshFilter filter = go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>().material = _material;
            go.transform.name = "DrawnObject";
            filter.mesh = msh;
            msh.name = "DrawnObjectMesh";

            Shape shape = go.AddComponent<Shape>();
            shape.BuiltGameObject = go;
            shape.BoundingBox = _boundingBox;
            shape.UVBounds = _uvBounds;
            shape.Area = _boundingBox.x * _boundingBox.y;
            shape.Points = _points.ToArray();
            shape.Polygon = _polygon;
			shape.Col = _vertexColor;

            _lastCreatedObject = shape;

        }

		private void ReassignDataToMesh()
		{
			_reusedMesh.Clear();
			_reusedMesh.vertices = _vertices;
			_reusedMesh.uv = _uvs;
			_reusedMesh.colors32 = _colors;
			_reusedMesh.triangles = _tris;
			_reusedMesh.RecalculateNormals();
			_reusedMesh.Optimize();
			_reusedMesh.RecalculateBounds();


			_reusedShape.BuiltGameObject.GetComponent<MeshRenderer>().material = _material;
			_reusedShape.BoundingBox = _boundingBox;
			_reusedShape.UVBounds = _uvBounds;
			_reusedShape.Area = _boundingBox.x * _boundingBox.y;
			_reusedShape.Points = _points.ToArray();
			_reusedShape.Polygon = _polygon;
			_reusedShape.Col = _vertexColor;
			
			_lastCreatedObject = _reusedShape;
		}
    }
}