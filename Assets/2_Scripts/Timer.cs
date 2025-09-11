using UnityEngine;

public class Timer : MonoBehaviour
{
    [SerializeField] float problemTime = 10f;
    [SerializeField] float solutionTime = 3f;
    float time = 0;

    [HideInInspector] public bool isProblemTime = true;
    [HideInInspector] public float fillamount;
    [HideInInspector] public bool loadNextQuestion;

    private void Start()
    {
        time = problemTime;
        loadNextQuestion = true;
    }

    private void Update()
    {
        TimerCountDowm();
        UpdateFillAmount();

    }

    private void UpdateFillAmount()
    {
        if (isProblemTime)
            fillamount = time / problemTime;

        else
            fillamount = time / solutionTime;
    }

    private void TimerCountDowm()
    {
        time -= Time.deltaTime;
        if (time <= 0f)
        {
            if (isProblemTime)
            {
                isProblemTime = false;
                time = solutionTime;
            }
            else
            {
                isProblemTime = true;
                time = problemTime;
                loadNextQuestion = true;
            }
        }
    }
    public void CancelTimer()
    {
        time = 0;
    }
}

