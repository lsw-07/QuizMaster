using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Quiz quiz;
    [SerializeField] private EndScreen endScreen;
    [SerializeField] private GameObject loadingCanvas;
    [SerializeField] private GameObject quizCanvasRoot;   // QuizCanvas ∑Á∆Æ

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); }
    }

    private void SetGO(GameObject go, bool on) { if (go) go.SetActive(on); }

    private void ShowQuizScreen()
    {
        SetGO(quizCanvasRoot, true);
        if (quiz) quiz.gameObject.SetActive(true);
        if (endScreen) endScreen.gameObject.SetActive(false);
        SetGO(loadingCanvas, false);
    }

    public void ShowEndSceen()
    {
        if (quiz) quiz.gameObject.SetActive(false);
        SetGO(quizCanvasRoot, false);
        if (endScreen)
        {
            endScreen.gameObject.SetActive(true);
            endScreen.ShowFinalScore();
        }
        SetGO(loadingCanvas, false);
    }

    public void ShowLoadingSceen()
    {
        if (quiz) quiz.gameObject.SetActive(false);
        SetGO(quizCanvasRoot, false);
        if (endScreen) endScreen.gameObject.SetActive(false);
        SetGO(loadingCanvas, true);
    }

    public void OnReplayLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    internal void ShowQuizSceen() { ShowQuizScreen(); }
}
