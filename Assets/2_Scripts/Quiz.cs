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
    public bool isComplete;

    void Start()
    {
        timer = FindFirstObjectByType<Timer>();
        scoreKeeper = FindFirstObjectByType<ScoreKeeper>();
        progressBar.maxValue = questions.Count;
        progressBar.value = 0;
        GetNextQuestion();
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
            if (questions.Count <= 0)
            {
                GameManager.Instance.ShowEndSceen();
            }
            timer.loadNextQuestion = false;
            GetNextQuestion();
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
