using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SessionManager : MonoBehaviour
{

    public static UnityEvent StartSessionEvent = new UnityEvent();
    public static UnityEvent EndSessionEvent = new UnityEvent();
    public static bool isInSession { get; private set; }

    public Heap<SnakeScoreKeeper> snakeScores = new Heap<SnakeScoreKeeper>(8);

    public GameObject SetupUI;

    public Text SnakeSlideNumber;
    public Text PlayTimeSlideNumber;
    public Text PlayTimerText;

    public RectTransform ScoreSlotHolder;
    public SnakeScoreSlot scoreSlotPrefab;

    private SnakeSpawner snakeSpawner;

    private List<SnakeScoreSlot> snakeScoreSlots = new List<SnakeScoreSlot>();

    private float playTime = 60f;
    private float sessionTimer = 0f;


    private float originalScoreSlotYOffset = 110;
    private float scoreSlotYOffset = 110;

    void Start()
    {
        snakeSpawner = GetComponent<SnakeSpawner>();
    }

    private void Update()
    {

        if (!isInSession) { return; }
        sessionTimer += Time.deltaTime;
        PlayTimerText.text = $"{sessionTimer.ToString("F2")} / {playTime}";

        foreach (var score in snakeScores)
        {
            snakeScores.UpdateItem(score);
        }

        var index = 0;
        foreach (var score in snakeScores)
        {
            if (index > snakeScoreSlots.Count) { break; }
            var slot = snakeScoreSlots[index];
            if (slot == null) { continue; }
            slot.SetImageColor(score.SnakeColor);
            slot.SetSnakeIndex(score.SnakeIndex);
            slot.SetFruitsEaten(score.FruitsEaten);
            slot.SetLifeTime(score.LifeTime);
            index++;
        }

        if (sessionTimer >= playTime)
        {
            isInSession = false;
            EndSessionEvent.Invoke();
            SetupUI.SetActive(true);
        }
    }

    public void StartSession()
    {
        if (SetupUI) { SetupUI.SetActive(false); }


        snakeScores.Clear();
        StartSessionEvent.Invoke();
        sessionTimer = 0f;
        isInSession = true;


        if (snakeScoreSlots.Count > 0)
        {
            for (int i = snakeScoreSlots.Count - 1; i >= 0; i--)
            {
                Destroy(snakeScoreSlots[i].gameObject);
            }
            snakeScoreSlots = new List<SnakeScoreSlot>();
        }

        scoreSlotYOffset = originalScoreSlotYOffset;
        var firstTime = false;
        foreach (var item in snakeScores)
        {
            if (item == null) { continue; }

            var slot = Instantiate(
                scoreSlotPrefab,
                ScoreSlotHolder.position - new Vector3(0f, scoreSlotYOffset, 0f),
                Quaternion.identity,
                ScoreSlotHolder);

            if (!firstTime)
            {
                firstTime = true;
                var slotTransform = slot.gameObject.transform;
                slotTransform.localScale += new Vector3(.3f, .3f, .3f);
                slotTransform.position += new Vector3(-39f, 20f, 0f);
            }

            scoreSlotYOffset += originalScoreSlotYOffset;

            slot.SetImageColor(item.SnakeColor);
            slot.SetSnakeIndex(item.SnakeIndex);

            snakeScoreSlots.Add(slot);

        }
    }

    public void SetSnakesToSpawn(System.Single value)
    {
        snakeSpawner.SetSnakesToSpawn((int)value);
        if (SnakeSlideNumber) { SnakeSlideNumber.text = "" + value; }
    }

    public void SetPlayTime(System.Single value)
    {
        playTime = ((float)value) * 60f;
        if (PlayTimeSlideNumber) { PlayTimeSlideNumber.text = "" + value; }
    }

}
