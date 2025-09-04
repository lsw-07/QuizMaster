using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Quiz quiz;
    [SerializeField] private EndScreen endScreen;

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

    private void Start()
    {
        ShowQuizScreen();
    }

    private void ShowQuizScreen()
    {
        quiz.gameObject.SetActive(true);
        endScreen.gameObject.SetActive(false);
    }
    public void ShowEndSceen()
    {
        quiz.gameObject.SetActive(false);
        endScreen.gameObject.SetActive(true);
        endScreen.ShowFinalScore();
    }
    public void OnReplayLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
