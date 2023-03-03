using System.Collections.Generic;
using UnityEngine;

namespace Connect.Generator
{
    public class NodeRenderer : MonoBehaviour
    {
        [SerializeField] private List<Color> NodeColors;

        [SerializeField] private GameObject _point;
        [SerializeField] private GameObject _topEdge;
        [SerializeField] private GameObject _bottomEdge;
        [SerializeField] private GameObject _leftEdge;
        [SerializeField] private GameObject _rightEdge;


        public void Init()
        {
            _point.SetActive(false);
            _topEdge.SetActive(false);
            _bottomEdge.SetActive(false);
            _leftEdge.SetActive(false);
            _rightEdge.SetActive(false);
        }

        public void SetEdge(int colorId, Point direction)
        {
            GameObject connectedNode = _point;

            if (direction == Point.up)
            {
                connectedNode = _topEdge;
            }

            else if (direction == Point.down)
            {
                connectedNode = _bottomEdge;
            }

            else if (direction == Point.left)
            {
                connectedNode = _leftEdge;
            }

            else if (direction == Point.right)
            {
                connectedNode = _rightEdge;
            }

            connectedNode.SetActive(true);
            connectedNode.GetComponent<SpriteRenderer>().color = NodeColors[colorId % NodeColors.Count];
        }
    } 
}
