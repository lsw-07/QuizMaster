using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Quiz : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI questionText;
    [SerializeField] QuestionSO question;
    //[SerializeField] TextMeshProUGUI[] answerTextArr;
    [SerializeField] GameObject[] answerButtons;
    [SerializeField] Sprite defaultAnswerSprite;
    [SerializeField] Sprite correctAnswerSprite;


    void Start()
    {
        GetNextQuestion();
    }

    public void OnAnswerButtonClicked(int index)
    {
        answerButtons[question.GetCorrectAnswerIndex()].GetComponent<Image>().sprite = correctAnswerSprite;
        if (index == question.GetCorrectAnswerIndex())
        {
            questionText.text = "정답!";
        }
        else
        {
            questionText.text = "정답을 틀렸습니다! 정답은    " + question.GetCorrectAnswer();
        }
        SetButtonState(false);
    }
    void GetNextQuestion()
    {
        SetButtonState(true);
        SetDefaultButtonSprites();
        OnDisplayQuestion();
    }

    private void OnDisplayQuestion()
    {
        questionText.text = question.GetQuestion();
        for (int i = 0; i < answerButtons.Length; i++)
        {
            TextMeshProUGUI answerText = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            answerText.text = question.GetAnswers(i);

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
}
