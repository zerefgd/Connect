using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;

public class AllStartSpawn : MonoBehaviour,GenerateMethod
{
    private LevelGenerator Instance;

    private Dictionary<Vector2Int, int> currentGrid;
    private List<Dictionary<Vector2Int, int>> resultGrid; 

    private void Start()
    {
        Instance = GetComponent<LevelGenerator>();
        currentGrid= new Dictionary<Vector2Int, int>();
        resultGrid = new List<Dictionary<Vector2Int, int>>();
    }

    public void Generate()
    {
        StartCoroutine(SpawnRandom());
    }

    private IEnumerator SpawnRandom()
    {
        bool isSpawning = true;

        ResetGrid();
        SetStartNodes();

        while (isSpawning)
        {
            ResetGrid();
            Instance.RenderGrid(currentGrid);
            yield return new WaitForSeconds(0.125f);
        }
    }

    private void ResetGrid()
    {
        currentGrid.Clear();

        for (int i = 0; i < Instance.levelSize; i++)
        {
            for (int j = 0; j < Instance.levelSize; j++)
            {
                currentGrid[new Vector2Int(i, j)] = -1;
            }
        }
    }

    private List<Vector2Int> directions = new List<Vector2Int>()
    { Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right};

    private void SetStartNodes()
    {
        int startSize = 2;
        int endSize = Instance.levelSize * Instance.levelSize / 3;

        List<Vector2Int> spawnList = currentGrid.Keys.ToList();

        HashSet<Vector2Int> pair = new HashSet<Vector2Int>();
        PairComparer comparer = new PairComparer();
        GridComparer gridComparer = new GridComparer();
        HashSet<HashSet<Vector2Int>> resultPairs = new HashSet<HashSet<Vector2Int>>(comparer);


        foreach (var firstPos in spawnList)
        {
            foreach (var secondPos in spawnList)
            {
                pair.Add(firstPos);
                pair.Add(secondPos);
                if(pair.Count > 1)
                {
                    resultPairs.Add(pair);
                }
                pair = new HashSet<Vector2Int>();
            }
        }

        Debug.Log(resultPairs.Count);

        HashSet<HashSet<Vector2Int>> grid = new HashSet<HashSet<Vector2Int>>(comparer);
        HashSet<HashSet<HashSet<Vector2Int>>> gridSet = new HashSet<HashSet<HashSet<Vector2Int>>>(gridComparer);

        foreach (var firstPair in resultPairs)
        {
            foreach (var secondPair in resultPairs)
            {
                grid.Add(firstPair);
                grid.Add(secondPair);
                if(grid.Count > 1)
                {
                    gridSet.Add(grid);
                }
                grid = new HashSet<HashSet<Vector2Int>>();
            }
        }

        Debug.Log(gridSet.Count);
    }
}

public class PairComparer : IEqualityComparer<HashSet<Vector2Int>>
{    
    public bool Equals(HashSet<Vector2Int> x, HashSet<Vector2Int> y)
    {
        if(x.Count != y.Count) return false;

        foreach (var item in x)
        {
            if(!y.Contains(item)) return false;
        }

        return true;
    }

    public int GetHashCode(HashSet<Vector2Int> obj)
    {
        int final = 0;
        foreach (var item in obj) 
        {
            final += (int)item.magnitude;
        }

        return final.GetHashCode();
    }
}

public class GridComparer : IEqualityComparer<HashSet<HashSet<Vector2Int>>>
{
    public bool Equals(HashSet<HashSet<Vector2Int>> x, HashSet<HashSet<Vector2Int>> y)
    {
        if(x.Count != y.Count) return false;

        foreach (var item in x)
        {
            if(!y.Contains(item)) return false;
        }

        return true;
    }

    public int GetHashCode(HashSet<HashSet<Vector2Int>> obj)
    {
        int result = 0;

        foreach (var item in obj)
        {
            result += (int)item.GetHashCode();
        }

        return result;
    }
}
