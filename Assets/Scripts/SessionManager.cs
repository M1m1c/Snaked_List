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

    public GameObject SetupUI;

    public Text SnakeSlideNumber;
    public Text PlayTimeSlideNumber;
    public Text PlayTimerText;

    private SnakeSpawner snakeSpawner;

    private float playTime = 60f;
    private float sessionTimer = 0f;

    void Start()
    {
        snakeSpawner = GetComponent<SnakeSpawner>();
    }

    private void Update()
    {

        if (!isInSession) { return; }
        sessionTimer += Time.deltaTime;
        PlayTimerText.text = $"{sessionTimer.ToString("F2")} / {playTime}";

        if (sessionTimer >= playTime)
        {
            isInSession = false;
            EndSessionEvent.Invoke();
            SetupUI.SetActive(true);
            //TODO end session
        }
    }

    public void StartSession()
    {
        if (SetupUI) { SetupUI.SetActive(false); }
        StartSessionEvent.Invoke();
        sessionTimer = 0f;
        isInSession = true;
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
