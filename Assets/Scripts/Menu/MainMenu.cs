using UnityEngine;
using UnityEngine.SceneManagement;

public class Mainenu : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject aboutMenu;
	

    public void aboutMenuFunction()
    {
        aboutMenu.SetActive(true);
	}
    public void mainMenuFunction()
    {
        aboutMenu.SetActive(false);
        mainMenu.SetActive(true);
	}

    public void startGame()
    {
        SceneManager.LoadScene(1);
	}

    public void quitGame()
    {
        Application.Quit();
	}
}
