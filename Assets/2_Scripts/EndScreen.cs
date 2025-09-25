using TMPro;
using UnityEngine;

public class EndScreen : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI finalScoreText;
    [SerializeField] ScoreKeeper scoreKeeper;

    public void ShowFinalScore()
    {
        finalScoreText.text = "축하합니다!\n" +
            $"당신의 점수는 {scoreKeeper.GetScore()}점입니다.";
    }
}
