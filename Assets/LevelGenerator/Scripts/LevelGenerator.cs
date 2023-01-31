using Connect.Common;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
            new Vector3(levelSize/2f,levelSize/2f,0f),
            Quaternion.identity);

        board.size = new Vector2(levelSize + 0.08f, levelSize + 0.08f);

        for (int i = 0;i < levelSize; i++)
        {
            for (int j = 0; j < levelSize; j++)
            {
                Instantiate(_bgCellPrefab,new Vector3(i+0.5f,j+0.5f,0f),Quaternion.identity);
            }
        }

        Camera.main.orthographicSize = levelSize / 1.6f + 1f;
        Camera.main.transform.position = new Vector3(levelSize / 2f, levelSize / 2f, -10f);
    }

    [SerializeField] private NodeRenderer _nodePrefab;

    public Dictionary<Vector2Int, NodeRenderer> nodeGrid;

    private void SpawnNodes()
    {
        nodeGrid = new Dictionary<Vector2Int, NodeRenderer>();
        Vector3 spawnPos;
        NodeRenderer spawnedNode;

        for (int i = 0; i < levelSize; i++)
        {
            for (int j = 0; j < levelSize; j++)
            {
                spawnPos = new Vector3(i + 0.5f, j + 0.5f, 0f);
                spawnedNode = Instantiate(_nodePrefab,spawnPos,Quaternion.identity);
                spawnedNode.Init();
                nodeGrid.Add(new Vector2Int(i,j), spawnedNode);
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

        }

        _simulateButton.SetActive(false);
    }

    [SerializeField] private LevelList _allLevelList;
    private Dictionary<string, LevelData> Levels;

    private void GenerateDefault()
    {     
        GenerateLevelData();
    }

    private LevelData currentLevelData;

    private void GenerateLevelData(int level = 0)
    {
        string currentLevelName = "Level" + stage.ToString() + level.ToString();

        if(!Levels.ContainsKey(currentLevelName))
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

    #region NODE_RENDERING

    private List<Vector2Int> directions = new List<Vector2Int>()
    { Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right}    ;

    public void RenderGrid(Dictionary<Vector2Int,int> grid)
    {
        int currentColor;
        int numOfConnectedNodes;

        foreach (var item in nodeGrid)
        {
            item.Value.Init();
            currentColor = grid[item.Key];
            numOfConnectedNodes = 0;

            if(currentColor != -1)
            {
                foreach (var direction in directions)
                {
                    if(grid.ContainsKey(item.Key + direction) &&
                        grid[item.Key + direction] == currentColor) 
                    {
                        item.Value.SetEdge(currentColor, direction);
                        numOfConnectedNodes++;
                    }
                }

                if(numOfConnectedNodes <= 1)
                {
                    item.Value.SetEdge(currentColor, Vector2Int.zero);
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
