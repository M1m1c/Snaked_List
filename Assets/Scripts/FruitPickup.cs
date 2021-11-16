using UnityEngine;
using UnityEngine.Events;

public class FruitPickup : MonoBehaviour
{
    public UnityEvent FruitEatenEvent = new UnityEvent();

    public bool pendingDestruction = false;

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
