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
    [SerializeField] TextMeshProUGUI timerText;          // 남은 시간 텍스트(TMP)
    [SerializeField] float fallbackSolutionTime = 3f;     // Timer에 solutionTime이 없을 때 사용할 해설시간(미사용시 무시)
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
    [SerializeField] int totalQuestionsToFinish = 10;     // 10문제 풀면 종료
    [SerializeField] GameObject winCanvas;                // WinCanvas를 에디터에서 연결
    private int answeredCount = 0;                        // 처리 완료된 문제 수(정답/오답/시간초과 포함)
    private bool solutionShownThisQuestion = false;       // 현재 문제에서 해설이 이미 떴는지
    private bool gameEnded = false;                       // 종료 플래그

    [Header("힌트")]
    [SerializeField] TextMeshProUGUI hintText;           // 힌트 표시 TMP
    [SerializeField] Button hintButton;                   // 힌트 버튼
    private bool hintShownThisQuestion = false;           // 이번 문제에서 힌트를 이미 봤는지

    [Header("입력(단축키)")]
    [SerializeField] KeyCode hintKey = KeyCode.H;         // 힌트 키 (기본 H)

    void Start()
    {
        timer = FindFirstObjectByType<Timer>();
        scoreKeeper = FindFirstObjectByType<ScoreKeeper>();

        if (chatGPTClient != null)
            chatGPTClient.quizGenerateHandler += QuizGeneratedHadler;

        // 진행바를 "끝낼 문제 수" 기준으로 설정
        if (progressBar != null)
        {
            progressBar.maxValue = totalQuestionsToFinish;
            progressBar.value = 0;
        }

        // 힌트 UI 초기화: 처음엔 숨김
        HideHint();
        if (hintButton)
        {
            hintButton.interactable = true;
            hintButton.gameObject.SetActive(false);
            // ★ 버튼 클릭으로 힌트 뜨게 연결
            hintButton.onClick.AddListener(OnHintButtonClicked);
        }

        if (questions.Count <= 0)
        {
            GenerateQuestionslfNeeded();
        }
        else
        {
            lnitalizeProgressBar();
        }

        UpdateTimerUI(); // 초기 타이머 텍스트 표시
    }

    private void GenerateQuestionslfNeeded()
    {
        if (isGenerateQuestions) return;

        isGenerateQuestions = true;
        if (GameManager.Instance != null)
            GameManager.Instance.ShowLoadingSceen(); // 프로젝트에 이미 있는 함수 사용

        string topicToUse = GetTrendingTopic();
        if (chatGPTClient != null)
        {
            chatGPTClient.GenerateQuestions(questionCount, topicToUse);
            Debug.Log($"GenerateQuestionslfNeeded {topicToUse}");
        }
        else
        {
            Debug.LogError("ChatGPTClient가 연결되지 않았습니다.");
            if (loadingText) loadingText.text = "문제 생성에 실패했습니다.\nChatGPTClient를 연결하세요.";
        }
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
            if (loadingText) loadingText.text = "문제 생성에 실패했습니다.\n인터넷 연결을 확인하고 다시 시도하세요.";
            return;
        }

        questions.AddRange(gemeratedQuestions);
        GetNextQuestion();
    }

    private void lnitalizeProgressBar()
    {
        if (progressBar != null) progressBar.value = 0;
    }

    private void Update()
    {
        if (gameEnded) return;

        // 타이머 이미지/게이지
        if (timerimage != null && timer != null)
        {
            timerimage.sprite = timer.isProblemTime ? problemTimerionSprite : solutTimerionSprite;
            timerimage.fillAmount = timer.fillamount;
        }

        // 남은 시간 텍스트 갱신
        UpdateTimerUI();

        // 문제 풀이 시간에만 힌트 버튼 보이기(아직 안봤고, 힌트가 있을 때만)
        if (hintButton)
        {
            bool hasHintBtn = currentQuestion != null && !string.IsNullOrWhiteSpace(currentQuestion.GetHint());
            bool showDuringSolveBtn = timer == null ? true : timer.isProblemTime; // timer 없으면 일단 보이도록 처리
            hintButton.gameObject.SetActive(showDuringSolveBtn && !hintShownThisQuestion && hasHintBtn);
        }

        // ▼▼▼ 단축키(H)로 힌트 띄우기
        bool hasHint = currentQuestion != null && !string.IsNullOrWhiteSpace(currentQuestion.GetHint());
        bool duringSolve = (timer == null) ? true : timer.isProblemTime;
        if (!gameEnded && duringSolve && hasHint && !hintShownThisQuestion && Input.GetKeyDown(hintKey))
        {
            OnHintButtonClicked();
        }

        // 다음 문제 로드 플래그
        if (timer != null && timer.loadNextQuestion)
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
        if (timer != null && !timer.isProblemTime && !chooseAnswer)
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

        if (timer != null) timer.loadNextQuestion = false;

        if (GameManager.Instance != null)
            GameManager.Instance.ShowQuizSceen();

        chooseAnswer = false;
        solutionShownThisQuestion = false;

        // 힌트 UI 리셋: 새 문제 시작마다 숨김
        hintShownThisQuestion = false;
        HideHint();
        if (hintButton)
        {
            hintButton.interactable = true;
            hintButton.gameObject.SetActive(false); // 문제 표시 직후엔 일단 숨김(아래 OnDisplayQuestion에서 판단)
        }

        SetButtonState(true);
        SetDefaultButtonSprites();
        GetRandomQuesion();
        OnDisplayQuestion();
        if (scoreKeeper != null) scoreKeeper.IncrementQuestionSeen();

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
        if (currentQuestion == null)
        {
            Debug.LogWarning("현재 문제가 없습니다.");
            return;
        }

        Debug.Log("문제 표시 " + currentQuestion.GetQuestion());
        if (questionText) questionText.text = currentQuestion.GetQuestion();

        Debug.Log("힌트 표시 : " + currentQuestion.GetHint());
        for (int i = 0; i < answerButtons.Length; i++)
        {
            var label = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = currentQuestion.GetAnswers(i);
        }

        // 힌트 버튼 표시 여부(힌트가 있는 문제 + 풀이 시간)
        if (hintButton)
        {
            bool hasHintLocal = !string.IsNullOrWhiteSpace(currentQuestion.GetHint());
            bool showDuringSolve = timer == null ? true : timer.isProblemTime;
            hintButton.gameObject.SetActive(showDuringSolve && hasHintLocal && !hintShownThisQuestion);

            // 버튼 라벨에 단축키 표기 붙이기 (예: "힌트 (H)")
            var btnLabel = hintButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnLabel != null)
            {
                string baseText = string.IsNullOrWhiteSpace(btnLabel.text) ? "힌트" : btnLabel.text.Split('(')[0].TrimEnd(); // 기존 (H) 중복 방지
                btnLabel.text = $"{baseText} ({hintKey})";
            }
        }
    }

    public void OnAnswerButtonClicked(int index)
    {
        if (gameEnded) return;

        chooseAnswer = true;
        DisplaySolution(index);
        if (timer != null) timer.CancelTimer();

        // 정답일 때만 점수 계산
        if (currentQuestion != null && index == currentQuestion.GetCorrectAnswerIndex() && scoreKeeper != null)
        {
            int points = 0;

            if (timer != null)
            {
                if (timer.elapsedTime <= 3f) points = 5;
                else if (timer.elapsedTime <= 7f) points = 3;
                else if (timer.elapsedTime <= timer.problemTime) points = 1;
            }
            else
            {
                points = 1; // 타이머가 없다면 기본 점수
            }

            scoreKeeper.AddScore(points);
        }

        if (scoreText != null && scoreKeeper != null)
            scoreText.text = $"Score: {scoreKeeper.GetScore()}";
    }

    private void DisplaySolution(int index)
    {
        // 현재 문제에 대해 해설이 이미 떴다면 중복 처리 방지
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
                questionText.text = "정답을 틀렸습니다! 정답은 " + currentQuestion.GetCorrectAnswer();
        }

        SetButtonState(false);

        // 해설 시간/정답 공개 시 힌트 버튼은 숨김
        if (hintButton) hintButton.gameObject.SetActive(false);

        // 문제 하나 처리 완료
        answeredCount++;
        if (progressBar != null)
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
            if (buttonImage != null)
                buttonImage.sprite = defaultAnswerSprite;
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

    // 남은 시간 “N초” 텍스트 표시
    private void UpdateTimerUI()
    {
        if (timerText == null || timer == null) return;

        float total = timer.isProblemTime ? timer.problemTime : timer.solutionTime;
        float remain = Mathf.Max(0f, total - timer.elapsedTime);

        timerText.text = $"{Mathf.CeilToInt(remain)}초";
    }

    private void EndGame()
    {
        if (gameEnded) return;
        gameEnded = true;

        if (timer != null) timer.CancelTimer();
        SetButtonState(false);

        // 종료 시 힌트 UI 숨김
        HideHint();
        if (hintButton) hintButton.gameObject.SetActive(false);

        // WinCanvas 활성화
        if (winCanvas != null) winCanvas.SetActive(true);
        else Debug.LogWarning("WinCanvas가 연결되지 않았습니다. Quiz 인스펙터에 할당하세요.");

        // 최종 점수 출력
        var end = FindFirstObjectByType<EndScreen>();
        if (end != null) end.ShowFinalScore();
        else Debug.LogWarning("EndScreen을 찾을 수 없습니다. 씬에 배치했는지 확인하세요.");
    }

    // ===== 힌트 버튼 핸들러 & 유틸 =====
    public void OnHintButtonClicked()
    {
        if (currentQuestion == null || hintShownThisQuestion) return;

        string hint = currentQuestion.GetHint();
        if (string.IsNullOrWhiteSpace(hint)) hint = "힌트가 준비되지 않았어요!";

        ShowHint(hint);               // 버튼/키 입력 시 텍스트 보이게 + 내용 표시
        hintShownThisQuestion = true; // 중복 방지

        if (hintButton)
        {
            hintButton.interactable = false;        // 더 못 누르게
            hintButton.gameObject.SetActive(false); // 원하면 true 유지 가능
        }
    }

    private void ShowHint(string text)
    {
        if (!hintText) return;
        hintText.gameObject.SetActive(true);   // ★ 보이게!
        hintText.text = $"힌트: {text}";
    }

    private void HideHint()
    {
        if (!hintText) return;
        hintText.gameObject.SetActive(false);  // ★ 숨기기
        hintText.text = "";
    }
}
