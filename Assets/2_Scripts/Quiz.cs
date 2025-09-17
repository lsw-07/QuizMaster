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

    [Header("TImer")]
    [SerializeField] Image timerimage;
    [SerializeField] Sprite problemTimerionSprite;
    [SerializeField] Sprite solutTimerionSprite;
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

    private bool isComplete;

    void Start()
    {
        timer = FindFirstObjectByType<Timer>();
        scoreKeeper = FindFirstObjectByType<ScoreKeeper>();
        chatGPTClient.quizGenerateHandler += QuizGeneratedHadler;

        if (questions.Count <= 0)
        {
            GenerateQuestionslfNeeded();
        }
        else
        {
            lnitalizeProgressBar();
        }
    }

    private void GenerateQuestionslfNeeded()
    {
        if (isGenerateQuestions) return;

        isGenerateQuestions = true;
        GameManager.Instance.ShowLoadingSceen();

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
            Debug.LogError("질문이 생성되지 않았습니다. ");
            loadingText.text = " 문제 생성에 실패했습니다 . \n 인터넷 연결을 확인하고 다시 시도하세요 . ";
            return;
        }

        questions.AddRange(gemeratedQuestions);
        progressBar.maxValue = gemeratedQuestions.Count;
        GetNextQuestion();
    }
    private void lnitalizeProgressBar()
    {
        progressBar.maxValue = questions.Count;
        progressBar.value = 0;
    }

    private void Update()
    {
        if (timer.isProblemTime)
            timerimage.sprite = problemTimerionSprite;
        else
            timerimage.sprite = solutTimerionSprite;
        timerimage.fillAmount = timer.fillamount;


        if (timer.loadNextQuestion)
        {
            if (questions.Count == 0)
            {
                GenerateQuestionslfNeeded();
            }
            else
            {
               // timer.loadNextQuestion = false;
                GetNextQuestion();
            }
        }
        if (timer.isProblemTime == false && chooseAnswer == false)
        {
            DisplaySolution(-1);
        }
    }
    private void GetNextQuestion()
    {
        if (questions.Count <= 0)
        {
            Debug.Log("더 이상 문제가 없습니다.");
            return;
        }
        timer .loadNextQuestion = false;

        GameManager. Instance.ShowQuizSceen();
        chooseAnswer = false;
        SetButtonState(true);
        SetDefaultButtonSprites();
        GetRandomQuesion();
        OnDisplayQuestion();
        scoreKeeper.IncrementQuestionSeen();
        progressBar.value++;
    }

    private void GetRandomQuesion()
    {
        int randomindex = UnityEngine.Random.Range(0, questions.Count);
        currentQuestion = questions[randomindex];
        questions.RemoveAt(randomindex);
    }

    private void OnDisplayQuestion()
    {
        Debug.Log("문제 표시" + currentQuestion.GetQuestion());
        questionText.text = currentQuestion.GetQuestion();

        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.GetAnswers(i);
        }
    }

    public void OnAnswerButtonClicked(int index)
    {
        chooseAnswer = true;
        DisplaySolution(index);
        timer.CancelTimer();
        scoreText.text = $"Score: {scoreKeeper.CalculateScore()}%";

        if (progressBar.value == progressBar.maxValue)
        {
            isComplete = true;
        }
    }

    private void DisplaySolution(int index)
    {
        if (index == currentQuestion.GetCorrectAnswerIndex())
        {
            questionText.text = "정답!";
            answerButtons[index].GetComponent<Image>().sprite = correctAnswerSprite;
            scoreKeeper.IncrementCorrectAnswers();
        }
        else
        {
            questionText.text = "정답을 틀렸습니다! 정답은    " + currentQuestion.GetCorrectAnswer();
        }
        SetButtonState(false);
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
}
