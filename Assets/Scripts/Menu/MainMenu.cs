using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject aboutMenu;

    private const string MapSceneName = "Map";

	public void Start()
	{
		Time.timeScale = 1f;
	}

	public void ShowAboutMenu()
    {
        AudioManager.Instance.PlayButtonClickSound();
		mainMenu.SetActive(false);
        aboutMenu.SetActive(true);
    }

    public void ShowMainMenu()
    {
        AudioManager.Instance.PlayButtonClickSound();
		aboutMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void StartGame()
    {
        AudioManager.Instance.PlayButtonClickSound();
		SceneManager.LoadScene(MapSceneName);
    }

    public void QuitGame()
    {
        AudioManager.Instance.PlayButtonClickSound();
		Application.Quit();
    }
}
