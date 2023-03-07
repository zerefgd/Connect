using Connect.Common;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Connect.Generator
{
    public class LevelGenerator : MonoBehaviour
    {
        #region START_METHODS

        [SerializeField] private bool canGeneratorOnce;

        [SerializeField] private int stage;

        public int levelSize => stage + 4;

        private void Awake()
        {
            SpawnBoard();
            SpawnNodes();
        }

        [SerializeField] private SpriteRenderer _boardPrefab, _bgCellPrefab;

        private void SpawnBoard()
        {
            var board = Instantiate(_boardPrefab,
                new Vector3(levelSize / 2f, levelSize / 2f, 0f),
                Quaternion.identity);

            board.size = new Vector2(levelSize + 0.08f, levelSize + 0.08f);

            for (int i = 0; i < levelSize; i++)
            {
                for (int j = 0; j < levelSize; j++)
                {
                    Instantiate(_bgCellPrefab, new Vector3(i + 0.5f, j + 0.5f, 0f), Quaternion.identity);
                }
            }

            Camera.main.orthographicSize = levelSize / 1.6f + 1f;
            Camera.main.transform.position = new Vector3(levelSize / 2f, levelSize / 2f, -10f);
        }

        [SerializeField] private NodeRenderer _nodePrefab;

        public Dictionary<Point, NodeRenderer> nodeGrid;
        private NodeRenderer[,] nodeArray;

        private void SpawnNodes()
        {
            nodeGrid = new Dictionary<Point, NodeRenderer>();
            nodeArray = new NodeRenderer[levelSize,levelSize];
            Vector3 spawnPos;
            NodeRenderer spawnedNode;

            for (int i = 0; i < levelSize; i++)
            {
                for (int j = 0; j < levelSize; j++)
                {
                    spawnPos = new Vector3(i + 0.5f, j + 0.5f, 0f);
                    spawnedNode = Instantiate(_nodePrefab, spawnPos, Quaternion.identity);
                    spawnedNode.Init();
                    nodeGrid.Add(new Point(i, j), spawnedNode);
                    nodeArray[i,j] = spawnedNode;
                    spawnedNode.gameObject.name = i.ToString() + j.ToString();
                }
            }
        }


        #endregion

        #region BUTTON_FUNCTION

        [SerializeField] private GameObject _simulateButton;

        public void ClickedSimulate()
        {
            Levels = new Dictionary<string, LevelData>();

            foreach (var item in _allLevelList.Levels)
            {
                Levels[item.LevelName] = item;
            }

            if (canGeneratorOnce)
            {
                GenerateDefault();
            }
            else
            {
                GenerateAll();
            }

            _simulateButton.SetActive(false);
        }

        [SerializeField] private LevelList _allLevelList;
        private Dictionary<string, LevelData> Levels;

        #region GENERATE_SINGLE_LEVEL
        private void GenerateDefault()
        {
            GenerateLevelData();
        }

        public LevelData currentLevelData;

        private void GenerateLevelData(int level = 0)
        {
            string currentLevelName = "Level" + stage.ToString() + level.ToString();

            if (!Levels.ContainsKey(currentLevelName))
            {
#if UNITY_EDITOR
                currentLevelData = ScriptableObject.CreateInstance<LevelData>();
                AssetDatabase.CreateAsset(currentLevelData, "Assets/Common/Prefabs/Levels/" +
                    currentLevelName + ".asset");
                AssetDatabase.SaveAssets();
#endif
                Levels[currentLevelName] = currentLevelData;
                _allLevelList.Levels.Add(currentLevelData);
            }

            currentLevelData = Levels[currentLevelName];
            currentLevelData.LevelName = currentLevelName;
            currentLevelData.Edges = new List<Edge>();

            GetComponent<GenerateMethod>().Generate();
        }

        #endregion

        #region GENERATE_ALL_LEVELS

        [SerializeField] private TMP_Text _counterText;
        public GridData result;

        private void GenerateAll()
        {
            StartCoroutine(GenerateAllLevels());
        }

        private IEnumerator GenerateAllLevels()
        {
            for (int i = 1; i < 51; i++)
            {
                yield return GenerateSingleLevelData(i);
                _counterText.text = i.ToString();
                yield return null;
            }
        }

        private IEnumerator GenerateSingleLevelData(int level = 0)
        {
            string currentLevelName = "Level" + stage.ToString() + level.ToString();

            if (!Levels.ContainsKey(currentLevelName))
            {
#if UNITY_EDITOR
                currentLevelData = ScriptableObject.CreateInstance<LevelData>();
                AssetDatabase.CreateAsset(currentLevelData, "Assets/Common/Prefabs/Levels/" +
                    currentLevelName + ".asset");
                AssetDatabase.SaveAssets();
#endif
                Levels[currentLevelName] = currentLevelData;
                _allLevelList.Levels.Add(currentLevelData);
            }

            currentLevelData = Levels[currentLevelName];
            currentLevelData.LevelName = currentLevelName;
            currentLevelData.Edges = new List<Edge>();

            yield return GetComponent<LevelGeneratorSingle>().Generate();
            currentLevelData.Edges = result.Edges;
            RenderGrid(result._grid);
        }

        #endregion

        #endregion

        #region NODE_RENDERING

        private List<Point> directions = new List<Point>()
        { Point.up,Point.down,Point.left,Point.right};

        public void RenderGrid(Dictionary<Point, int> grid)
        {
            int currentColor;
            int numOfConnectedNodes;

            foreach (var item in nodeGrid)
            {
                item.Value.Init();
                currentColor = grid[item.Key];
                numOfConnectedNodes = 0;

                if (currentColor != -1)
                {
                    foreach (var direction in directions)
                    {
                        if (grid.ContainsKey(item.Key + direction) &&
                            grid[item.Key + direction] == currentColor)
                        {
                            item.Value.SetEdge(currentColor, direction);
                            numOfConnectedNodes++;
                        }
                    }

                    if (numOfConnectedNodes <= 1)
                    {
                        item.Value.SetEdge(currentColor, Point.zero);
                    }
                }
            }
        }

        private Point[] neighbourPoints = new Point[]
        {
            Point.up,Point.left,Point.down, Point.right
        };

        public void RenderGrid(int[,] grid)
        {
            int currentColor;
            int numOfConnectedNodes;

            for (int i = 0; i < levelSize; i++)
            {
                for (int j = 0; j < levelSize; j++)
                {
                    nodeArray[i,j].Init();
                    currentColor = grid[i, j];
                    numOfConnectedNodes = 0;

                    if(currentColor != -1)
                    {
                        for (int p = 0; p < neighbourPoints.Length; p++)
                        {
                            Point tempPoint = new Point(i,j) + neighbourPoints[p];

                            if(tempPoint.IsPointValid(levelSize) &&
                                grid[tempPoint.x,tempPoint.y] == currentColor                                
                                )
                            {
                                nodeArray[i,j].SetEdge(currentColor, neighbourPoints[p]);
                                numOfConnectedNodes++;
                            }
                        }

                        if(numOfConnectedNodes <= 1)
                        {
                            nodeArray[i, j].SetEdge(currentColor, Point.zero);
                        }
                    }
                }
            }
        }

        #endregion

    }

    public interface GenerateMethod
    {
        public void Generate();
    }

    public struct Point
    {
        public int x;
        public int y;

        public Point(int x,int y)
        {
            this.x = x;
            this.y = y;
        }

        public bool IsPointValid(int maxCount)
        {
            return x < maxCount && y < maxCount && x > -1 && y > -1;    
        }

        public static Point operator +(Point p1, Point p2)
        {
            return new Point(p1.x + p2.x, p1.y + p2.y);
        }

        public static Point operator -(Point p1, Point p2)
        {
            return new Point(p1.x - p2.x, p1.y - p2.y);
        }

        public static Point up => new Point(0,1);
        public static Point left => new Point(-1,0);
        public static Point down => new Point(0,-1);
        public static Point right => new Point(1, 0);
        public static Point zero => new Point(0, 0);
        public static bool operator ==(Point p1, Point p2) => p1.x == p2.x && p1.y == p2.y;
        public static bool operator !=(Point p1, Point p2) => p1.x != p2.x || p1.y != p2.y;
        public override bool Equals(object obj)
        {
            Point a = (Point)obj;
            return x == a.x && y == a.y;
        }
        public override int GetHashCode()
        {
            return (100*x + y).GetHashCode();
        }

    }
}
