using UnityEngine;
using System.Collections.Generic;
using System;

namespace MeshTools
{
    public class MeshBuilderVO
    {
        public List<Vector2> Points;
        public Action<Shape> CompletedHandler;
        public bool Thread;
        public Color Col;
        public Material Mat;
		public bool Rebuild;
		public Mesh ReusedMesh;
		public Shape ReusedShape;
    }

}