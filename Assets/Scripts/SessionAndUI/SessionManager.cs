using m1m1c_3DAstarPathfinding;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SessionManager : MonoBehaviour
{

    public static UnityEvent StartSessionEvent = new UnityEvent();
    public static UnityEvent EndSessionEvent = new UnityEvent();
    public static bool isInSession { get; private set; }

    public Heap<ScoreKeeper> snakeScores = new Heap<ScoreKeeper>(8);

    public GameObject SetupUI;

    public Text SnakeSlideNumber;
    public Text PlayTimeSlideNumber;
    public Text PlayTimerText;

    public RectTransform ScoreSlotHolder;
    public ScoreSlot scoreSlotPrefab;

    private SnakeSpawner snakeSpawner;

    private List<ScoreSlot> snakeScoreSlots = new List<ScoreSlot>();

    private float playTimeThreshold = 60f;
    private float sessionTimer = 0f;


    private float originalScoreSlotYOffset = 110;
    private float scoreSlotYOffset = 110;

    void Start()
    {
        snakeSpawner = GetComponent<SnakeSpawner>();
    }

    //Increases session timer and stops session if the timer reches the playtimeThreshold
    private void Update()
    {

        if (!isInSession) { return; }
        sessionTimer += Time.deltaTime;
        PlayTimerText.text = $"{sessionTimer.ToString("F2")} / {playTimeThreshold}";

        if (snakeScores.Count > 0 && snakeScoreSlots.Count > 0)
        {
            UpdateScoreSlots();
        }

        if (sessionTimer >= playTimeThreshold)
        {
            isInSession = false;
            EndSessionEvent.Invoke();
            SetupUI.SetActive(true);
        }
    }

    //Updates the Score slot UI to show who is winning
    private void UpdateScoreSlots()
    {
        var index = 0;
        snakeScores.Sort();
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
    }

    //Removes setup UI and starts game session
    public void StartSession()
    {
        if (SetupUI) { SetupUI.SetActive(false); }

        snakeScores.Clear();
        StartSessionEvent.Invoke();

        RemoveScoreSlotsIfPresent();
        InitaliseScoreSlots();

        sessionTimer = 0f;
        isInSession = true;
    }

    private void RemoveScoreSlotsIfPresent()
    {
        if (snakeScoreSlots.Count > 0)
        {
            for (int i = snakeScoreSlots.Count - 1; i >= 0; i--)
            {
                Destroy(snakeScoreSlots[i].gameObject);
            }
            snakeScoreSlots = new List<ScoreSlot>();
        }
    }

    //Adds score slot UI to the screen and gives each the correct index and color.
    //Stores newly created slots in snakeScoreSlots list
    private void InitaliseScoreSlots()
    {
        scoreSlotYOffset = originalScoreSlotYOffset;
        var firstSlot = true;
        foreach (var item in snakeScores)
        {
            if (item == null) { continue; }

            var spawnPos = ScoreSlotHolder.position - new Vector3(0f, scoreSlotYOffset, 0f);

            var slot = Instantiate(scoreSlotPrefab, spawnPos, Quaternion.identity, ScoreSlotHolder);

            if (firstSlot)
            {
                firstSlot = false;
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
        playTimeThreshold = ((float)value) * 60f;
        if (PlayTimeSlideNumber) { PlayTimeSlideNumber.text = "" + value; }
    }


}
