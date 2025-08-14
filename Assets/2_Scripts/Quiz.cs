using TMPro;
using UnityEngine;

public class Quiz : MonoBehaviour
{
    [SerializeField]  TextMeshProUGUI questionText; 
    [SerializeField]  QuestionSO questions;
    [SerializeField]  TextMeshProUGUI[] answerTextArr = new TextMeshProUGUI[4];
    void Start()
    {
        questionText.text = questions.GetQuestion();

        Debug.Log("answerTextArr length: " + answerTextArr.Length);

        answerTextArr[0].text = questions.GetAnswers(0);
        answerTextArr[1].text = questions.GetAnswers(1); 
        answerTextArr[2].text = questions.GetAnswers(2);
        answerTextArr[3].text = questions.GetAnswers(3);

        for (int i = 0; i < answerTextArr.Length; i++)
        {
            answerTextArr[i].text = questions.GetAnswers(i);
        }
    }

}
