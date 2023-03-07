using Connect.Common;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Connect.Generator.BubbleRemove
{
    public class BubbleRemove : MonoBehaviour,GenerateMethod
    {
        [SerializeField] private TMP_Text _timerText, _gridCountText;
        [SerializeField] private bool _showOnlyResult;
        private GridList checkingGrid;
        private LevelGenerator Instance;
        private bool isCreating;
        private GridNode CurrentNode;

        [SerializeField] private float speedMultipler;
        [SerializeField] private float speed;
        private long checkingGridCount;

        private void Start()
        {
            Instance = GetComponent<LevelGenerator>();
            isCreating = true;
            checkingGridCount = 0;
        }

        public void Generate()
        {
            StartCoroutine(GeneratePaths());
        }

        private IEnumerator GeneratePaths()
        {
            CurrentNode = new GridNode(Instance.levelSize);
            CurrentNode = CurrentNode.Next();
            checkingGrid = new GridList(CurrentNode.Data);            

            StartCoroutine(SolvePaths());

            yield return null;

            int count = 0;

            GridList showList = checkingGrid;

            while (isCreating)
            {

                while (_showOnlyResult && !showList.Data.IsGridComplete())
                {
                    if (showList.Next == null)
                    {
                        yield break;
                    }
                    showList = showList.Next;
                }


                Instance.RenderGrid(showList.Data._grid);
                count++;

                _timerText.text = count.ToString();
                _gridCountText.text = checkingGridCount.ToString();

                showList = showList.Next;


                if (showList == null)
                {
                    yield break;
                }

                yield return new WaitForSeconds(speed);
            }
        }

        private IEnumerator SolvePaths()
        {
            int iterPerFrame = (int)(speedMultipler);
            int currentIter = 0;

            GridList solveList = checkingGrid;
            GridList tempList;
            GridNode nextNode;

            while (CurrentNode != null)
            {
                if(CurrentNode.Data.IsGridComplete())
                {
                    Instance.currentLevelData.Edges = CurrentNode.Data.Edges;
                    yield break;
                }

                nextNode = CurrentNode.Next();

                if (nextNode != null)
                {
                    CurrentNode = nextNode;
                    tempList = new GridList(nextNode.Data);
                }
                else
                {
                    CurrentNode = CurrentNode.Prev;
                    if (CurrentNode == null) yield break;
                    tempList = new GridList(CurrentNode.Data);
                }

                solveList.Next = tempList;
                solveList = solveList.Next;

                checkingGridCount++;
                currentIter++;

                if (currentIter > iterPerFrame * Time.deltaTime)
                {
                    currentIter = 0;
                    yield return null;
                }
            }
        }        

        public static bool IsSolvable(HashSet<Point> points)
        {
            if(points.Count < 3) return false;
            if(points.Count == 3) return true;
            if(points.Count > 9) return true;

            GridNode CurrentCheckNode = new GridNode(GridData.LevelSize, points);
            GridNode NextNode;

            while(CurrentCheckNode != null )
            {
                if(CurrentCheckNode.Data.IsGridComplete())
                {
                    return true;
                }
                NextNode = CurrentCheckNode.Next();
                if (NextNode != null)
                {
                    CurrentCheckNode = NextNode;
                }
                else
                {
                    CurrentCheckNode = CurrentCheckNode.Prev;
                }

            }

            return false;
        }
    }

    public class GridList
    {
        public GridList Next;
        public GridData Data;

        public GridList(GridData data)
        {
            Next = null;
            Data = data;
        }
    }

    public class GridNode
    {
        public GridNode Prev;
        public GridData Data;
        private int neighborIndex, emptyIndex;
        private List<Point> neighbors, emptyPositions;
        public GridNode(int LevelSize)
        {
            Prev = null;
            Data = new GridData(LevelSize);
            neighbors = new List<Point>();
            emptyPositions = new List<Point>();
            neighborIndex = 0;
            emptyIndex = 0;
            for (int i = 0; i < LevelSize; i++)
            {
                for (int j = 0; j < LevelSize; j++)
                {
                    emptyPositions.Add(new Point(i, j));
                }
            }
            Shuffle(emptyPositions);
        }

        public GridNode(GridData data, GridNode prev = null)
        {
            Data = data;
            Prev = prev;
            neighborIndex = 0;
            emptyIndex = 0;
            neighbors = new List<Point>();
            emptyPositions = new List<Point>();
            Data.GetResultsList(neighbors, emptyPositions);
            Shuffle(neighbors);
            Shuffle(emptyPositions);
        }

        public GridNode(int levelSize, HashSet<Point> points)
        {
            Prev = null;
            Data = new GridData(levelSize,points);
            neighborIndex = 0;
            emptyIndex = 0;
            neighbors = new List<Point>();
            emptyPositions = new List<Point>();

            foreach (var item in points)
            {
                emptyPositions.Add(item);
            }

            Shuffle(emptyPositions);
        }


        public GridNode Next()
        {
            GridData tempGrid;

            if (neighborIndex < neighbors.Count && emptyIndex < emptyPositions.Count)
            {
                if (UnityEngine.Random.Range(0, GridData.LevelSize) != 0)
                {
                    tempGrid = new GridData(neighbors[neighborIndex].x, neighbors[neighborIndex].y, Data.ColorId, Data);
                    neighborIndex++;
                    return new GridNode(tempGrid, this);
                }
                else
                {
                    tempGrid = new GridData(emptyPositions[emptyIndex].x, emptyPositions[emptyIndex].y, Data.ColorId + 1, Data);
                    emptyIndex++;
                    return new GridNode(tempGrid, this);
                }
            }
            else if (neighborIndex < neighbors.Count)
            {
                tempGrid = new GridData(neighbors[neighborIndex].x, neighbors[neighborIndex].y, Data.ColorId, Data);
                neighborIndex++;
                return new GridNode(tempGrid, this);
            }
            else if (emptyIndex < emptyPositions.Count)
            {
                tempGrid = new GridData(emptyPositions[emptyIndex].x, emptyPositions[emptyIndex].y, Data.ColorId + 1, Data);
                emptyIndex++;
                return new GridNode(tempGrid, this);
            }

            return null;
        }

        public static void Shuffle(List<Point> list)
        {
            System.Random rng = new System.Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }

    public class GridData
    {
        private static Point[] directionChecks = new Point[]
        { Point.up,Point.down,Point.left,Point.right };

        public int[,] _grid;
        public bool IsSolved;
        public Point CurrentPos;
        public int ColorId;
        public static int LevelSize;
        public List<Edge> Edges;

        public GridData(int levelSize)
        {
            _grid = new int[levelSize, levelSize];

            for (int i = 0; i < levelSize; i++)
            {
                for (int j = 0; j < levelSize; j++)
                {
                    _grid[i, j] = -1;
                }
            }

            IsSolved = false;
            ColorId = -1;
            LevelSize = levelSize;
            Edges = new List<Edge>();
        }

        public GridData(int i, int j, int passedColor, GridData gridCopy)
        {
            _grid = new int[LevelSize, LevelSize];

            for (int a = 0; a < LevelSize; a++)
            {
                for (int b = 0; b < LevelSize; b++)
                {
                    _grid[a, b] = gridCopy._grid[a, b];
                }
            }

            Edges = new List<Edge>();

            foreach (var item in gridCopy.Edges)
            {
                Edge temp = new Edge();
                temp.Points = new List<Vector2Int>();
                foreach (var point in item.Points)
                {
                    temp.Points.Add(point);
                }
                Edges.Add(temp);
            }

            ColorId = gridCopy.ColorId;
            if(passedColor == ColorId)
            {
                Edges[Edges.Count - 1].Points.Add(new Vector2Int(i, j));
            }
            else
            {
                Edges.Add(new Edge()
                {
                    Points = new List<Vector2Int>() { new Vector2Int(i, j) }
                });
            }

            CurrentPos = new Point(i, j);
            ColorId = passedColor;
            _grid[CurrentPos.x, CurrentPos.y] = ColorId;
            IsSolved = false;
        }

        public GridData(int levelSize,HashSet<Point> points)
        {
            _grid = new int[levelSize, levelSize];

            for (int i = 0; i < levelSize; i++)
            {
                for (int j = 0; j < levelSize; j++)
                {
                    _grid[i, j] = -2;
                }
            }

            foreach (var point in points)
            {
                _grid[point.x, point.y] = -1;
            }

            IsSolved = false;
            ColorId = -1;
            LevelSize = levelSize;
            Edges = new List<Edge>();
        }

        public bool IsInsideGrid(Point pos)
        {
            return pos.IsPointValid(LevelSize);
        }

        public bool IsGridComplete()
        {
            foreach (var item in _grid)
            {
                if (item == -1) return false;
            }

            for (int i = 0; i <= ColorId; i++)
            {
                int result = 0;

                foreach (var item in _grid)
                {
                    if (item == i)
                        result++;
                }

                if (result < 3)
                    return false;

            }

            return true;
        }

        public bool IsNotNeighbour(Point pos)
        {

            for (int i = 0; i < LevelSize; i++)
            {
                for (int j = 0; j < LevelSize; j++)
                {
                    if (_grid[i, j] == ColorId && new Point(i, j) != CurrentPos)
                    {
                        for (int p = 0; p < directionChecks.Length; p++)
                        {
                            if (pos - new Point(i, j) == directionChecks[p])
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        public int FlowLength()
        {
            int result = 0;
            foreach (var item in _grid)
            {
                if (item == ColorId)
                    result++;
            }

            return result;
        }

        public void GetResultsList(List<Point> neighbors, List<Point> emptyPositions)
        {
            int[,] emptyGrid = new int[LevelSize, LevelSize];
            for (int i = 0; i < LevelSize; i++)
            {
                for (int j = 0; j < LevelSize; j++)
                {
                    emptyGrid[i, j] = -1;
                }
            }

            for (int i = 0; i < LevelSize; i++)
            {
                for (int j = 0; j < LevelSize; j++)
                {
                    if (_grid[i, j] == -1)
                    {
                        emptyGrid[i, j] = 0;
                        for (int k = 0; k < directionChecks.Length; k++)
                        {
                            Point tempPoint = new Point(directionChecks[k].x + i, directionChecks[k].y + j);
                            if (IsInsideGrid(tempPoint) && _grid[tempPoint.x, tempPoint.y] == -1)
                            {
                                emptyGrid[i, j]++;
                            }
                        }
                    }
                }
            }

            List<Point> zeroNeighbours = new List<Point>();
            List<Point> allNeighbours = new List<Point>();

            for (int i = 0; i < directionChecks.Length; i++)
            {
                Point tempPoint = CurrentPos + directionChecks[i];
                if (IsInsideGrid(tempPoint) &&
                    IsNotNeighbour(tempPoint) &&
                    emptyGrid[tempPoint.x, tempPoint.y] != -1)
                {
                    if (emptyGrid[tempPoint.x, tempPoint.y] == 0)
                    {
                        zeroNeighbours.Add(tempPoint);
                        emptyGrid[tempPoint.x, tempPoint.y] = -1;
                    }
                    allNeighbours.Add(tempPoint);
                }
            }

            List<Point> zeroEmpty = new List<Point>();
            List<Point> oneEmpty = new List<Point>();
            List<Point> allEmpty = new List<Point>();

            for (int i = 0; i < LevelSize; i++)
            {
                for (int j = 0; j < LevelSize; j++)
                {
                    if (emptyGrid[i, j] == 0)
                    {
                        zeroEmpty.Add(new Point(i, j));
                    }

                    if (emptyGrid[i, j] == 1)
                    {
                        oneEmpty.Add(new Point(i, j));
                    }

                    if (emptyGrid[i, j] != -1)
                    {
                        allEmpty.Add(new Point(i, j));
                    }
                }
            }

            List<HashSet<Point>> connectedSet = new List<HashSet<Point>>();
            HashSet<Point> minSet = FindMinConnectedSet(new List<Point>(allEmpty),connectedSet);
            List<HashSet<Point>> tempSet = new List<HashSet<Point>>();

            foreach (var item in connectedSet)
            {
                bool canAdd = true;

                foreach (var neighbor in allNeighbours)
                {
                    if(item.Contains(neighbor))
                        canAdd = false;
                }
                if(canAdd)
                {
                    tempSet.Add(item);
                }
            }
            connectedSet = tempSet;

            if (zeroEmpty.Count > 0 || zeroNeighbours.Count > 1)
            {
                return;
            }

            foreach (var item in connectedSet)
            {
                if(!BubbleRemove.IsSolvable(item))
                {
                    return;
                }
            }

            if (zeroNeighbours.Count == 1)
            {
                neighbors.Add(zeroNeighbours[0]);
                return;
            }

            foreach (var item in allNeighbours)
            {
                neighbors.Add(item);
            }

            if (FlowLength() < 3) return;

            if (oneEmpty.Count > 0)
            {
                foreach (var item in oneEmpty)
                {
                    if (minSet.Contains(item))
                        emptyPositions.Add(item);
                }

                return;
            }

            foreach (var item in allEmpty)
            {
                if (minSet.Contains(item))
                    emptyPositions.Add(item);
            }

        }

        public static HashSet<Point> FindMinConnectedSet(List<Point> points, List<HashSet<Point>> connectedSet)
        {
            HashSet<Point> visited = new HashSet<Point>();
            HashSet<Point> allPoints = new HashSet<Point>(points);

            foreach (var point in points)
            {
                if (!visited.Contains(point))
                {
                    HashSet<Point> connected = new HashSet<Point>();
                    Queue<Point> queue = new Queue<Point>();

                    queue.Enqueue(point);

                    while (queue.Count > 0)
                    {
                        Point current = queue.Dequeue();

                        if (!visited.Contains(current))
                        {
                            connected.Add(current);
                            visited.Add(current);

                            foreach (var neighbor in GetNeighbors(current))
                            {
                                if (!visited.Contains(neighbor) && allPoints.Contains(neighbor))
                                {
                                    queue.Enqueue(neighbor);
                                }
                            }
                        }
                    }

                    connectedSet.Add(connected);
                }
            }

            HashSet<Point> minSet = null;

            foreach (var item in connectedSet)
            {
                if (minSet == null || item.Count < minSet.Count)
                {
                    minSet = item;
                }
            }

            return minSet;
        }

        private static List<Point> GetNeighbors(Point point)
        {
            List<Point> result = new List<Point>
            {
                new Point(point.x, point.y + 1),
                new Point(point.x, point.y - 1),
                new Point(point.x + 1, point.y),
                new Point(point.x - 1, point.y)
            };

            return result;
        }
    }
}
