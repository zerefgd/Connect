using System.Collections.Generic;
using UnityEngine;

namespace Connect.Core
{
    public class Node : MonoBehaviour
    {

        [SerializeField] private GameObject _point;
        [SerializeField] private GameObject _topEdge;
        [SerializeField] private GameObject _bottomEdge;
        [SerializeField] private GameObject _leftEdge;
        [SerializeField] private GameObject _rightEdge;
        [SerializeField] private GameObject _highLight;

        private Dictionary<Node, GameObject> ConnectedEdges;

        [HideInInspector] public int colorId;

        public bool IsWin
        {
            get
            {
                if(_point.activeSelf)
                {
                    return ConnectedNodes.Count == 1;
                }

                return ConnectedNodes.Count == 2;
            }
        }

        public bool IsClickable
        {
            get
            {
                if(_point.activeSelf)
                {
                    return true;
                }

                return ConnectedNodes.Count > 0;
            }
        }

        public bool IsEndNode => _point.activeSelf;

        public Vector2Int Pos2D 
        { get; set; }

        public void Init()
        {
            _point.SetActive(false);
            _topEdge.SetActive(false);
            _bottomEdge.SetActive(false);
            _leftEdge.SetActive(false);
            _rightEdge.SetActive(false);
            _highLight.SetActive(false);
            ConnectedEdges = new Dictionary<Node, GameObject>();
            ConnectedNodes = new List<Node>();
        }

        public void SetColorForPoint(int colorIdForSpawnedNode)
        {
            colorId = colorIdForSpawnedNode;
            _point.SetActive(true);
            _point.GetComponent<SpriteRenderer>().color =
                GameplayManager.Instance.NodeColors[colorId % GameplayManager.Instance.NodeColors.Count];
        }

        public void SetEdge(Vector2Int offset, Node node)
        {
            if(offset == Vector2Int.up)
            {
                ConnectedEdges[node] = _topEdge;
                return;
            }

            if(offset == Vector2Int.down) 
            {
                ConnectedEdges[node] = _bottomEdge;
                return;
            }

            if(offset == Vector2Int.right) 
            {
                ConnectedEdges[node] = _rightEdge;
                return;
            }

            if (offset == Vector2Int.left)
            {
                ConnectedEdges[node] = _leftEdge;
                return;
            }
        }        

        [HideInInspector ]public List<Node> ConnectedNodes;

        public void UpdateInput(Node connectedNode)
        {
            //Invalid Input
            if(!ConnectedEdges.ContainsKey(connectedNode))
            {
                return;
            }

            //Connected Node already exist
            //Delete the Edge and the parts
            if(ConnectedNodes.Contains(connectedNode))
            {
                ConnectedNodes.Remove(connectedNode);
                connectedNode.ConnectedNodes.Remove(this);
                RemoveEdge(connectedNode);
                DeleteNode();
                connectedNode.DeleteNode();
                return;
            }

            //Start Node has 2 Edges
            if(ConnectedNodes.Count == 2)
            {
                Node tempNode = ConnectedNodes[0];

                if(!tempNode.IsConnectedToEndNode())
                {
                    ConnectedNodes.Remove(tempNode);
                    tempNode.ConnectedNodes.Remove(this);
                    RemoveEdge(tempNode);
                    tempNode.DeleteNode();
                }
                else
                {
                    tempNode = ConnectedNodes[1];
                    ConnectedNodes.Remove(tempNode);
                    tempNode.ConnectedNodes.Remove(this);
                    RemoveEdge(tempNode);
                    tempNode.DeleteNode();
                }
            }
            
            //End Node has 2 Edges
            if (connectedNode.ConnectedNodes.Count == 2)
            {
                Node tempNode = connectedNode.ConnectedNodes[0];
                connectedNode.ConnectedNodes.Remove(tempNode);
                tempNode.ConnectedNodes.Remove(connectedNode);
                connectedNode.RemoveEdge(tempNode);
                tempNode.DeleteNode();

                tempNode = connectedNode.ConnectedNodes[0];
                connectedNode.ConnectedNodes.Remove(tempNode);
                tempNode.ConnectedNodes.Remove(connectedNode);
                connectedNode.RemoveEdge(tempNode);
                tempNode.DeleteNode();
            }

            //Start Node is Different Color and connected Node Has 1 Edge
            if(connectedNode.ConnectedNodes.Count == 1 && connectedNode.colorId != colorId) 
            {
                Node tempNode = connectedNode.ConnectedNodes[0];
                connectedNode.ConnectedNodes.Remove(tempNode);
                tempNode.ConnectedNodes.Remove(connectedNode);
                connectedNode.RemoveEdge(tempNode);
                tempNode.DeleteNode();
            }

            //Starting is Edge Node and has 1 Edge already
            if(ConnectedNodes.Count == 1 && IsEndNode)
            {
                Node tempNode = ConnectedNodes[0];
                ConnectedNodes.Remove(tempNode);
                tempNode.ConnectedNodes.Remove(this);
                RemoveEdge(tempNode);
                tempNode.DeleteNode();
            }

            //ConnectedNode is EdgeNode and has 1 Edge already
            if(connectedNode.ConnectedNodes.Count == 1 && connectedNode.IsEndNode)
            {
                Node tempNode = connectedNode.ConnectedNodes[0];
                connectedNode.ConnectedNodes.Remove(tempNode);
                tempNode.ConnectedNodes.Remove(connectedNode);
                connectedNode.RemoveEdge(tempNode);
                tempNode.DeleteNode();
            }

            AddEdge(connectedNode);

            //Dont allow Boxes
            if(colorId != connectedNode.colorId)
            {
                return;
            }

            List<Node> checkingNodes = new List<Node>() { this };
            List<Node> resultNodes = new List<Node>() { this };

            while(checkingNodes.Count > 0)
            {
                foreach (var item in checkingNodes[0].ConnectedNodes)
                {
                    if(!resultNodes.Contains(item))
                    {
                        resultNodes.Add(item);
                        checkingNodes.Add(item);
                    }
                }

                checkingNodes.Remove(checkingNodes[0]);
            }

            foreach (var item in resultNodes)
            {
                if(!item.IsEndNode && item.IsDegreeThree(resultNodes))
                {
                    Node tempNode = item.ConnectedNodes[0];
                    item.ConnectedNodes.Remove(tempNode);
                    tempNode.ConnectedNodes.Remove(item);
                    item.RemoveEdge(tempNode);
                    tempNode.DeleteNode();

                    if (item.ConnectedNodes.Count == 0) return;

                    tempNode = item.ConnectedNodes[0];
                    item.ConnectedNodes.Remove(tempNode);
                    tempNode.ConnectedNodes.Remove(item);
                    item.RemoveEdge(tempNode);
                    tempNode.DeleteNode();

                    return;
                }
            }

        }

