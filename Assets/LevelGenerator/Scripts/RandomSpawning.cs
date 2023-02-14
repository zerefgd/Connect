using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Connect.Generator.RandomSpawning
{
    public class RandomSpawning : MonoBehaviour, GenerateMethod
    {
        private LevelGenerator Instance;

        private Dictionary<Point, int> currentGrid;

        private void Start()
        {
            Instance = GetComponent<LevelGenerator>();
            currentGrid = new Dictionary<Point, int>();
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
                    currentGrid[new Point(i, j)] = -1;
                }
            }
        }

        private List<Point> directions = new List<Point>()
    { Point.up,Point.down,Point.left,Point.right};

        private bool SetStartNodes()
        {
            List<Point> spawnList = currentGrid.Keys.ToList();

            int maxColors = Instance.levelSize;

            int randomFirstId, randomSecondId;
            Point firstSpawnPos, secondSpawnPos;

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
