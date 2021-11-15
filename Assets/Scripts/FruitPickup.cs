using UnityEngine;
using UnityEngine.Events;

public class FruitPickup : MonoBehaviour
{
    public UnityEvent FruitEatenEvent = new UnityEvent();


    private void OnTriggerEnter(Collider other)
    {
        var snakeHead = other.gameObject.GetComponent<SnakeHead>();
        if (snakeHead)
        {
            snakeHead.AddSnakeSegment();
            FruitEatenEvent.Invoke();
            Destroy(gameObject);
        }
    }
}
