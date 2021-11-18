using m1m1c_3DAstarPathfinding;
using UnityEngine;
using UnityEngine.Events;

public class FruitPickup : MonoBehaviour
{
    public UnityEvent FruitEatenEvent = new UnityEvent();
    public Node SpawnNode { get; private set; }

    public bool pendingDestruction = false;

    public void Setup(Node spawnNode)
    {
        SpawnNode = spawnNode;
    }

    private void OnTriggerEnter(Collider other)
    {
        var snakeHead = other.gameObject.GetComponent<SnakeHead>();
        if (snakeHead)
        {
            snakeHead.AddSnakeSegment();
            pendingDestruction = true;
            FruitEatenEvent.Invoke();
            Destroy(gameObject);
        }
    }
}
