using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Quiz quiz;
    [SerializeField] private EndScreen endScreen;
    [SerializeField] private GameObject loadingCanvas;
    [SerializeField] private GameObject quizCanvasRoot;   // ← QuizCanvas 루트 참조 추가

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        // 초기 상태는 에디터에서: Start Canvas ON, QuizCanvas/WinCanvas OFF
        // 필요 시 여기서 강제하고 싶으면 아래 주석 해제:
        // if (quizCanvasRoot) quizCanvasRoot.SetActive(false);
        // if (endScreen) endScreen.gameObject.SetActive(false);
        // if (loadingCanvas) loadingCanvas.SetActive(false);
    }

    private void ShowQuizScreen()
    {
        if (quizCanvasRoot) quizCanvasRoot.SetActive(true);   // ← 루트 켜기
        if (quiz) quiz.gameObject.SetActive(true);

        if (endScreen) endScreen.gameObject.SetActive(false);
        if (loadingCanvas) loadingCanvas.SetActive(false);
    }

    public void ShowEndSceen()
    {
        if (quiz) quiz.gameObject.SetActive(false);
        if (quizCanvasRoot) quizCanvasRoot.SetActive(false);  // ← 루트 끄기

        if (endScreen)
        {
            endScreen.gameObject.SetActive(true);
            endScreen.ShowFinalScore();
        }
        if (loadingCanvas) loadingCanvas.SetActive(false);
    }

    public void ShowLoadingSceen()
    {
        if (quiz) quiz.gameObject.SetActive(false);
        if (quizCanvasRoot) quizCanvasRoot.SetActive(false);  // ← 루트 끄기

        if (endScreen) endScreen.gameObject.SetActive(false);
        if (loadingCanvas) loadingCanvas.SetActive(true);
    }

    public void OnReplayLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    internal void ShowQuizSceen()
    {
        ShowQuizScreen(); // 표기만 다름
    }
}
