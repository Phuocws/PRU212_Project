using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseGame : MonoBehaviour
{
    public GameObject pauseMenu;

    public void pauseGame()
    {
        pauseMenu.SetActive(true);
	}
	public void resumeGame()
    {
		pauseMenu.SetActive(false);
	}
	public void replayGame()
	{
		SceneManager.LoadScene(0);
		pauseMenu.SetActive(false);
	}

	public void backToMenu()
    {
        SceneManager.LoadScene(0);
        pauseMenu.SetActive(false);
	}
}
