using m1m1c_3DAstarPathfinding;
using UnityEngine;

public class SnakeSpawner : MonoBehaviour
{
    public SnakeHead snakePrefab;
    public Vector3Int SpawnCoordinate = new Vector3Int(1, 1, 1);
    void Start()
    {
        var navVolume = GetComponent<NavVolume>();
        var node = navVolume.GetNodeFromIndex(SpawnCoordinate);
        var pos = node.WorldPosition;

        var snakeInstance = Instantiate(snakePrefab, pos, Quaternion.identity);
        snakeInstance.Setup(node);
    }
}
