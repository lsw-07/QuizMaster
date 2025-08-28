using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Quiz : MonoBehaviour
{
    [Header("질문")]
    [SerializeField] TextMeshProUGUI questionText;
    [SerializeField] QuestionSO question;

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

    void Start()
    {
        timer = FindFirstObjectByType<Timer>();
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
            chooseAnswer = false;
            timer.loadNextQuestion = false;
            GetNextQuestion();
        }
        if(timer.isProblemTime ==false && chooseAnswer == false)
        {
            DisplaySolution(-1);
        }
    }
    private void GetNextQuestion()
    {
        chooseAnswer = false;
        SetButtonState(true);
        SetDefaultButtonSprites();
        OnDisplayQuestion();
    }

    private void OnDisplayQuestion()
    {
        Debug.Log("문제 표시" + question.GetQuestion());
        questionText.text = question.GetQuestion();

        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = question.GetAnswers(i);
        }
    }

    public void OnAnswerButtonClicked(int index)
    {
        chooseAnswer = true;
        DisplaySolution(index);
        timer.CancelTimer();
    }

    private void DisplaySolution(int index)
    {
        if (index == question.GetCorrectAnswerIndex())
        {
            questionText.text = "정답!";
            answerButtons[index].GetComponent<Image>().sprite = correctAnswerSprite;
        }
        else
        {
            questionText.text = "정답을 틀렸습니다! 정답은    " + question.GetCorrectAnswer();
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
