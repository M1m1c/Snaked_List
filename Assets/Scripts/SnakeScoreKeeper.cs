using UnityEngine;


public class SnakeScoreKeeper : IHeapItem<SnakeScoreKeeper>
{
    public int HeapIndex { get; set; }
    public int Key { get; set; }

    public int FruitsEaten { get; set; }
    public float LifeTime { get; set; }
    public int SnakeIndex { get; private set; }
    public Color SnakeColor { get; private set; }

    public SnakeScoreKeeper(int snakeIndex, Color snakeColor)
    {
        SnakeIndex = snakeIndex;
        Key = snakeIndex;
        SnakeColor = snakeColor;
        FruitsEaten = 0;
        LifeTime = 0f;
    }

    public int CompareTo(SnakeScoreKeeper other)
    {
        var compare = 0;

        if (other != null && other != this)
        {
            var myVal = ((float)FruitsEaten) * LifeTime;
            var otherVal = ((float)other.FruitsEaten) * other.LifeTime;
            var notTooSimilar = !Mathf.Approximately(myVal, otherVal);
            var myValueIsZero = Mathf.Approximately(myVal, 0f);
            var otherValueIsZero = Mathf.Approximately(otherVal, 0f);

            if ( (myVal < otherVal || myValueIsZero) && notTooSimilar)
            {
                compare = -1;
            }
            else if ( (myVal > otherVal || otherValueIsZero) && notTooSimilar)
            {
                compare = 1;
            }
        }

        return compare;
    }
}
