using UnityEngine;
[CreateAssetMenu(menuName = "Quiz Question", fileName = "New Question")]
public class QuestionSO : ScriptableObject
{
    [TextArea(2, 6)]
    [SerializeField] string question = "여기에 질문을 적어주세요.";
    [SerializeField] string[] answers = new string[4];
    [SerializeField] int correctAnswerIndex = 0;

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
}
