using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseGame : MonoBehaviour
{
    public GameObject pauseMenu;
    private const string MainMenuSceneName = "MainMenu";

    public void Pause()
    {
        GameManager.Instance.background.SetActive(true);
		pauseMenu.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        GameManager.Instance.background.SetActive(false);
		pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    public void Replay()
    {
        GameManager.Instance.background.SetActive(false);
		Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        pauseMenu.SetActive(false);
    }

    public void BackToMenu()
    {
        GameManager.Instance.background.SetActive(false);
		Time.timeScale = 0f;
        pauseMenu.SetActive(false);
        SceneManager.LoadScene(MainMenuSceneName);
    }
}
