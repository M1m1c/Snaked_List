using m1m1c_3DAstarPathfinding;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class FruitSpawner : MonoBehaviour
{
    public static UnityEvent<Node> FruitSpawnEvent = new UnityEvent<Node>();

    public FruitPickup FruitPrefab;

    private NavVolume navVolume;

    private FruitPickup fruitInstance;

    private void Start()
    {
        navVolume = GetComponent<NavVolume>();
        StartCoroutine(LoadTimer());
    }

    private IEnumerator LoadTimer()
    {
        yield return new WaitForSeconds(0.5f);
        SpawnFruit();
    }

    private void SpawnFruit()
    {
        if (fruitInstance != null && fruitInstance.pendingDestruction == false) { return; }

        Node spawnNode;
        while (true)
        {
            spawnNode = navVolume.GetRandomNode();
            if (spawnNode.Walkable)
            {
                break;
            }
        }

        var tempRef = Instantiate(FruitPrefab, spawnNode.WorldPosition, Quaternion.identity);
        tempRef.FruitEatenEvent.AddListener(SpawnFruit);
        fruitInstance = tempRef;
        FruitSpawnEvent.Invoke(spawnNode);
    }

}
