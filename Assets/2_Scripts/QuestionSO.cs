using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Quiz Question", fileName = "New Question")]
public class QuestionSO : ScriptableObject
{
    [TextArea(2, 6)]
    [SerializeField] string question = "여기에 질문을 적어주세요.";

    [SerializeField] string[] answers = new string[4];

    // 정답 인덱스를 에디터에서 안전하게 입력하도록 범위 지정
    [SerializeField, Range(0, 3)] int correctAnswerIndex = 0;

    [SerializeField] string hint = "힌트를 여기에 적어주세요.";

    public string GetQuestion() => question;

    // 범위 체크 보강(에러 방지)
    public string GetAnswers(int index)
    {
        if (answers == null || index < 0 || index >= answers.Length) return "";
        return answers[index];
    }

    public string GetCorrectAnswer()
    {
        if (answers == null || correctAnswerIndex < 0 || correctAnswerIndex >= answers.Length) return "";
        return answers[correctAnswerIndex];
    }

    public int GetCorrectAnswerIndex() => correctAnswerIndex;

    public string GetHint() => hint;

    public void SetData(string q, string[] a, int correctIndex, string hint)
    {
        SetData(q, a, correctIndex);
        this.hint = hint ?? "";
    }

    public void SetData(string q, string[] a, int correctIndex)
    {
        question = q;
        answers = (a != null && a.Length > 0) ? a : new string[4]; // 최소 4칸 확보
        correctAnswerIndex = Mathf.Clamp(correctIndex, 0, Mathf.Max(0, answers.Length - 1));
    }

    // 에디터에서 값이 바뀔 때 자동 보정(답안 4칸 유지, 정답 인덱스 보정)
    private void OnValidate()
    {
        if (answers == null || answers.Length != 4)
        {
            Array.Resize(ref answers, 4);
        }
        correctAnswerIndex = Mathf.Clamp(correctAnswerIndex, 0, answers.Length - 1);
    }
}
