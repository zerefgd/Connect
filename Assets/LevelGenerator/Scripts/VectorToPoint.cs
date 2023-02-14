using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace Connect.Generator.VectorToPoint
{
	public class VectorToPoint : MonoBehaviour,GenerateMethod
	{
        [SerializeField] private TMP_Text _timerText, _gridCountText;
        [SerializeField] private bool _showOnlyResult;
        private GridList checkingGrid;
        private LevelGenerator Instance;
        private bool isCreating;

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
            GridData tempGrid = new GridData(0, 0, Instance.levelSize);
            GridList tempList = new GridList(tempGrid);
            checkingGrid = tempList;
            AddToGridSet(tempList);


            GridList addList = checkingGrid;

            for (int i = 0; i < Instance.levelSize; i++)
            {
                for (int j = 0; j < Instance.levelSize; j++)
                {
                    tempGrid = new GridData(i, j, Instance.levelSize);
                    tempList = new GridList(tempGrid);

                    if (!GridSet.Contains(tempList))
                    {
                        addList.Next = tempList;
                        addList = addList.Next;
                        AddToGridSet(tempList);
                    }
                }
            }

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

        private List<Point> directionChecks = new List<Point>()
        { Point.up,Point.down,Point.left,Point.right };

        private IEnumerator SolvePaths()
        {
            bool canSolve = true;
            int iterPerFrame = (int)(speedMultipler);
            int currentIter = 0;

            GridList solveList = checkingGrid;
            GridList resultGridList;
            GridData item;

            GridData tempGrid;
            GridList tempList, connectList;
            Point checkingDirection;

            while (canSolve)
            {
                resultGridList = solveList;

                if (solveList == null)
                {
                    canSolve = false;
                    yield break;
                }

                item = resultGridList.Data;

                foreach (var direction in directionChecks)
                {
                    checkingDirection = item.CurrentPos + direction;

                    if (item.IsInsideGrid(checkingDirection)
                        && item._grid[checkingDirection.x,checkingDirection.y] == -1
                        && item.IsNotNeighbour(checkingDirection)
                        )
                    {
                        tempGrid = new GridData(checkingDirection.x, checkingDirection.y, item.ColorId, item);
                        tempList = new GridList(tempGrid);

                        if (!GridSet.Contains(tempList))
                        {
                            connectList = resultGridList.Next;
                            resultGridList.Next = tempList;
                            tempList.Next = connectList;
                            resultGridList = resultGridList.Next;
                            AddToGridSet(tempList);
                        }
                    }
                }

                foreach (var emptyPos in item.EmptyPosition())
                {
                    if (item.FlowLength() > 2)
                    {
                        tempGrid = new GridData(emptyPos.x, emptyPos.y, item.ColorId + 1, item);
                        tempList = new GridList(tempGrid);

                        if (!GridSet.Contains(tempList))
                        {
                            connectList = resultGridList.Next;
                            resultGridList.Next = tempList;
                            tempList.Next = connectList;
                            resultGridList = resultGridList.Next;
                            AddToGridSet(tempList);
                        }
                    }
                }

                item.IsSolved = true;

                currentIter++;

                if (currentIter > iterPerFrame * Time.deltaTime)
                {
                    currentIter = 0;
                    yield return null;
                }

                solveList = solveList.Next;
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

    public class GridSetComparer : IEqualityComparer<GridList>
    {
        private static Point[] directionChecks = new Point[]
        { Point.up,Point.left,Point.down,Point.right };

        public bool Equals(GridList x, GridList y)
        {
            int[,] firstGrid, secondGrid;
            firstGrid = x.Data._grid;
            secondGrid = y.Data._grid;

            int[] colorSwap = new int[GridData.LevelSize];

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

                    if (firstGrid[pos.x,pos.y] != -1 && colorSwap[firstGrid[pos.x,pos.y]] != -1)
                    {
                        colorSwap[firstGrid[pos.x, pos.y]] = secondGrid[pos.x, pos.y];
                    }
                    else if (firstGrid[pos.x,pos.y] != -1)
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
            bool[,,] graph = new bool[GridData.LevelSize,GridData.LevelSize,4];

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
                            graph[i,j,d] = obj.Data.IsInsideGrid(checkPos) &&
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

            bool[] resultBinary = Convert3DTo1D(graph);
            long[] resultLong = Convert1DToLong(resultBinary);
            return ConvertLongArrayToString(resultLong).GetHashCode();
        }

        public static bool[] Convert3DTo1D(bool[,,] array)
        {
            int l1 = array.GetLength(0);
            int l2 = array.GetLength(1);
            int l3 = array.GetLength(2);
            int l = l1 * l2 *l3;
            bool[] result = new bool[l];

            int index = 0;
            for (int i = 0; i < l1; i++)
            {
                for (int j = 0; j < l2; j++)
                {
                    for (int k = 0; k < l3; k++)
                    {
                        result[index++] = array[i,j,k];
                    }
                }
            }
            return result;
        }

        public static long[] Convert1DToLong(bool[] array)
        {
            int length = array.Length;
            int longCount = (length + 63)/ 64;
            long[] result = new long[longCount];
            for (int i = 0; i < length; i++)
            {
                if (array[i])
                {
                    result[i/64] |=  1L << (i % 64);
                }
            }
            return result;
        }

        public static string ConvertLongArrayToString(long[] array)
        {
            StringBuilder sb= new StringBuilder();
            for (int i = 0; i < array.Length; i++)
            {
                sb.Append(
                    string.Format(new FixedLengthFormatProvider(20),"{0:D}",
                    array[i].ToString())
                    );
            }
            return sb.ToString();
        }

    }

    public class FixedLengthFormatProvider : IFormatProvider, ICustomFormatter
    {
        private readonly int _length;

        public FixedLengthFormatProvider(int length)
        {
            _length = length;
        }

        public object GetFormat(Type formatType)
        {
            if (formatType == typeof(ICustomFormatter))
            {
                return this;
            }

            return null;
        }

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg is long && format == "D")
            {
                return ((long)arg).ToString().PadLeft(_length, '0');
            }

            return arg.ToString();
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

        public GridData(int i, int j, int levelSize)
        {
            _grid = new int[levelSize,levelSize];

            for (int a = 0; a < levelSize; a++)
            {
                for (int b = 0; b < levelSize; b++)
                {
                    _grid[a,b] = -1;
                }
            }
            IsSolved = false;
            CurrentPos = new Point(i, j);
            ColorId = 0;
            _grid[CurrentPos.x,CurrentPos.y] = ColorId;
            LevelSize = levelSize;
        }

        public GridData(int i, int j, int passedColor, GridData gridCopy)
        {
            _grid = new int[LevelSize,LevelSize];

            for (int a = 0; a < LevelSize; a++)
            {
                for (int b = 0; b < LevelSize; b++)
                {
                    _grid[a,b] = gridCopy._grid[a,b];
                }
            }

            CurrentPos = new Point(i, j);
            ColorId = passedColor;
            _grid[CurrentPos.x,CurrentPos.y] = ColorId;
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
                    if (_grid[i,j] == ColorId && new Point(i,j) != CurrentPos)
                    {
                        for (int p = 0; p < directionChecks.Length; p++)
                        {
                            if(pos - new Point(i,j) == directionChecks[p])
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        public List<Point> EmptyPosition()
        {
            List<Point> result = new List<Point>();

            for (int a = 0; a < LevelSize; a++)
            {
                for (int b = 0; b < LevelSize; b++)
                {
                    if (_grid[a,b] == -1)
                        result.Add(new Point(a,b));
                }
            }

            return result;
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
                addData = new GridData(addData.CurrentPos.x,addData.CurrentPos.y,addData.ColorId,addData);
                addData.Flip();
                result.Add(addData);
            }

            return result;
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
                while(start < end)
                {
                    int temp = _grid[i, start];
                    _grid[i,start] = _grid[i,end];
                    _grid[i,end] = temp;
                    start++;
                    end--;
                }
            }

            for (int i = 0; i < LevelSize; i++)
            {
                for (int j = i; j < LevelSize; j++)
                {
                    int temp = _grid[i, j];
                    _grid[i,j] = _grid[j,i];
                    _grid[j,i] = temp;
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
                    tempColor = _grid[firstPos.x,firstPos.y];
                    secondPos = Flip(firstPos);
                    _grid[firstPos.x,firstPos.y] = _grid[secondPos.x,secondPos.y];
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
    }
}
