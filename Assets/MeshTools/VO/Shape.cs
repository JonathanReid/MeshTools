﻿using UnityEngine;
using System.Collections;
using Poly2Tri;

namespace MeshTools
{
    public class Shape : MonoBehaviour
    {
        public GameObject BuiltGameObject;
        public Vector2 BoundingBox;
        public Vector2 UVBounds;
        public float Area;
        public Vector2[] Points;
        public Polygon Polygon;
        public float Radius;
		public Color Col;
    }
}
