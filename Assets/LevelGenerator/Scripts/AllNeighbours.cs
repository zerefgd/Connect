using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AllNeighbours : MonoBehaviour, GenerateMethod
{
    [SerializeField] private TMP_Text _timerText, _gridCountText;
    [SerializeField] private bool _showOnlyResult;
    private List<GridData> checkingGrid;
    private LevelGenerator Instance;
    private bool isCreating;

    [SerializeField] private float speedMultipler;
    [SerializeField] private float speed;

    private void Start()
    {
        checkingGrid= new List<GridData>();
        Instance = GetComponent<LevelGenerator>();
        isCreating =true;
    }

    public void Generate()
    {
        StartCoroutine(GeneratePaths());
    }

    private IEnumerator GeneratePaths()
    {
        for (int i = 0; i <= Instance.levelSize/2; i++)
        {
            for (int j = 0; j <= i; j++)
            {
                GridData tempGrid = new GridData(i,j,Instance.levelSize);
                checkingGrid.Add(tempGrid);
            }
        }
        yield return new WaitForSeconds(speed);

        StartCoroutine(SolvePaths());

        int count = 0;

        while(isCreating)
        {
            if(count == checkingGrid.Count)
            {
                yield break;
            }

            
            while (_showOnlyResult && !checkingGrid[count].IsGridComplete())
            {
                count++;
                if (count == checkingGrid.Count)
                {
                    yield break;
                }
            }
            

            Instance.RenderGrid(checkingGrid[count]._grid);
            count++;

            _timerText.text = count.ToString();
            _gridCountText.text = checkingGrid.Count.ToString();

            yield return new WaitForSeconds(speed);
        }
    }

    private List<Vector2Int> directionChecks = new List<Vector2Int>()
    { Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right };

    private IEnumerator SolvePaths()
    {
        bool canSolve = true;
        int iterPerFrame = (int)(speedMultipler / speed);
        int currentIter = 0;

        while(canSolve)
        {
            List<GridData> resultGridData = new List<GridData>();

            foreach (var item in checkingGrid)
            {
                if(!item.IsSolved)
                {
                    resultGridData.Add(item);
                }
            }

            if(resultGridData.Count == 0)
            {
                canSolve = false;
                yield break;
            }

            foreach (var item in resultGridData)
            {
                int posIndex = checkingGrid.IndexOf(item);
                int insertIndex = 1;

                foreach (var direction in directionChecks)
                {
                    Vector2Int checkingDirection = item.CurrentPos + direction;

                    if(item.IsInsideGrid(checkingDirection)
                        && item._grid[checkingDirection] == -1
                        && item.IsNotNeighbour(checkingDirection)
                        )
                    {
                        checkingGrid.Insert(posIndex + insertIndex,
                            new GridData(checkingDirection.x,checkingDirection.y,
                            item.ColorId,item)
                            );
                        insertIndex++;
                    }
                }

                foreach (var emptyPos in item.EmptyPosition())
                {
                    if(item.FlowLength() > 2)
                    {
                        checkingGrid.Insert(posIndex + insertIndex,
                            new GridData(emptyPos.x, emptyPos.y,
                            item.ColorId + 1, item)
                            );
                        insertIndex++;
                    }
                }

                item.IsSolved = true;

                currentIter++;

                if(currentIter > iterPerFrame)
                {
                    currentIter = 0;
                    yield return new WaitForSeconds(speed);
                }
            }
        }
    }
}

public class GridData
{
    private static List<Vector2Int> directionChecks = new List<Vector2Int>()
    { Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right };

    public Dictionary<Vector2Int, int> _grid;
    public bool IsSolved;
    public Vector2Int CurrentPos;
    public int ColorId;

    public GridData(int i,int j,int levelSize)
    {
        _grid = new Dictionary<Vector2Int, int>();

        for (int a = 0; a < levelSize; a++)
        {
            for (int b = 0; b < levelSize; b++)
            {
                _grid[new Vector2Int(a,b)] = -1;
            }
        }
        IsSolved = false;
        CurrentPos = new Vector2Int(i,j);
        ColorId = 0;
        _grid[CurrentPos] = ColorId;
    }

    public GridData(int i,int j,int passedColor,GridData gridCopy)
    {
        _grid = new Dictionary<Vector2Int, int>();

        foreach (var item in gridCopy._grid)
        {
            _grid[item.Key] = item.Value; 
        }

        CurrentPos= new Vector2Int(i,j);
        ColorId = passedColor;
        _grid[CurrentPos] = ColorId;
        IsSolved = false;
    }

    public bool IsInsideGrid(Vector2Int pos) 
    {
        return _grid.ContainsKey(pos);
    }

    public bool IsGridComplete()
    {
        foreach (var item in _grid)
        {
            if(item.Value == -1) return false;
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

    public bool IsNotNeighbour(Vector2Int pos)
    {
        foreach (var item in _grid)
        {
            if(item.Value == ColorId && item.Key != CurrentPos)
            {
                foreach (var direction in directionChecks)
                {
                    if(pos - item.Key == direction)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    public List<Vector2Int> EmptyPosition()
    {
        List<Vector2Int> result = new List<Vector2Int>();

        foreach (var item in _grid)
        {
            if(item.Value == -1)
                result.Add(item.Key);
        }

        return result;
    }

    public int FlowLength()
    {
        int result = 0;
        foreach (var item in _grid)
        {
            if(item.Value == ColorId)
                result++;
        }

        return result;
    }
}
