using UnityEngine;

public class EnemyTracker : MonoBehaviour
{
	public static EnemyTracker Instance { get; private set; }

	private int aliveEnemies = 0;
	private bool allWavesCompleted = false;

	private void Awake()
	{
		if (Instance == null) Instance = this;
		else { Destroy(gameObject); return; }
	}

	public void RegisterEnemy()
	{
		aliveEnemies++;
		Debug.Log("Enemy registered. Total alive: " + aliveEnemies);
	}

	public void UnregisterEnemy()
	{
		aliveEnemies--;
		Debug.Log("Enemy unregistered. Total alive: " + aliveEnemies);
		TryWinCheck();
	}

	public void NotifyWavesCompleted()
	{
		allWavesCompleted = true;
		TryWinCheck();
	}

	private void TryWinCheck()
	{
		if (GameManager.Instance.IsGameOver) return;

		if (allWavesCompleted && aliveEnemies <= 0)
		{
			Debug.Log(aliveEnemies);
			GameManager.Instance.Victory();
		}
	}

	public void ResetTracker()
	{
		aliveEnemies = 0;
		allWavesCompleted = false;
	}
}
