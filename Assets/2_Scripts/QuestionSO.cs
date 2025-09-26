using System;
using UnityEngine;
[CreateAssetMenu(menuName = "Quiz Question", fileName = "New Question")]
public class QuestionSO : ScriptableObject
{
    [TextArea(2, 6)]
    [SerializeField] string question = "여기에 질문을 적어주세요.";
    [SerializeField] string[] answers = new string[4];
    [SerializeField] int correctAnswerIndex = 0;
    [SerializeField] string hint = "힌트를 여기에 적어주세요.";

    public string GetQuestion()
    {
       return question;
    }
    public string GetAnswers(int index)
    {
       return answers[index];
    }

    public string GetCorrectAnswer()
    {
       return answers[correctAnswerIndex];
    }

    public int GetCorrectAnswerIndex()
    {
       return correctAnswerIndex;
    }

    public string GetHint()
    {
       return hint;
    }   

    public void SetData(string q, string[] a, int correctIndex, string hint)
    {
        SetData(q, a, correctIndex);
        this.hint = hint;   
    }

    public void SetData(string q, string[] a, int correctIndex)
    {
        question = q;
        answers = a;
        correctAnswerIndex = correctIndex;
    }

}
