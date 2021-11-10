using m1m1c_3DAstarPathfinding;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject PlayerPrefab;
    void Start()
    {
        var navVolume = GetComponent<NavVolume>();

        var pos = navVolume.GetNodeFromIndex(new Vector3Int(1, 1, 1)).WorldPosition;

        Instantiate(PlayerPrefab, pos, Quaternion.identity);
    }
}
