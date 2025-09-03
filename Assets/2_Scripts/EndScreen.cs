using TMPro;
using UnityEngine;

public class EndScreen : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI finalScoreText;
    [SerializeField] ScoreKeeper scoreKeeper;

    public void ShowFinalScore()
    {
        finalScoreText.text = "축하합니다! WrWn" +
            $"당신의 점수는 {scoreKeeper.CalculateScore()}% 입니다!";
    }

}
