using UnityEngine;
using UnityEngine.UI;

public class ScoreSlot : MonoBehaviour
{
    public Image SnakeColorImage;
    public Text SnakeIndexText;
    public Text FruitsEatenText;
    public Text LifeTimeText;

    public void SetImageColor(Color color)
    {
        SnakeColorImage.color = color;
    }

    public void SetSnakeIndex(int index)
    {
        SnakeIndexText.text = "" + index;
    }

    public void SetFruitsEaten(int count)
    {
        FruitsEatenText.text = "" + count;
    }

    public void SetLifeTime(float time)
    {
        LifeTimeText.text = time.ToString("F2");
    }
}
