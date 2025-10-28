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
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] float fallbackSolutionTime = 3f;
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
    [SerializeField] int totalQuestionsToFinish = 10;
    [SerializeField] GameObject winCanvas;
    int answeredCount = 0;
    bool solutionShownThisQuestion = false;
    bool gameEnded = false;

    [Header("힌트")]
    [SerializeField] TextMeshProUGUI hintText;
    [SerializeField] Button hintButton;
    bool hintShownThisQuestion = false;

    [Header("입력(단축키)")]
    [SerializeField] KeyCode hintKey = KeyCode.H;

    [Header("시작 옵션")]
    [SerializeField] bool startImmediately = false; // 메뉴 시작이면 false

    [Header("힌트 자동 표시")]
    [SerializeField] bool autoShowHint = true;
    [SerializeField, Min(0f)] float autoHintThreshold = 5f;

    string topicOverride = null;

    void Awake()
    {
        // 비활성 상태여도 실행됨
        if (chatGPTClient != null)
            chatGPTClient.quizGenerateHandler += QuizGeneratedHadler;
    }

    void OnDestroy()
    {
        if (chatGPTClient != null)
            chatGPTClient.quizGenerateHandler -= QuizGeneratedHadler;
    }

    void Start()
    {
        timer = FindFirstObjectByType<Timer>();
        scoreKeeper = FindFirstObjectByType<ScoreKeeper>();

        if (progressBar != null)
        {
            progressBar.maxValue = totalQuestionsToFinish;
            progressBar.value = 0;
        }

        HideHint();
        if (hintButton)
        {
            hintButton.interactable = true;
            hintButton.gameObject.SetActive(false);
            hintButton.onClick.AddListener(OnHintButtonClicked);
        }

        if (questions.Count <= 0)
        {
            if (startImmediately) GenerateQuestionslfNeeded();
            else lnitalizeProgressBar();
        }
        else lnitalizeProgressBar();

        UpdateTimerUI();
    }

    public void BeginFromMenu(string topicOrNull)
    {
        topicOverride = topicOrNull;
        ResetRunState();
        GenerateQuestionslfNeeded();
    }

    void ResetRunState()
    {
        gameEnded = false;
        chooseAnswer = false;
        answeredCount = 0;
        solutionShownThisQuestion = false;
        hintShownThisQuestion = false;

        if (progressBar) { progressBar.maxValue = totalQuestionsToFinish; progressBar.value = 0; }
        if (winCanvas) winCanvas.SetActive(false);
        HideHint();
        SetButtonState(false);

        questions.Clear();

        if (timer != null)
        {
            timer.CancelTimer();
            timer.loadNextQuestion = false;
        }
    }

    void GenerateQuestionslfNeeded()
    {
        if (isGenerateQuestions) return;
        isGenerateQuestions = true;

        if (GameManager.Instance) GameManager.Instance.ShowLoadingSceen();

        string topicToUse = string.IsNullOrWhiteSpace(topicOverride) ? GetTrendingTopic() : topicOverride;

        if (chatGPTClient != null)
        {
            chatGPTClient.GenerateQuestions(questionCount, topicToUse);
            Debug.Log($"GenerateQuestionslfNeeded {topicToUse}");
        }
        else
        {
            Debug.LogError("ChatGPTClient 미연결");
            if (loadingText) loadingText.text = "문제 생성 실패: ChatGPTClient 연결 필요.";
        }
    }

    string GetTrendingTopic()
    {
        string[] topics = { "과학", "역사", "음악", "영화", "스포츠", "기술", "문학", "예술", "지리", "정치" };
        int i = UnityEngine.Random.Range(0, topics.Length);
        return topics[i];
    }

    void QuizGeneratedHadler(List<QuestionSO> generated)
    {
        isGenerateQuestions = false;

        if (generated != null && generated.Count > 0)
        {
            questions.AddRange(generated);
            GetNextQuestion();
            return;
        }

        // 폴백
        Debug.LogWarning("문제 생성 실패 → 로컬 폴백 사용");
        UseFallbackQuestions();
        if (GameManager.Instance) GameManager.Instance.ShowQuizSceen();
        GetNextQuestion();
    }

    void UseFallbackQuestions()
    {
        questions.Clear();
        void Add(string q, string[] a, int idx, string h)
        {
            var so = ScriptableObject.CreateInstance<QuestionSO>();
            so.SetData(q, a, idx, h);
            questions.Add(so);
        }

        string t = string.IsNullOrWhiteSpace(topicOverride) ? "무작위" : topicOverride;
        if (t.Contains("상식"))
            Add("지구에서 가장 깊은 해구는?", new[] { "마리아나", "통가", "쿠릴", "자와" }, 0, "태평양 서부");
        else if (t.Contains("넌센스"))
            Add("세상에서 가장 무서운 차는?", new[] { "덤프트럭", "유령버스", "전기차", "과속카" }, 1, "버스가...");
        else if (t.Contains("맞춤법"))
            Add("맞는 표기는?", new[] { "되려", "돼려", "되여", "되였어" }, 0, "‘되-’ 활용");
        else
            Add("파이에 가장 가까운 값?", new[] { "3.14", "3.10", "3.20", "3.05" }, 0, "원주율");

        Add("가장 큰 행성은?", new[] { "목성", "토성", "천왕성", "해왕성" }, 0, "줄무늬");
        Add("빛의 속도 단위?", new[] { "m/s", "kg", "N·m", "A" }, 0, "SI 조합");
    }

    void lnitalizeProgressBar() { if (progressBar != null) progressBar.value = 0; }

    void Update()
    {
        if (gameEnded) return;

        if (timerimage != null && timer != null)
        {
            timerimage.sprite = timer.isProblemTime ? problemTimerionSprite : solutTimerionSprite;
            timerimage.fillAmount = timer.fillamount;
        }

        UpdateTimerUI();

        if (hintButton)
        {
            bool hasHintBtn = currentQuestion != null && !string.IsNullOrWhiteSpace(currentQuestion.GetHint());
            bool showDuringSolveBtn = timer == null ? true : timer.isProblemTime;
            hintButton.gameObject.SetActive(showDuringSolveBtn && !hintShownThisQuestion && hasHintBtn);
        }

        // === 자동 힌트: 문제 풀이 시간에 남은 시간이 임계치 이하이면 1회 표시 ===
        if (autoShowHint && !hintShownThisQuestion && currentQuestion != null && timer != null && timer.isProblemTime)
        {
            bool hasHint = !string.IsNullOrWhiteSpace(currentQuestion.GetHint());
            if (hasHint)
            {
                float remain = Mathf.Max(0f, timer.problemTime - timer.elapsedTime);
                if (remain <= autoHintThreshold)
                {
                    OnHintButtonClicked();   // 내부에서 중복 방지됨
                }
            }
        }
        // === 자동 힌트 끝 ===

        bool hasHint2 = currentQuestion != null && !string.IsNullOrWhiteSpace(currentQuestion.GetHint());
        bool duringSolve = (timer == null) ? true : timer.isProblemTime;
        if (!gameEnded && duringSolve && hasHint2 && !hintShownThisQuestion && Input.GetKeyDown(hintKey))
            OnHintButtonClicked();

        if (timer != null && timer.loadNextQuestion)
        {
            if (answeredCount >= totalQuestionsToFinish) { EndGame(); return; }
            if (questions.Count == 0) GenerateQuestionslfNeeded();
            else GetNextQuestion();
        }

        if (timer != null && !timer.isProblemTime && !chooseAnswer)
            DisplaySolution(-1);
    }


    void GetNextQuestion()
    {
        if (gameEnded) return;
        if (answeredCount >= totalQuestionsToFinish) { EndGame(); return; }
        if (questions.Count <= 0) { Debug.Log("문제 없음"); return; }

        if (timer != null) timer.loadNextQuestion = false;
        if (GameManager.Instance) GameManager.Instance.ShowQuizSceen();

        chooseAnswer = false;
        solutionShownThisQuestion = false;
        hintShownThisQuestion = false;
        HideHint();

        if (hintButton)
        {
            hintButton.interactable = true;
            hintButton.gameObject.SetActive(false);
        }

        SetButtonState(true);
        SetDefaultButtonSprites();
        GetRandomQuesion();
        OnDisplayQuestion();
        if (scoreKeeper != null) scoreKeeper.IncrementQuestionSeen();
        UpdateTimerUI();
    }

    void GetRandomQuesion()
    {
        int i = UnityEngine.Random.Range(0, questions.Count);
        currentQuestion = questions[i];
        questions.RemoveAt(i);
    }

    void OnDisplayQuestion()
    {
        if (currentQuestion == null) return;

        if (questionText) questionText.text = currentQuestion.GetQuestion();

        for (int i = 0; i < answerButtons.Length; i++)
        {
            var label = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = currentQuestion.GetAnswers(i);
        }
    }

    public void OnAnswerButtonClicked(int index)
    {
        if (gameEnded) return;

        chooseAnswer = true;
        DisplaySolution(index);
        if (timer != null) timer.CancelTimer();

        if (currentQuestion != null && index == currentQuestion.GetCorrectAnswerIndex() && scoreKeeper != null)
        {
            int pts = 1;
            if (timer != null)
            {
                if (timer.elapsedTime <= 3f) pts = 5;
                else if (timer.elapsedTime <= 7f) pts = 3;
                else if (timer.elapsedTime <= timer.problemTime) pts = 1;
            }
            scoreKeeper.AddScore(pts);
        }

        if (scoreText != null && scoreKeeper != null)
            scoreText.text = $"Score: {scoreKeeper.GetScore()}";
    }

    void DisplaySolution(int index)
    {
        if (solutionShownThisQuestion) return;
        solutionShownThisQuestion = true;

        if (currentQuestion != null && index == currentQuestion.GetCorrectAnswerIndex())
        {
            if (questionText) questionText.text = "정답!";
            if (index >= 0 && index < answerButtons.Length)
                answerButtons[index].GetComponent<Image>().sprite = correctAnswerSprite;
        }
        else
        {
            if (questionText && currentQuestion != null)
                questionText.text = "틀렸습니다! 정답은 " + currentQuestion.GetCorrectAnswer();
        }

        SetButtonState(false);
        if (hintButton) hintButton.gameObject.SetActive(false);

        answeredCount++;
        if (progressBar != null)
            progressBar.value = Mathf.Min(answeredCount, (int)progressBar.maxValue);

        if (answeredCount >= totalQuestionsToFinish) EndGame();
    }

    void SetDefaultButtonSprites()
    {
        foreach (GameObject obj in answerButtons)
        {
            var img = obj.GetComponent<Image>();
            if (img != null) img.sprite = defaultAnswerSprite;
        }
    }

    public void SetButtonState(bool state)
    {
        foreach (GameObject obj in answerButtons)
        {
            var btn = obj.GetComponent<Button>();
            if (btn != null) btn.interactable = state;
        }
    }

    void UpdateTimerUI()
    {
        if (timerText == null || timer == null) return;
        float total = timer.isProblemTime ? timer.problemTime : timer.solutionTime;
        float remain = Mathf.Max(0f, total - timer.elapsedTime);
        timerText.text = $"{Mathf.CeilToInt(remain)}초";
    }

    void EndGame()
    {
        if (gameEnded) return;
        gameEnded = true;

        if (timer != null) timer.CancelTimer();
        SetButtonState(false);
        HideHint();
        if (hintButton) hintButton.gameObject.SetActive(false);

        if (winCanvas != null) winCanvas.SetActive(true);
        var end = FindFirstObjectByType<EndScreen>();
        if (end != null) end.ShowFinalScore();
    }

    public void OnHintButtonClicked()
    {
        if (currentQuestion == null || hintShownThisQuestion) return;

        string hint = currentQuestion.GetHint();
        if (string.IsNullOrWhiteSpace(hint)) hint = "힌트가 없습니다.";
        ShowHint(hint);
        hintShownThisQuestion = true;
        if (hintButton) { hintButton.interactable = false; hintButton.gameObject.SetActive(false); }
    }

    void ShowHint(string text)
    {
        if (!hintText) return;
        hintText.gameObject.SetActive(true);
        hintText.text = $"힌트: {text}";
    }

    void HideHint()
    {
        if (!hintText) return;
        hintText.gameObject.SetActive(false);
        hintText.text = "";
    }
}
