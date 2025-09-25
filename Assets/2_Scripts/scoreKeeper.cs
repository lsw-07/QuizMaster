using System;
using UnityEngine;

public class ScoreKeeper : MonoBehaviour
{
    int score = 0;
    int questionSeen = 0;

    public int GetScore()
    {
        return score;
    }

    public void AddScore(int points)
    {
        score += points;
    }

    public int GetQuestionSeen()
    {
        return questionSeen;
    }

    public void IncrementQuestionSeen()
    {
        questionSeen++;
    }

    internal void IncrementCorrectAnswers()
    {
        throw new NotImplementedException();
    }
}
