using UnityEngine;

public class QuizMenu : MonoBehaviour
{
    [SerializeField] private Quiz quiz;          // QuizCanvas안의 Quiz 컴포넌트
    [SerializeField] private GameObject menuCanvas; // Start Canvas 루트

    public void OnGeneral() { StartMode("상식"); }     // 상식만
    public void OnNonsense() { StartMode("넌센스"); }   // 넌센스만
    public void OnSpelling() { StartMode("맞춤법"); }   // 맞춤법만
    public void OnRandom() { StartMode("무작위"); }   // 무작위(랜덤)

    private void StartMode(string topic)
    {
        if (menuCanvas) menuCanvas.SetActive(false);
        if (GameManager.Instance) GameManager.Instance.ShowLoadingSceen(); // 로딩 표시
        if (quiz) quiz.BeginFromMenu(topic); // 선택 주제로 시작
    }
}
