using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MeshTools;
using UnityEngine.EventSystems;

public class ShapeBuilding : MonoBehaviour {

	private Shape _shape;
	private List<Vector2> _points;

	// Use this for initialization
	void Start () {
		Reset();
	}

	public void Reset()
	{
		if(_shape != null)
		{
			Destroy(_shape.BuiltGameObject);
		}

		_points = new List<Vector2>();
		
		_points.Add(new Vector2(1,1));
		_points.Add(new Vector2(1,-1));
		_points.Add(new Vector2(-1,-1));
		_points.Add(new Vector2(-1,1));
		
		Mesh2D.Instance.Build(_points,ShapeBuilt,Color.black);
	}

	private void ShapeBuilt(Shape shape)
	{
		_shape = shape;
	}
	
	// Update is called once per frame
	void Update () {
		
		if(Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
		{
			AddPointToShape();
		}
	}

	private void AddPointToShape()
	{
		_points.Add(Camera.main.ScreenToWorldPoint(Input.mousePosition));
		Mesh2D.Instance.ReBuild(_shape.BuiltGameObject,_points,ShapeBuilt,Color.black);
	}
}
