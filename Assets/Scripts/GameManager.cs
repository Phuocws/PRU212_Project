using UnityEngine;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance { get; private set; }

	[Header("Game Settings")]
	[SerializeField] private int startingHearts = 20;
	[SerializeField] private int startingCoins = 100;

	private bool isGameOver = false;
	public bool IsGameOver => isGameOver;

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
		DontDestroyOnLoad(gameObject);
	}

	private void Start()
	{
		InitGame();
	}

	private void InitGame()
	{
		CurrentHearts = startingHearts;
		CurrentCoints = startingCoins;

		isGameOver = false;

		GameUIManager.Instance.SetHearts(CurrentHearts);
		GameUIManager.Instance.SetCoins(CurrentCoints);
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
			Defeat();
		}
	}

	public void Defeat()
	{
		if (isGameOver) return;

		isGameOver = true;
		Debug.Log("[GameManager] GAME OVER - YOU LOST");
		// TODO: Show defeat UI
	}

	public void Victory()
	{
		if (isGameOver) return;

		isGameOver = true;
		Debug.Log("[GameManager] GAME OVER - YOU WON");
		// TODO: Show win UI
	}
}
