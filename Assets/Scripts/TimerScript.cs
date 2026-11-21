using TMPro;
using UnityEngine;

public class TimerScript : MonoBehaviour
{
    public float startTime;
    public BaseMovement playerScript;
    private float time;
    private bool running = true;
    private TextMeshProUGUI text;
    private void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        time = startTime;
    }
    void Update()
    {
        if (!running)
        {
            return;
        }
        time -= Time.deltaTime;
        displayTime();
        if (time <= 0)
        {
            running = false;
            Debug.Log("Dream timer ran out: player frozen (endGame Timeout). Raise Start Time on the Timer Text to play longer.");
            playerScript.endGame(DeathType.Timeout);
        }
    }
    private void displayTime()
    {
        int seconds = (int)time % 60;
        int minutes = (int)time / 60;
        string fillerZero;
        if (seconds < 10)
        {
            fillerZero = "0";
        } else
        {
            fillerZero = "";
        }
        if (time < 0)
        {
            text.SetText("0:00");
        } else
        {
            text.SetText(minutes + ":" + fillerZero + seconds);
        }
    }
    public void setTime(float t)
    {
        time = t;
    }
    public void addTime(float amount)
    {
        if (time < 0)
        {
            time = 0;
        }
        time += amount;
    }
    public void subtractTime(float amount)
    {
        time -= amount;
    }
    public void stopTimer()
    {
        running = false;
    }
    public void continueTimer()
    {
        running = true;
    }
}