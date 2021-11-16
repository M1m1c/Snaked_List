using m1m1c_3DAstarPathfinding;
using UnityEngine;

public class SnakeSpawner : MonoBehaviour
{
    public GameObject SnakePrefab;

    [Range(1,8)]public int SnakesToSpawn = 1;

    private Vector3Int[] SpawnCoordinates = new Vector3Int[] 
    { 
        new Vector3Int(1, 1, 1),
        new Vector3Int(18,18,18),
        new Vector3Int(1, 18, 18),
        new Vector3Int(18, 1, 18),
        new Vector3Int(18,18,1),
        new Vector3Int(18,1,1),
        new Vector3Int(1,18,1),
        new Vector3Int(1,1,18),
    };

    void Start()
    {
        var navVolume = GetComponent<NavVolume>();

        for (int i = 0; i < SnakesToSpawn; i++)
        {
            var node = navVolume.GetNodeFromIndex(SpawnCoordinates[i]);
            if (node == null) { continue; }

            var pos = node.WorldPosition;
            var snakeInstance = Instantiate(SnakePrefab, pos, Quaternion.identity);
            var snakeHead = snakeInstance.GetComponentInChildren<SnakeHead>();
            snakeHead.Setup(node);
        }
    }
}
