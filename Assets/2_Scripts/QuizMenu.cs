using UnityEngine;

public class QuizMenu : MonoBehaviour
{
    [SerializeField] private Quiz quiz;              // QuizCanvas 안의 Quiz
    [SerializeField] private GameObject menuCanvas;  // Start Canvas

    public void OnGeneral() { StartMode("상식"); }
    public void OnNonsense() { StartMode("넌센스"); }
    public void OnSpelling() { StartMode("맞춤법"); }
    public void OnRandom() { StartMode("무작위"); }

    private void StartMode(string topic)
    {
        if (menuCanvas) menuCanvas.SetActive(false);
        if (GameManager.Instance) GameManager.Instance.ShowLoadingSceen();
        if (quiz) quiz.BeginFromMenu(topic);
    }
}
