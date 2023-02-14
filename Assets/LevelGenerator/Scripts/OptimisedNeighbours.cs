using Connect.Common;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Connect.Generator.OptimisedNeighbours
{
    public class OptimisedNeighbours : MonoBehaviour,GenerateMethod
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
            GridSet =  new HashSet<GridList>(setComparer);
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

                    if(!GridSet.Contains(tempList))
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

                if(solveList == null)
                {
                    canSolve = false;
                    yield break;
                }

                item = resultGridList.Data;

                foreach (var direction in directionChecks)
                {
                    checkingDirection = item.CurrentPos + direction;

                    if (item.IsInsideGrid(checkingDirection)
                        && item._grid[checkingDirection] == -1
                        && item.IsNotNeighbour(checkingDirection)
                        )
                    {
                        tempGrid = new GridData(checkingDirection.x,checkingDirection.y,item.ColorId,item);
                        tempList = new GridList(tempGrid);

                        if(!GridSet.Contains(tempList))
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
                if(!GridSet.Contains(tempList))
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
        private static List<Point> directionChecks = new List<Point>()
        { Point.up,Point.left,Point.down,Point.right };

        public bool Equals(GridList x, GridList y)
        {
            Dictionary<Point,int> firstGrid,secondGrid;
            firstGrid = x.Data._grid;
            secondGrid = y.Data._grid;

            Dictionary<int,int> colorSwap = new Dictionary<int, int>();
            Point[] posKeys = firstGrid.Keys.ToArray();
            Point pos;
            bool isEmpty = false;

            for (int i = 0; i < posKeys.Length; i++)
            {
                pos = posKeys[i];

                if (!isEmpty && firstGrid[pos] == -1 || secondGrid[pos] == -1)
                {
                    isEmpty = true;
                }

                if ((firstGrid[pos] == -1 && secondGrid[pos] != -1) ||
                    (firstGrid[pos] != -1 && secondGrid[pos] == -1))
                {
                    return false;
                }

                if (!colorSwap.ContainsKey(firstGrid[pos]))
                {
                    colorSwap[firstGrid[pos]] = secondGrid[pos];  
                }
                else
                {
                    if (colorSwap[firstGrid[pos]] != secondGrid[pos])
                    {
                        return false;
                    }
                }
            }

            if(isEmpty && x.Data.CurrentPos != y.Data.CurrentPos)
            {
                return false;
            }

            return true;
        }

        public int GetHashCode(GridList obj)
        {
            string resultString = "";

            Point startPos, checkPos;

            startPos = Point.zero;

            for (int i = 0; i < GridData.LevelSize; i++)
            {
                for (int j = 0; j < GridData.LevelSize; j++)
                {
                    startPos.x = i;
                    startPos.y = j;

                    if (obj.Data._grid[startPos] != -1)
                    {
                        foreach (var d in directionChecks)
                        {
                            checkPos = startPos + d;
                            if(obj.Data.IsInsideGrid(checkPos) &&
                                obj.Data._grid[checkPos] == obj.Data._grid[startPos])
                            {
                                resultString += "1";
                            }
                            else
                            {
                                resultString += "0";
                            }
                        }
                    }
                    else
                    {
                        resultString = "0000";
                    }
                }
            }

            return resultString.GetHashCode();
        }
    }

    public class GridData
    {
        private static List<Point> directionChecks = new List<Point>()
        { Point.up,Point.down,Point.left,Point.right };

        public Dictionary<Point, int> _grid;
        public bool IsSolved;
        public Point CurrentPos;
        public int ColorId;
        public static int LevelSize;

        public GridData(int i, int j, int levelSize)
        {
            _grid = new Dictionary<Point, int>();

            for (int a = 0; a < levelSize; a++)
            {
                for (int b = 0; b < levelSize; b++)
                {
                    _grid[new Point(a, b)] = -1;
                }
            }
            IsSolved = false;
            CurrentPos = new Point(i, j);
            ColorId = 0;
            _grid[CurrentPos] = ColorId;
            LevelSize = levelSize;
        }

        public GridData(int i, int j, int passedColor, GridData gridCopy)
        {
            _grid = new Dictionary<Point, int>();

            foreach (var item in gridCopy._grid)
            {
                _grid[item.Key] = item.Value;
            }

            CurrentPos = new Point(i, j);
            ColorId = passedColor;
            _grid[CurrentPos] = ColorId;
            IsSolved = false;
        }

        public bool IsInsideGrid(Point pos)
        {
            return _grid.ContainsKey(pos);
        }

        public bool IsGridComplete()
        {
            foreach (var item in _grid)
            {
                if (item.Value == -1) return false;
            }

            for (int i = 0; i <= ColorId; i++)
            {
                int result = 0;

                foreach (var item in _grid)
                {
                    if (item.Value == i)
                        result++;
                }

                if (result < 3)
                    return false;

            }

            return true;
        }

        public bool IsNotNeighbour(Point pos)
        {
            foreach (var item in _grid)
            {
                if (item.Value == ColorId && item.Key != CurrentPos)
                {
                    foreach (var direction in directionChecks)
                    {
                        if (pos - item.Key == direction)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public List<Point> EmptyPosition()
        {
            List<Point> result = new List<Point>();

            foreach (var item in _grid)
            {
                if (item.Value == -1)
                    result.Add(item.Key);
            }

            return result;
        }

        public int FlowLength()
        {
            int result = 0;
            foreach (var item in _grid)
            {
                if (item.Value == ColorId)
                    result++;
            }

            return result;
        }

        public List<GridData> GetSimilar()
        {
            List<GridData> result = new List<GridData>();

            GridData addData;

            for(int i = 0; i < 4; i++)
            {
                addData = new GridData(CurrentPos.x, CurrentPos.y, ColorId, this);
                addData.Rotate(i);
                result.Add(addData);
                addData = new GridData(CurrentPos.x, CurrentPos.y, ColorId, this);
                addData.Flip(i);
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
            Dictionary<Point,int> result = new Dictionary<Point,int>();

            for (int i = 0; i < LevelSize; i++)
            {
                for (int j = 0; j < LevelSize; j++)
                {
                    result[Rotate(new Point(i, j))] = _grid[new Point(i, j)];
                }
            }

            for (int i = 0; i < LevelSize; i++)
            {
                for (int j = 0; j < LevelSize; j++)
                {
                    _grid[new Point(i, j)] = result[new Point(i, j)];
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


        public void Flip(int flip)
        {
            Rotate(flip);

            int tempColor;
            Point firstPos, secondPos;

            for (int i = 0; i < LevelSize/2; i++)
            {
                for (int j = 0; j < LevelSize; j++)
                {
                    firstPos = new Point(i, j);
                    tempColor = _grid[firstPos];
                    secondPos = Flip(firstPos);
                    _grid[firstPos] = _grid[secondPos];
                    _grid[secondPos] = tempColor; 
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
