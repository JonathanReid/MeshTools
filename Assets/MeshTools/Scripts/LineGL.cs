using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MeshTools
{
	public class LineGLVO : ScriptableObject {
		
		public List<Vector3> Points;
		public List<Vector3> DrawnPoints;
		public Color Color;
		public bool AutoDelete;
		public float Width;
		public bool VariableWidth;
		public bool Stretch;
		public bool AA;
		public bool Continuous;
	}

	public class LineGL : MonoBehaviour {

		private List<LineGLVO> _vos = new List<LineGLVO>();
		private List<LineGLVO> _removing = new List<LineGLVO>();
		
		public Material LineMaterial;

		public void ClearLines()
		{
			_vos = new List<LineGLVO>();
		}

		public void ClearLine(LineGLVO key)
		{
			_vos.Remove(key);
		}

		public LineGLVO Build(List<Vector3> points, float width, Color color, bool autoDelete = true, bool AA = false)
		{
			if(points.Count == 0)
				return null;

			LineGLVO vo = ScriptableObject.CreateInstance<LineGLVO>();
			vo.Points = points;
			vo.Color = color;
			vo.AutoDelete = autoDelete;
			vo.Width = width;
			vo.AA = AA;
			_vos.Add(vo);
			return vo;
		}

		public LineGLVO BuildContinuousLine(List<Vector3> points, float width, Color color, bool autoDelete = true, bool AA = false)
		{
			if(points.Count == 0)
				return null;
			
			LineGLVO vo = ScriptableObject.CreateInstance<LineGLVO>();
			vo.Points = points;
			vo.Color = color;
			vo.AutoDelete = autoDelete;
			vo.Width = width;
			vo.AA = AA;
			vo.Continuous = true;
			_vos.Add(vo);

			return vo;
		}

		public LineGLVO BuildSpline(List<Vector3> points, float width, Color color, bool autoDelete = true, bool AA = false, bool continuous = false, bool stretch = false)
		{
			if(points.Count == 0)
				return null;
			
			LineGLVO vo = ScriptableObject.CreateInstance<LineGLVO>();

			List<Vector3> p = new List<Vector3>(points);
			List<Vector3> results = new List<Vector3>();

			if(continuous)
			{
				p.Remove(p[p.Count-1]);
				results = MeshUtils.CatmullRom(p);
				results.Add(results[0]);
			}
			else
			{
				results = MeshUtils.CatmullRom(p);
			}
			vo.Points = results;

			vo.Color = color;
			vo.AutoDelete = autoDelete;
			vo.Width = width;
			vo.VariableWidth = true;
			vo.Stretch = stretch;
			vo.AA = AA;
			vo.Continuous = true;
			_vos.Add(vo);

			return vo;
		}

		public LineGLVO BuildStretchLine(List<Vector3> points, float width, Color color, bool autoDelete = true, bool AA = false, bool continuous = false)
		{
			if(points.Count == 0)
				return null;
			
			LineGLVO vo = ScriptableObject.CreateInstance<LineGLVO>();

			vo.Points = MeshUtils.CatmullRom(points);

			vo.Color = color;
			vo.AutoDelete = autoDelete;
			vo.Width = width;
			vo.VariableWidth = false;
			vo.AA = AA;
			vo.Continuous = true;
			vo.Stretch = true;
			_vos.Add(vo);
			
			return vo;
		}



		private Vector3 _previousLeftVertice;
		private Vector3 _previousRightVertice;
	
		void OnPostRender()
		{
			_removing = new List<LineGLVO>();

			System.Random rand = new System.Random(1);


			GL.PushMatrix();
			LineMaterial.SetPass(0);
			GL.Begin(GL.QUADS);
			int i = 0, l = _vos.Count;
			for(;i<l;++i)
			{
				_previousLeftVertice = Vector3.zero;
				_previousRightVertice = Vector3.zero;
				LineGLVO vo = _vos[i];
				if(vo.Points != null)
				{
					int j = 1, k = vo.Points.Count;
					for(;j<k;++j) 
					{

						GL.Color(vo.Color);
						Vector3 p = (vo.Points[j-1] - vo.Points[j]).normalized;

						Vector3 prevP = vo.Points[j-1];
						Vector3 point = vo.Points[j];

						p = Vector3.Cross(point - prevP, Vector3.forward);
						p.Normalize();

						Vector3 v = prevP;

						float width = vo.Width;

						if(vo.VariableWidth)
						{
							width = vo.Width + ((Mathf.Sin(((j+(rand.Next(-20,20))/5f)) )/50f)- vo.Width/4);
						}

						if(vo.Stretch && j > k-10)
						{
							width = vo.Width - ((j-(k-10)) /80f) - Vector3.Distance(point, vo.Points[k-10])/10f;
						}
						if(vo.Stretch && j < 10)
						{
							width = vo.Width - Mathf.Abs(j-10)/50f;
						}

						if((_previousLeftVertice == Vector3.zero
						   && _previousRightVertice == Vector3.zero)
						   || !vo.Continuous)
						{
							GL.Vertex(v - p * (width*0.5f));
							GL.Vertex(v + p * (width*0.5f));
						}
						else
						{
							GL.Vertex(_previousLeftVertice);
							GL.Vertex(_previousRightVertice);
						}

						v = vo.Points[j];
						GL.Vertex(v + p * (width*0.5f));
						GL.Vertex(v - p * (width*0.5f));

						_previousLeftVertice = v - p * (width*0.5f);
						_previousRightVertice = v + p * (width*0.5f);

						if(vo.AA)
						{
							//adding fake Anti aliasing to the line by adding fading quads to the edge.
//							float width = 0.4f;
//							v = vo.Points[j-1];
//							Vector3 v1 = vo.Points[j];
//							GL.Vertex(v + p * (vo.Width*width));
//							GL.Vertex(v1 + p * (vo.Width*width));
//							Color c = vo.Color;
//							c.a = 0;
//							GL.Color(c);
//							GL.Vertex(v1 + p * ((vo.Width*width) + vo.Width*width));
//							GL.Vertex(v + p * (vo.Width*width + vo.Width*width));
//
//							GL.Color(vo.Color);
//							GL.Vertex(v - p * (vo.Width*width));
//							GL.Vertex(v1 - p * (vo.Width*width));
//							GL.Color(c);
//							GL.Vertex(v1 - p * ((vo.Width*width) + vo.Width*width));
//							GL.Vertex(v - p * (vo.Width*width + vo.Width*width));



//							GL.Color(Color.red);
//							GL.Vertex(v - p * (vo.Width*0.5f));
//							GL.Vertex(v - (p1 * (vo.Width*0.5f)));
//							GL.Color(Color.red);
//							GL.Vertex(v + p * (vo.Width*0.5f));
//							GL.Vertex(v + (p1 * (vo.Width*0.5f)));
						}


					}
				}
				if(vo.AutoDelete)
				{
					_removing.Add(vo);
				}
			}

			GL.End();
			GL.PopMatrix();
		}

		private void RemoveOldData(List<LineGLVO> vosToRemove)
		{
			try
			{
				int i = 0, l = vosToRemove.Count;
				for(;i<l;++i)
				{
					int index = _vos.IndexOf(vosToRemove[i]);
					Destroy(_vos[index]);
					_vos.RemoveAt(index);
				}
			}
			catch{}
		}

		void LateUpdate()
		{
			RemoveOldData(_removing);
		}
	}
}