        private void AddEdge(Node connectedNode)
        {
            connectedNode.colorId = colorId;
            connectedNode.ConnectedNodes.Add(this);
            ConnectedNodes.Add(connectedNode);
            GameObject connectedEdge = ConnectedEdges[connectedNode];
            connectedEdge.SetActive(true);
            connectedEdge.GetComponent<SpriteRenderer>().color =
                GameplayManager.Instance.NodeColors[colorId % GameplayManager.Instance.NodeColors.Count];
        }

        private void RemoveEdge(Node node)
        {
            GameObject edge = ConnectedEdges[node];
            edge.SetActive(false);
            edge = node.ConnectedEdges[this];
            edge.SetActive(false);
        }

        private void DeleteNode()
        {
            Node startNode = this;

            if(startNode.IsConnectedToEndNode())
            {
                return;
            }

            while(startNode != null)
            {
                Node tempNode = null;
                if(startNode.ConnectedNodes.Count != 0)
                {
                    tempNode = startNode.ConnectedNodes[0];
                    startNode.ConnectedNodes.Clear();
                    tempNode.ConnectedNodes.Remove(startNode);
                    startNode.RemoveEdge(tempNode);
                }
                startNode = tempNode;
            }
        }

        public bool IsConnectedToEndNode(List<Node> checkedNode = null)
        {
            if(checkedNode == null)
            {
                checkedNode = new List<Node>();
            }

            if(IsEndNode)
            {
                return true;
            }

            foreach (var item in ConnectedNodes)
            {
                if(!checkedNode.Contains(item))
                {
                    checkedNode.Add(item);
                    return item.IsConnectedToEndNode(checkedNode);
                }
            }

            return false;
        }

        public void SolveHighlight()
        {
            if (ConnectedNodes.Count == 0)
            {
                _highLight.SetActive(false);
                return;
            }

            List<Node> checkingNodes =  new List<Node>() { this };
            List<Node> resultNodes =new List<Node>() { this };

            while(checkingNodes.Count > 0)
            {
                foreach (var item in checkingNodes[0].ConnectedNodes)
                {
                    if(!resultNodes.Contains(item))
                    {
                        resultNodes.Add(item);
                        checkingNodes.Add(item);
                    }
                }

                checkingNodes.Remove(checkingNodes[0]);
            }

            checkingNodes.Clear();

            foreach (var item in resultNodes)
            {
                if(item.IsEndNode)
                {
                    checkingNodes.Add(item);
                }
            }

            if(checkingNodes.Count == 2)
            {
                _highLight.SetActive(true);
                _highLight.GetComponent<SpriteRenderer>().color = 
                    GameplayManager.Instance.GetHighLightColor(colorId);
            }
            else
            {
                _highLight.SetActive(false);
            }

        }

        private List<Vector2Int> directionCheck = new List<Vector2Int>()
        {
            Vector2Int.up,Vector2Int.left,Vector2Int.down,Vector2Int.right
        };

        public bool IsDegreeThree(List<Node> resultNodes)
        {
            bool isdegreethree = false;

            int numOfNeighbours = 0;

            for (int i = 0; i < directionCheck.Count; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Vector2Int checkingPos = Pos2D + directionCheck[(i + j) % directionCheck.Count];

                    if(GameplayManager.Instance._nodeGrid.TryGetValue(checkingPos,out Node result))
                    {
                        if(resultNodes.Contains(result))
                        {
                            numOfNeighbours++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                if(numOfNeighbours == 3)
                {
                    break;
                }

                numOfNeighbours = 0;
            }

            if(numOfNeighbours >= 3)
            {
                isdegreethree = true;
            }

            return isdegreethree;
        }
    } 
}
