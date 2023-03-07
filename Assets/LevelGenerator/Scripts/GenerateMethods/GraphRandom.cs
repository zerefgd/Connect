using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Connect.Generator.GraphRandom
{
	public class GraphRandom : MonoBehaviour,GenerateMethod
	{
        [SerializeField] private TMP_Text _timerText, _gridCountText;
        [SerializeField] private bool _showOnlyResult;
        private GridList checkingGrid;
        private LevelGenerator Instance;
        private bool isCreating;
        private GridNode CurrentNode;


        private HashSet<GridList> GridSet;

        [SerializeField] private float speedMultipler;
        [SerializeField] private float speed;
        private long checkingGridCount;

        private void Start()
        {
            Instance = GetComponent<LevelGenerator>();
            isCreating = true;
            GridSetComparer setComparer = new GridSetComparer();
            GridSet = new HashSet<GridList>(setComparer);
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
            AddToGridSet(checkingGrid);                        

            StartCoroutine(SolvePaths());

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

                nextNode = CurrentNode.Next();

                if(nextNode != null)
                {
                    tempList = new GridList(nextNode.Data);
                    if(!GridSet.Contains(tempList))
                    {
                        AddToGridSet(tempList);
                        CurrentNode = nextNode;
                    }
                    else
                    {
                        CurrentNode = CurrentNode.Prev;
                        if (CurrentNode == null) yield break;
                        tempList = new GridList(CurrentNode.Data);
                    }
                }
                else
                {
                    CurrentNode = CurrentNode.Prev;
                    if (CurrentNode == null) yield break;
                    tempList = new GridList(CurrentNode.Data);
                }

                solveList.Next = tempList;
                solveList = solveList.Next;

                currentIter++;

                if (currentIter > iterPerFrame * Time.deltaTime)
                {
                    currentIter = 0;
                    yield return null;
                }
            }
        }

        private void AddToGridSet(GridList addList)
        {
            GridList tempList;
            foreach (var item in addList.Data.GetSimilar())
            {
                tempList = new GridList(item);
                if (!GridSet.Contains(tempList))
                {
                    GridSet.Add(tempList);
                }
            }

            checkingGridCount++;
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
        }

        public GridNode(GridData data,GridNode prev = null)
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

        public GridNode Next()
        {
            GridData tempGrid;

            if(neighborIndex < neighbors.Count && emptyIndex < emptyPositions.Count)
            {
                if(UnityEngine.Random.Range(0,GridData.LevelSize) != 0)
                {
                    tempGrid = new GridData(neighbors[neighborIndex].x, neighbors[neighborIndex].y,Data.ColorId,Data);
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
            else if(neighborIndex < neighbors.Count)
            {
                tempGrid = new GridData(neighbors[neighborIndex].x, neighbors[neighborIndex].y, Data.ColorId, Data);
                neighborIndex++;
                return new GridNode(tempGrid, this);
            }
            else if(emptyIndex < emptyPositions.Count)
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
            while(n > 1)
            {
                n--;
                int k = rng.Next(n+1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }

    public class GridSetComparer : IEqualityComparer<GridList>
    {
        private static Point[] directionChecks = new Point[]
        { Point.up,Point.left,Point.down,Point.right };

        public bool Equals(GridList x, GridList y)
        {
            int[,] firstGrid, secondGrid;
            firstGrid = x.Data._grid;
            secondGrid = y.Data._grid;

            int[] colorSwap = new int[x.Data.ColorId + 1];

            for (int i = 0; i < colorSwap.Length; i++)
            {
                colorSwap[i] = -1;
            }

            Point pos;
            bool isEmpty = false;

            for (int i = 0; i < GridData.LevelSize; i++)
            {
                for (int j = 0; j < GridData.LevelSize; j++)
                {
                    pos.x = i;
                    pos.y = j;

                    if (!isEmpty && firstGrid[pos.x, pos.y] == -1 || secondGrid[pos.x, pos.y] == -1)
                    {
                        isEmpty = true;
                    }

                    if ((firstGrid[pos.x, pos.y] == -1 && secondGrid[pos.x, pos.y] != -1) ||
                        (firstGrid[pos.x, pos.y] != -1 && secondGrid[pos.x, pos.y] == -1))
                    {
                        return false;
                    }

                    bool canCheck = firstGrid[pos.x, pos.y] != -1;

                    if (canCheck && colorSwap[firstGrid[pos.x, pos.y]] == -1)
                    {
                        colorSwap[firstGrid[pos.x, pos.y]] = secondGrid[pos.x, pos.y];
                    }
                    else if (canCheck)
                    {
                        if (colorSwap[firstGrid[pos.x, pos.y]] != secondGrid[pos.x, pos.y])
                        {
                            return false;
                        }
                    }
                }
            }

            if (isEmpty && x.Data.CurrentPos != y.Data.CurrentPos)
            {
                return false;
            }

            return true;
        }

        public int GetHashCode(GridList obj)
        {
            bool[,,] graph = new bool[GridData.LevelSize, GridData.LevelSize, 4];

            Point startPos, checkPos;

            startPos = Point.zero;

            for (int i = 0; i < GridData.LevelSize; i++)
            {
                for (int j = 0; j < GridData.LevelSize; j++)
                {
                    startPos.x = i;
                    startPos.y = j;

                    if (obj.Data._grid[startPos.x, startPos.y] != -1)
                    {
                        for (int d = 0; d < directionChecks.Length; d++)
                        {
                            checkPos = directionChecks[d] + startPos;
                            graph[i, j, d] = obj.Data.IsInsideGrid(checkPos) &&
                               obj.Data._grid[checkPos.x, checkPos.y] ==
                               obj.Data._grid[startPos.x, startPos.y];
                        }
                    }
                    else
                    {
                        for (int d = 0; d < 4; d++)
                        {
                            graph[i, j, d] = false;
                        }
                    }
                }
            }
            return GetHashCodeBool3D(graph);
        }

        public int GetHashCodeBool3D(bool[,,] arr)
        {
            int length = arr.GetLength(0) * arr.GetLength(1) * arr.GetLength(2);
            byte[] byteArray = new byte[(length + 7) / 8];

            int index = 0;
            int bitIndex = 0;
            byte currentByte = 0;

            for (int i = 0; i < arr.GetLength(0); i++)
            {
                for (int j = 0; j < arr.GetLength(1); j++)
                {
                    for (int k = 0; k < arr.GetLength(2); k++)
                    {
                        if (arr[i, j, k])
                        {
                            currentByte |= (byte)(1 << bitIndex);
                        }

                        bitIndex++;

                        if (bitIndex == 8)
                        {
                            byteArray[index] = currentByte;
                            currentByte = 0;
                            bitIndex = 0;
                            index++;
                        }
                    }
                }
            }

            if (bitIndex > 0)
            {
                byteArray[index] = currentByte;
            }

            return BitConverter.ToString(byteArray).GetHashCode();
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

        public GridData(int levelSize)
        {
            _grid = new int[levelSize,levelSize];

            for (int i = 0; i < levelSize; i++)
            {
                for (int j = 0; j < levelSize; j++)
                {
                    _grid[i,j] = -1;
                }
            }

            IsSolved = false;
            ColorId = -1;
            LevelSize = levelSize;
        }

        public GridData(int i, int j, int levelSize)
        {
            _grid = new int[levelSize, levelSize];

            for (int a = 0; a < levelSize; a++)
            {
                for (int b = 0; b < levelSize; b++)
                {
                    _grid[a, b] = -1;
                }
            }
            IsSolved = false;
            CurrentPos = new Point(i, j);
            ColorId = 0;
            _grid[CurrentPos.x, CurrentPos.y] = ColorId;
            LevelSize = levelSize;
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

            CurrentPos = new Point(i, j);
            ColorId = passedColor;
            _grid[CurrentPos.x, CurrentPos.y] = ColorId;
            IsSolved = false;
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

        public List<GridData> GetSimilar()
        {
            List<GridData> result = new List<GridData>();

            GridData addData;

            for (int i = 0; i < 4; i++)
            {
                addData = new GridData(CurrentPos.x, CurrentPos.y, ColorId, this);
                addData.Rotate(i);
                result.Add(addData);
                addData = new GridData(addData.CurrentPos.x, addData.CurrentPos.y, addData.ColorId, addData);
                addData.Flip();
                result.Add(addData);
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

            if (zeroEmpty.Count > 0 || zeroNeighbours.Count > 1)
            {
                return;
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

            HashSet<Point> minSet = FindMinConnectedSet(new List<Point>(allEmpty));

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

        public void Rotate(int rot)
        {
            for (int i = 0; i < rot; i++)
            {
                Rotate();
            }
        }

        private void Rotate()
        {
            for (int i = 0; i < LevelSize; i++)
            {
                int start = 0;
                int end = LevelSize - 1;
                while (start < end)
                {
                    int temp = _grid[i, start];
                    _grid[i, start] = _grid[i, end];
                    _grid[i, end] = temp;
                    start++;
                    end--;
                }
            }

            for (int i = 0; i < LevelSize; i++)
            {
                for (int j = i; j < LevelSize; j++)
                {
                    int temp = _grid[i, j];
                    _grid[i, j] = _grid[j, i];
                    _grid[j, i] = temp;
                }
            }

            CurrentPos = Rotate(CurrentPos);

        }

        private Point Rotate(Point pos)
        {
            Point result = pos;
            result.x = LevelSize - 1 - pos.y;
            result.y = pos.x;
            return result;
        }


        public void Flip()
        {

            int tempColor;
            Point firstPos, secondPos;

            for (int i = 0; i < LevelSize / 2; i++)
            {
                for (int j = 0; j < LevelSize; j++)
                {
                    firstPos = new Point(i, j);
                    tempColor = _grid[firstPos.x, firstPos.y];
                    secondPos = Flip(firstPos);
                    _grid[firstPos.x, firstPos.y] = _grid[secondPos.x, secondPos.y];
                    _grid[secondPos.x, secondPos.y] = tempColor;
                }
            }

            CurrentPos = Flip(CurrentPos);
        }

        private Point Flip(Point pos)
        {
            pos.x = LevelSize - 1 - pos.x;
            return pos;
        }

        public static HashSet<Point> FindMinConnectedSet(List<Point> points)
        {
            HashSet<Point> visited = new HashSet<Point>();
            HashSet<Point> allPoints = new HashSet<Point>(points);
            List<HashSet<Point>> connectedSet = new List<HashSet<Point>>();

            foreach (var point in points)
            {
                if(!visited.Contains(point))
                {
                    HashSet<Point> connected = new HashSet<Point>();
                    Queue<Point> queue = new Queue<Point>();

                    queue.Enqueue(point);

                    while(queue.Count > 0)
                    {
                        Point current = queue.Dequeue();

                        if(!visited.Contains(current))
                        {
                            connected.Add(current);
                            visited.Add(current);

                            foreach (var neighbor in GetNeighbors(current))
                            {
                                if(!visited.Contains(neighbor) && allPoints.Contains(neighbor))
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
                if(minSet == null || item.Count < minSet.Count)
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
