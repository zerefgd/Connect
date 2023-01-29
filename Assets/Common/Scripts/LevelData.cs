using System.Collections.Generic;
using UnityEngine;

namespace Connect.Common
{
    [CreateAssetMenu(fileName = "Level",menuName = "SO/Level")]
    public class LevelData : ScriptableObject
    {
        public string LevelName;
        public List<Edge> Edges;
    }

    [System.Serializable]
    public struct Edge
    {
        public List<Vector2Int> Points;
        public Vector2Int StartPoint
        {
            get 
            {
                if(Points!=null && Points.Count > 0) 
                {
                    return Points[0];
                }
                return new Vector2Int(-1, -1);
            }
        }
        public Vector2Int EndPoint
        {
            get
            {
                if (Points != null && Points.Count > 0)
                {
                    return Points[Points.Count - 1];
                }
                return new Vector2Int(-1, -1);
            }
        }
    }
}
