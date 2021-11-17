using m1m1c_3DAstarPathfinding;
using System.Collections.Generic;
using UnityEngine;

public class SnakeSpawner : MonoBehaviour
{
    public GameObject SnakePrefab;

    private Vector3Int[] spawnCoordinates = new Vector3Int[]
    {
        new Vector3Int(1, 1, 1),
        new Vector3Int(18,18,18),
        new Vector3Int(18, 1, 18),
        new Vector3Int(1,18,1),
        new Vector3Int(1, 18, 18),
        new Vector3Int(18,1,1),
        new Vector3Int(18,18,1),
        new Vector3Int(1,1,18),
    };

    private Color[] snakeColors = new Color[]
    {
        Color.cyan,
        Color.red,
        Color.green,
        Color.yellow,
        Color.blue,
        Color.magenta,
        new Color(1f,0.7f,0.3f,1f),
        new Color(0.3f,1f,0.7f,1f)
    };

    private NavVolume navVolume;
    private FruitSpawner fruitSpawner;
    private SessionManager sessionManager;

    private List<RespawnItem> queuedRespawns = new List<RespawnItem>();

    [Range(1, 8)] public int snakesToSpawn = 1;
    private float respawnTime = 3f;

    public void SetSnakesToSpawn(int value)
    {
        snakesToSpawn = value;
    }

    private void Start()
    {
        navVolume = GetComponent<NavVolume>();
        fruitSpawner = GetComponent<FruitSpawner>();
        sessionManager = GetComponent<SessionManager>();
        SessionManager.StartSessionEvent.AddListener(SpawnMultipleSnakes);
    }

    private void Update()
    {
        if (!SessionManager.isInSession) { return; }

        if (queuedRespawns.Count == 0) { return; }

        for (int i = queuedRespawns.Count - 1; i >= 0; i--)
        {
            var resItem = queuedRespawns[i];
            if (resItem == null)
            {
                queuedRespawns.RemoveAt(i);
                continue;
            }

            resItem.RespawnTime += Time.deltaTime;

            if (resItem.RespawnTime >= resItem.TimeTilSpawn)
            {
                SpawnSnake(resItem.SnakeIndex);
                queuedRespawns.RemoveAt(i);
            }
        }
    }

    private void SpawnMultipleSnakes()
    {
        ClearRespawns();
        for (int i = 0; i < snakesToSpawn; i++)
        {
            sessionManager.snakeScores.Add(new SnakeScoreKeeper(i, snakeColors[i]));

            SpawnSnake(i);
        }
    }

    private void SpawnSnake(int snakeIndex)
    {
        var node = navVolume.GetNodeFromIndex(spawnCoordinates[snakeIndex]);
        if (node == null) { return; }
        var color = snakeColors[snakeIndex];

        var pos = node.WorldPosition;
        var snakeInstance = Instantiate(SnakePrefab, pos, Quaternion.identity);
        var snakeHead = snakeInstance.GetComponentInChildren<SnakeHead>();

        var scoreKeper = sessionManager.snakeScores.GetItemWithKey(snakeIndex);
        if (scoreKeper == null) { return; }
        snakeHead.MyScoreKeeper = scoreKeper;

        snakeHead.Setup(node, color, snakeIndex);
        snakeHead.SnakeDeathEvent.AddListener(QueueSnakeRespawn);
        SessionManager.EndSessionEvent.AddListener(snakeHead.KillSnake);

        var fruitNode = fruitSpawner.GetFruitNode();
        if (fruitNode != null)
        {
            snakeHead.InformFruitExistsInLevel(fruitNode);
        }
    }

    private void QueueSnakeRespawn(int snakeIndex)
    {
        var respawnItem = new RespawnItem(snakeIndex, respawnTime);

        if (queuedRespawns.Exists((q) => q.SnakeIndex == respawnItem.SnakeIndex)) { return; }
        queuedRespawns.Add(respawnItem);
    }

    private void ClearRespawns()
    {
        queuedRespawns = new List<RespawnItem>();
    }

    private class RespawnItem
    {
        //public Node SpawnNode { get; private set; }

        public int SnakeIndex { get; private set; }
        public float TimeTilSpawn { get; private set; }
        //public Color color { get; private set; }
        public float RespawnTime { get; set; }
        public RespawnItem(int index, float timeTilSpawn)
        {
            SnakeIndex = index;
            TimeTilSpawn = timeTilSpawn;
            RespawnTime = 0;
        }
    }
}
