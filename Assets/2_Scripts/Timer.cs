using UnityEngine;

public class Timer : MonoBehaviour
{
    [SerializeField] public float problemTime = 15f;   // Quiz에서 접근 가능
    [SerializeField] public float solutionTime = 5f;   

    float time = 0f;

    [HideInInspector] public bool isProblemTime = true;
    [HideInInspector] public float fillamount;
    [HideInInspector] public bool loadNextQuestion;

    // 현재 "해당 단계(문제/해설)"에서 경과 시간
    [HideInInspector] public float elapsedTime;

    private void Start()
    {
        time = problemTime;
        loadNextQuestion = true;  // 첫 문제 로드 신호
        elapsedTime = 0f;
    }

    private void Update()
    {
        TimerCountDown();
        UpdateFillAmount();

        //  문제/해설 상관없이 현재 단계 경과 시간 계속 증가
        elapsedTime += Time.deltaTime;
    }

    private void UpdateFillAmount()
    {
        float total = isProblemTime ? problemTime : solutionTime;
        fillamount = Mathf.Clamp01(time / total); // 1 → 0
    }

    private void TimerCountDown()
    {
        time -= Time.deltaTime;

        if (time <= 0f)
        {
            if (isProblemTime)
            {
                // ★ 문제 → 해설 전환 시, 해설 단계 시간으로 세팅하고 경과시간 리셋
                isProblemTime = false;
                time = solutionTime;
                elapsedTime = 0f;            // 해설 카운트다운용
            }
            else
            {
                // 해설 종료 → 다음 문제 신호
                isProblemTime = true;
                time = problemTime;
                loadNextQuestion = true;
                elapsedTime = 0f;            //  다음 문제 카운트다운용
            }
        }
    }

    // 정답 버튼에서 "바로 해설로" 넘길 때 사용 (기존 동작 유지)
    public void CancelTimer()
    {
        time = 0f;  // 다음 Update에서 즉시 단계 전환
    }

    public void ResetElapsedTime()
    {
        elapsedTime = 0f;
    }
}
