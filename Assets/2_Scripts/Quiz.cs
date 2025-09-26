using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Quiz : MonoBehaviour
{
    [Header("질문")]
    [SerializeField] TextMeshProUGUI questionText;
    [SerializeField] List<QuestionSO> questions = new List<QuestionSO>();
    QuestionSO currentQuestion;

    [Header("보기")]
    [SerializeField] GameObject[] answerButtons;

    [Header("버튼 색깔")]
    [SerializeField] Sprite defaultAnswerSprite;
    [SerializeField] Sprite correctAnswerSprite;

    [Header("Timer")]
    [SerializeField] Image timerimage;
    [SerializeField] Sprite problemTimerionSprite;
    [SerializeField] Sprite solutTimerionSprite;
    [SerializeField] TextMeshProUGUI timerText;          // ★ 추가: 남은 시간 글자 표시(TMP)
    [SerializeField] float fallbackSolutionTime = 3f;     // ★ Timer에 solutionTime이 없을 때 사용할 해설시간
    Timer timer;
    bool chooseAnswer = false;

    [Header("점수")]
    [SerializeField] TextMeshProUGUI scoreText;
    ScoreKeeper scoreKeeper;

    [Header("ProgressBar")]
    [SerializeField] Slider progressBar;

    [Header("ChatGPT Client")]
    [SerializeField] ChatGPTClient chatGPTClient;
    [SerializeField] int questionCount = 3;
    [SerializeField] TextMeshProUGUI loadingText;
    bool isGenerateQuestions = false;

    [Header("Game Flow")]
    [SerializeField] int totalQuestionsToFinish = 10;     // ★ 10문제 풀면 종료
    [SerializeField] GameObject winCanvas;                // ★ WinCanvas를 에디터에서 연결
    private int answeredCount = 0;                        // ★ 처리 완료된 문제 수(정답/오답/시간초과 포함)
    private bool solutionShownThisQuestion = false;       // ★ 현재 문제에서 해설이 이미 떴는지
    private bool gameEnded = false;                       // ★ 종료 플래그

    [Header("힌트")]
    [SerializeField] TextMeshProUGUI hintText;

    void Start()
    {
        timer = FindFirstObjectByType<Timer>();
        scoreKeeper = FindFirstObjectByType<ScoreKeeper>();
        chatGPTClient.quizGenerateHandler += QuizGeneratedHadler;

        // 진행바를 "끝낼 문제 수(10)" 기준으로 맞춥니다.
        progressBar.maxValue = totalQuestionsToFinish;
        progressBar.value = 0;

        if (questions.Count <= 0)
        {
            GenerateQuestionslfNeeded();
        }
        else
        {
            lnitalizeProgressBar(); // 필요하면 유지
        }

        UpdateTimerUI(); // 초기 타이머 텍스트 표시
    }

    private void GenerateQuestionslfNeeded()
    {
        if (isGenerateQuestions) return;

        isGenerateQuestions = true;
        GameManager.Instance.ShowLoadingSceen(); // 프로젝트에 이미 있는 함수 사용

        string topicToUse = GetTrendingTopic();
        chatGPTClient.GenerateQuestions(questionCount, topicToUse);
        Debug.Log($"GenerateQuestionslfNeeded {topicToUse}");
    }

    private string GetTrendingTopic()
    {
        string[] topics = new string[] { "과학", "역사", "음악", "영화", "스포츠", "기술", "문학", "예술", "지리", "정치" };
        int randomIndex = UnityEngine.Random.Range(0, topics.Length);
        return topics[randomIndex];
    }

    void QuizGeneratedHadler(List<QuestionSO> gemeratedQuestions)
    {
        isGenerateQuestions = false;

        if (gemeratedQuestions == null || gemeratedQuestions.Count == 0)
        {
            Debug.LogError("질문이 생성되지 않았습니다.");
            loadingText.text = "문제 생성에 실패했습니다.\n인터넷 연결을 확인하고 다시 시도하세요.";
            return;
        }

        questions.AddRange(gemeratedQuestions);
        // progressBar.maxValue = gemeratedQuestions.Count; // 엔딩 기준(10)에 맞추기 위해 비활성화 권장
        GetNextQuestion();
    }

    private void lnitalizeProgressBar()
    {
        progressBar.value = 0;
    }

    private void Update()
    {
        if (gameEnded) return; // 종료 후 추가 진행 방지

        // 타이머 이미지/게이지
        timerimage.sprite = timer.isProblemTime ? problemTimerionSprite : solutTimerionSprite;
        timerimage.fillAmount = timer.fillamount;

        // 남은 시간 텍스트 갱신
        UpdateTimerUI();

        // 다음 문제 로드 플래그
        if (timer.loadNextQuestion)
        {
            // 10문제 다 풀었으면 종료
            if (answeredCount >= totalQuestionsToFinish)
            {
                EndGame();
                return;
            }

            if (questions.Count == 0)
            {
                GenerateQuestionslfNeeded();
            }
            else
            {
                GetNextQuestion();
            }
        }

        // 풀이 시간이 끝났는데 선택을 안 했다면, 자동으로 해설 표시(시간초과)
        if (!timer.isProblemTime && !chooseAnswer)
        {
            DisplaySolution(-1);
        }
    }

    private void GetNextQuestion()
    {
        if (gameEnded) return;

        // 10문제 다 풀었으면 종료
        if (answeredCount >= totalQuestionsToFinish)
        {
            EndGame();
            return;
        }

        if (questions.Count <= 0)
        {
            Debug.Log("더 이상 문제가 없습니다.");
            return;
        }

        timer.loadNextQuestion = false;

        GameManager.Instance.ShowQuizSceen();
        chooseAnswer = false;
        solutionShownThisQuestion = false; // 새 문제 시작 시 초기화
        SetButtonState(true);
        SetDefaultButtonSprites();
        GetRandomQuesion();
        OnDisplayQuestion();
        scoreKeeper.IncrementQuestionSeen();

        // 문제 시작 즉시 타이머 텍스트 갱신
        UpdateTimerUI();
    }

    private void GetRandomQuesion()
    {
        int randomindex = UnityEngine.Random.Range(0, questions.Count);
        currentQuestion = questions[randomindex];
        questions.RemoveAt(randomindex);
    }

    private void OnDisplayQuestion()
    {
        Debug.Log("문제 표시 " + currentQuestion.GetQuestion());
        questionText.text = currentQuestion.GetQuestion();

        Debug.Log("힌트 표시 : " +  currentQuestion.GetHint());
        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.GetAnswers(i);
        }
    }

    public void OnAnswerButtonClicked(int index)
    {
        if (gameEnded) return;

        chooseAnswer = true;
        DisplaySolution(index);   // 이 안에서 문제 처리 수 증가 + 종료 체크까지 수행
        timer.CancelTimer();

        // 정답일 때만 점수 계산
        if (index == currentQuestion.GetCorrectAnswerIndex())
        {
            int points = 0;

            if (timer.elapsedTime <= 3f) points = 5;
            else if (timer.elapsedTime <= 7f) points = 3;
            else if (timer.elapsedTime <= timer.problemTime) points = 1;

            scoreKeeper.AddScore(points);
        }

        scoreText.text = $"Score: {scoreKeeper.GetScore()}";
    }

    private void DisplaySolution(int index)
    {
        // 현재 문제에 대해 해설이 이미 떴다면 중복 처리 방지
        if (solutionShownThisQuestion) return;
        solutionShownThisQuestion = true;

        if (index == currentQuestion.GetCorrectAnswerIndex())
        {
            questionText.text = "정답!";
            if (index >= 0 && index < answerButtons.Length)
                answerButtons[index].GetComponent<Image>().sprite = correctAnswerSprite;
        }
        else
        {
            questionText.text = "정답을 틀렸습니다! 정답은 " + currentQuestion.GetCorrectAnswer();
        }

        SetButtonState(false);

        // 문제 하나 처리 완료
        answeredCount++;
        progressBar.value = Mathf.Min(answeredCount, (int)progressBar.maxValue);

        // 10개 달성 시 종료
        if (answeredCount >= totalQuestionsToFinish)
        {
            EndGame();
        }
    }

    private void SetDefaultButtonSprites()
    {
        foreach (GameObject obj in answerButtons)
        {
            Image buttonImage = obj.GetComponent<Image>();
            buttonImage.sprite = defaultAnswerSprite;
        }
    }

    public void SetButtonState(bool state)
    {
        foreach (GameObject obj in answerButtons)
        {
            obj.GetComponent<Button>().interactable = state;
        }
    }

    // ▼ 남은 시간 “N초” 텍스트 표시
    private void UpdateTimerUI()
    {
        if (timerText == null || timer == null) return;

        float remainSeconds = 0f;

        if (timer.isProblemTime)
        {
            // 풀이 시간 남은 초
            remainSeconds = Mathf.Max(0f, timer.problemTime - timer.elapsedTime);
        }
        else
        {
            // 해설 시간 남은 초
            float totalSolution = fallbackSolutionTime;

            // 프로젝트의 Timer에 public float solutionTime 이 있다면 아래 주석 해제:
            // totalSolution = timer.solutionTime;

            remainSeconds = Mathf.Max(0f, totalSolution - timer.elapsedTime);
        }

        int display = Mathf.CeilToInt(remainSeconds);
        timerText.text = $"{display}초";
    }

    private void EndGame()
    {
        if (gameEnded) return;
        gameEnded = true;

        if (timer != null) timer.CancelTimer();
        SetButtonState(false);

        // WinCanvas 활성화
        if (winCanvas != null) winCanvas.SetActive(true);
        else Debug.LogWarning("WinCanvas가 연결되지 않았습니다. Quiz 인스펙터에 할당하세요.");

        // 최종 점수 출력
        var end = FindFirstObjectByType<EndScreen>();
        if (end != null) end.ShowFinalScore();
        else Debug.LogWarning("EndScreen을 찾을 수 없습니다. 씬에 배치했는지 확인하세요.");
    }
}
