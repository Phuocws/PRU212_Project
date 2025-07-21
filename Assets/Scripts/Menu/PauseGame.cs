using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseGame : MonoBehaviour
{
	public GameObject pauseMenu;
	private const string MainMenuSceneName = "MainMenu";

	public void Pause()
	{
		AudioManager.Instance.PlayButtonClickSound();
		GameManager.Instance.SetPauseState(true);
		GameManager.Instance.background.SetActive(true);
		pauseMenu.SetActive(true);
		Time.timeScale = 0f;

		AudioManager.Instance.PauseAllGameAudio();
	}

	public void Resume()
	{
		AudioManager.Instance.PlayButtonClickSound();
		GameManager.Instance.SetPauseState(false);
		GameManager.Instance.background.SetActive(false);
		pauseMenu.SetActive(false);
		Time.timeScale = 1f;

		AudioManager.Instance.ResumeAllGameAudio();
	}

	public void Replay()
	{
		AudioManager.Instance.PlayButtonClickSound();
		GameManager.Instance.SetPauseState(true);
		GameManager.Instance.background.SetActive(false);
		Time.timeScale = 1f;
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		pauseMenu.SetActive(false);
	}

	public void BackToMenu()
	{
		AudioManager.Instance.PlayButtonClickSound();
		GameManager.Instance.SetPauseState(true);
		GameManager.Instance.background.SetActive(false);
		Time.timeScale = 0f;
		pauseMenu.SetActive(false);
		SceneManager.LoadScene(MainMenuSceneName);
	}
}
