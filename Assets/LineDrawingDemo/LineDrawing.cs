using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MeshTools;

public class LineDrawing : MonoBehaviour {

	private const float WIDTH = 0.5f;

	private Shape _lineShape;
	private List<Vector2> _linePoints;
	private Vector3 _lastMousePosition;

	private Shape _startCap;
	private Shape _endCap;
	
	// Update is called once per frame
	void Update () {
	
		if(Input.GetMouseButtonDown(0))
		{
			StartLineDrawing();
		}

		if(Input.GetMouseButton(0))
		{
			UpdateLineDrawing();
		}

	}

	private void StartLineDrawing()
	{
		_linePoints = new List<Vector2>();
		_lineShape = Line2D.Instance.BuildLinkedSpline<BaseLineStyle>(_linePoints,WIDTH, Color.black,null);

		_startCap = Circle2D.Instance.Build(WIDTH*0.5f,100,Color.black);
		_endCap = Circle2D.Instance.Build(WIDTH*0.5f,100,Color.black);

		GameObject line = new GameObject();
		line.name = "Line";
		_lineShape.BuiltGameObject.transform.SetParent(line.transform);
		_startCap.BuiltGameObject.transform.SetParent(line.transform);
		_endCap.BuiltGameObject.transform.SetParent(line.transform);
	}

	private void UpdateLineDrawing()
	{
		if (Vector3.Distance (Input.mousePosition, _lastMousePosition) > 8) 
		{
			_linePoints.Add( Camera.main.ScreenToWorldPoint(Input.mousePosition));
			Line2D.Instance.RebuildLinkedSpline(_lineShape,_linePoints,WIDTH);

			_lastMousePosition = Input.mousePosition;
		}

		_startCap.BuiltGameObject.transform.position = _linePoints[0];
		_endCap.BuiltGameObject.transform.position = _linePoints[_linePoints.Count-1];
	}
}
