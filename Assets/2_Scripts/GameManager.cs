using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Quiz quiz;
    [SerializeField] private EndScreen endScreen;
    [SerializeField] private GameObject loadingCanvas;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        //ShowQuizScreen();
    }

    private void ShowQuizScreen()
    {
        quiz.gameObject.SetActive(true);
        endScreen.gameObject.SetActive(false);
        loadingCanvas.SetActive(false);
    }
    public void ShowEndSceen()
    {
        quiz.gameObject.SetActive(false);
        endScreen.gameObject.SetActive(true);
        endScreen.ShowFinalScore();
        loadingCanvas.SetActive(false);

    }

    public void ShowLoadingSceen()
    {
        quiz.gameObject.SetActive(false);
        endScreen.gameObject.SetActive(false);
        loadingCanvas.SetActive(true);
    }
    public void OnReplayLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
