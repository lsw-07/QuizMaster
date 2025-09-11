using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ChatGPTClient;

public class ChatGPTClient : MonoBehaviour
{
    public delegate void QuizGeneratedHandler(List<QuestionSO> questions);
    public event QuizGeneratedHandler quizGenerataHndler;

    public void GenerateQuestions(object questionCount, string topicToUse)
    {
        Debug.Log($"Generating {questionCount} questions on the topic: {topicToUse}");

        StartCoroutine(GenerateWithDelay());
    }

    private IEnumerator GenerateWithDelay()
    {
        yield return new WaitForSeconds(3f);
        List<QuestionSO> questtions = new List<QuestionSO>();
        QuestionSO so1 = CreateQuesion("GPT 생선 질문 1", new string[] { "답변1 (정답)", "답변2", "답변3", "답변4" }, 0);
        questtions.Add(so1);
        QuestionSO so2 = CreateQuesion("GPT 생선 질문 1", new string[] { "답변1", "답변2", "답변3 (정답)", "답변4" }, 2);
        questtions.Add(so2);
        QuestionSO so3 = CreateQuesion("GPT 생선 질문 1", new string[] { "답변1", "답변2 (정답)", "답변3", "답변4" }, 1); 
        questtions.Add(so3);

        quizGenerataHndler?.Invoke(new List<QuestionSO>());
        Debug.Log("Finished GeneratWithDekay............");

    }

    QuestionSO CreateQuesion(string q, string[] a, int correctIndex)
    {
        QuestionSO so = ScriptableObject.CreateInstance<QuestionSO>();
        so.SetData(q, a, correctIndex);

        return so;
    }
}