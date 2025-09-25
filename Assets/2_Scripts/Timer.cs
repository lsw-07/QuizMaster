using UnityEngine;

public class Timer : MonoBehaviour
{
    [SerializeField] public float problemTime = 15f;  // public 으로 변경해서 Quiz에서 접근 가능
    [SerializeField] float solutionTime = 5f;
    float time = 0;

    [HideInInspector] public bool isProblemTime = true;
    [HideInInspector] public float fillamount;
    [HideInInspector] public bool loadNextQuestion;

    // 문제 푸는 시간 기록
    [HideInInspector] public float elapsedTime;

    private void Start()
    {
        time = problemTime;
        loadNextQuestion = true;
        elapsedTime = 0;
    }

    private void Update()
    {
        TimerCountDown();
        UpdateFillAmount();

        if (isProblemTime)
        {
            elapsedTime += Time.deltaTime;
        }
    }

    private void UpdateFillAmount()
    {
        if (isProblemTime)
            fillamount = time / problemTime;
        else
            fillamount = time / solutionTime;
    }

    private void TimerCountDown()
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
                elapsedTime = 0; // 다음 문제 시작할 때 초기화
            }
        }
    }

    public void CancelTimer()
    {
        time = 0;
    }

    public void ResetElapsedTime()
    {
        elapsedTime = 0;
    }
}
