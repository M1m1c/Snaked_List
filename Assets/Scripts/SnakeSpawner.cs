using m1m1c_3DAstarPathfinding;
using UnityEngine;

public class SnakeSpawner : MonoBehaviour
{
    public Transform SnakeParent;
    public SnakeHead SnakePrefab;
    public Vector3Int SpawnCoordinate = new Vector3Int(1, 1, 1);
    void Start()
    {
        var navVolume = GetComponent<NavVolume>();
        var node = navVolume.GetNodeFromIndex(SpawnCoordinate);
        var pos = node.WorldPosition;
        var snakeInstance = Instantiate(SnakePrefab, pos, Quaternion.identity,SnakeParent);
        snakeInstance.Setup(node);
    }
}
