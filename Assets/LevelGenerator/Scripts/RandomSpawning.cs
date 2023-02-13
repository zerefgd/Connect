using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Connect.Generator.RandomSpawning
{
    public class RandomSpawning : MonoBehaviour, GenerateMethod
    {
        private LevelGenerator Instance;

        private Dictionary<Vector2Int, int> currentGrid;

        private void Start()
        {
            Instance = GetComponent<LevelGenerator>();
            currentGrid = new Dictionary<Vector2Int, int>();
        }

        public void Generate()
        {
            StartCoroutine(SpawnRandom());
        }

        private IEnumerator SpawnRandom()
        {
            bool isSpawning = true;

            while (isSpawning)
            {
                ResetGrid();
                while (!SetStartNodes())
                {
                    ResetGrid();
                }
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

        private bool SetStartNodes()
        {
            List<Vector2Int> spawnList = currentGrid.Keys.ToList();

            int maxColors = Instance.levelSize;

            int randomFirstId, randomSecondId;
            Vector2Int firstSpawnPos, secondSpawnPos;

            for (int i = 0; i < maxColors; i++)
            {
                randomFirstId = Random.Range(0, spawnList.Count);
                randomSecondId = Random.Range(0, spawnList.Count);

                while (randomFirstId == randomSecondId)
                {
                    randomFirstId = Random.Range(0, spawnList.Count);
                    randomSecondId = Random.Range(0, spawnList.Count);
                }

                firstSpawnPos = spawnList[randomFirstId];
                secondSpawnPos = spawnList[randomSecondId];

                foreach (var direction in directions)
                {
                    if (firstSpawnPos - secondSpawnPos == direction)
                    {
                        randomFirstId = Random.Range(0, spawnList.Count);
                        randomSecondId = Random.Range(0, spawnList.Count);
                        return false;
                    }
                }

                currentGrid[firstSpawnPos] = i;
                currentGrid[secondSpawnPos] = i;
                spawnList.Remove(firstSpawnPos);
                spawnList.Remove(secondSpawnPos);
            }

            return true;
        }
    } 
}
