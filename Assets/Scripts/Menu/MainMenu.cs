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
		mainMenu.SetActive(false);
        aboutMenu.SetActive(true);
    }

    public void ShowMainMenu()
    {
		aboutMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void StartGame()
    {
		SceneManager.LoadScene(MapSceneName);
    }

    public void QuitGame()
    {
		Application.Quit();
    }
}
