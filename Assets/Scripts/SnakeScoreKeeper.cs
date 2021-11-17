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
        int compare = -1;

        var myVal = (float)FruitsEaten * LifeTime;

        if (other != null)
        {
            var otherVal = (float)other.FruitsEaten * other.LifeTime;
            if (otherVal < myVal)
            {
                compare = -compare;
            }
        }
        

        return compare;
    }
}
