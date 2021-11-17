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

    private NavVolume navVolume;

    private List<RespawnItem> queuedRespawns = new List<RespawnItem>();

    [Range(1, 8)] private int snakesToSpawn = 1;
    private float respawnTime = 3f;

    public void SetSnakesToSpawn(int value)
    {
        snakesToSpawn = value;
    }

    private void Start()
    {
        navVolume = GetComponent<NavVolume>();
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
                SpawnSnake(resItem.SpawnNode);
                queuedRespawns.RemoveAt(i);
            }
        }
    }

    private void SpawnMultipleSnakes()
    {
        ClearRespawns();
        for (int i = 0; i < snakesToSpawn; i++)
        {
            var node = navVolume.GetNodeFromIndex(spawnCoordinates[i]);
            if (node == null) { continue; }

            SpawnSnake(node);
        }
    }

    private void SpawnSnake(Node node)
    {
        var pos = node.WorldPosition;
        var snakeInstance = Instantiate(SnakePrefab, pos, Quaternion.identity);
        var snakeHead = snakeInstance.GetComponentInChildren<SnakeHead>();
        snakeHead.Setup(node);
        snakeHead.SnakeDeathEvent.AddListener(QueueSnakeRespawn);
        SessionManager.EndSessionEvent.AddListener(snakeHead.KillSnake);
    }

    private void QueueSnakeRespawn(Node spawnNode)
    {
        var respawnItem = new RespawnItem(spawnNode, respawnTime);

        if(queuedRespawns.Exists((q)=> q.SpawnNode == respawnItem.SpawnNode)) { return; }
        queuedRespawns.Add(respawnItem);
    }

    private void ClearRespawns()
    {
        queuedRespawns = new List<RespawnItem>();
    }

    private class RespawnItem
    {
        public Node SpawnNode { get; private set; }
        public float TimeTilSpawn { get; private set; }
        public float RespawnTime { get; set; }
        public RespawnItem(Node spawnNode, float timeTilSpawn)
        {
            SpawnNode = spawnNode;
            TimeTilSpawn = timeTilSpawn;
            RespawnTime = 0;
        }
    }
}
