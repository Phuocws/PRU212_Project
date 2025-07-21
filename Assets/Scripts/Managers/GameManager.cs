using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance { get; private set; }

	[Header("Game Settings")]
	[SerializeField] private int startingHearts = 20;
	[SerializeField] private int startingCoins = 100;

	[Header("Game Over Panels")]
	[SerializeField] public GameObject background;
	[SerializeField] private GameObject winPanel;
	[SerializeField] private GameObject losePanel;

	private bool isGameOver = false;
	public bool IsGameOver => isGameOver;

	public static bool IsGamePaused { get; private set; }


	public int CurrentHearts { get; private set; }
	public int CurrentCoints { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Destroy(gameObject);
		}
	}

	private void Start()
	{
		InitGame();
	}

	private void InitGame()
	{
		Time.timeScale = 1f; 
		CurrentHearts = startingHearts;
		CurrentCoints = startingCoins;

		isGameOver = false;

		GameUIManager.Instance.SetHearts(CurrentHearts);
		GameUIManager.Instance.SetCoins(CurrentCoints);

		// Hide panels at game start
		if (background != null) background.SetActive(false);
		if (winPanel != null) winPanel.SetActive(false);
		if (losePanel != null) losePanel.SetActive(false);
	}

	public void AddCoins(int amount)
	{
		CurrentCoints += amount;
		GameUIManager.Instance.SetCoins(CurrentCoints);
	}

	public bool SpendCoins(int amount)
	{
		if (CurrentCoints < amount) return false;

		CurrentCoints -= amount;
		GameUIManager.Instance.SetCoins(CurrentCoints);
		return true;
	}

	public void TakeDamage(int amount)
	{
		if (isGameOver) return;

		CurrentHearts -= amount;
		GameUIManager.Instance.SetHearts(CurrentHearts);

		if (CurrentHearts <= 0)
		{
			AudioManager.Instance.PlaySound(AudioManager.Instance.defeat);	
			DefeatDelayed(1);
		}
	}

	public void Defeat()
	{
		if (isGameOver) return;

		isGameOver = true;
		if (losePanel != null)
		{
			if (background != null) background.SetActive(true);
			losePanel.SetActive(true);
			Time.timeScale = 0f; 
		}
	}

	public void Victory()
	{
		if (isGameOver) return;
					
		isGameOver = true;
		if (winPanel != null)
		{
			Time.timeScale = 0f;
			if (background != null) background.SetActive(true);
			winPanel.SetActive(true);
		}
	}

	public void VictoryDelayed(float delaySeconds)
	{
		StartCoroutine(VictoryDelayCoroutine(delaySeconds));
	}

	private IEnumerator VictoryDelayCoroutine(float delaySeconds)
	{
		yield return new WaitForSeconds(delaySeconds);
		Victory();
	}

	public void DefeatDelayed(float delaySeconds)
	{
		var allEnemies = FindObjectsByType<EnemyMovement>(FindObjectsSortMode.None);
		foreach (var enemy in allEnemies)
		{
			enemy.enabled = false; 
		}

		StartCoroutine(DefeatDelayCoroutine(delaySeconds));
	}

	private IEnumerator DefeatDelayCoroutine(float delaySeconds)
	{
		yield return new WaitForSeconds(delaySeconds);
		Defeat();
	}
	public void SetPauseState(bool isPaused)
	{
		IsGamePaused = isPaused;
	}
}
