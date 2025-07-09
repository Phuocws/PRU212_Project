using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
	public static EnemySpawner Instance { get; private set; }

	[SerializeField] private EnemyWave[] waves;
	[SerializeField] private Transform spawnPoint;
	[SerializeField] private float delayBetweenWaves = 5f;
	[SerializeField] private UIManager uiManager;
	[SerializeField] private GameObject startWaveButton;

	private int currentWaveIndex = 0;
	private float spawnTimer = 0f;

	private int enemyTypeIndex = 0; // current enemy type
	private int spawnedOfCurrentType = 0;

	private bool isSpawning = false;
	private bool isWaitingForNextWave = false;
	private bool isWaitingForPlayerStart = true;

	[System.Serializable]
	public class WaveEnemyInfo
	{
		public string enemyTag;
		public int count;
	}

	[System.Serializable]
	public class EnemyWave
	{
		public List<WaveEnemyInfo> enemies; // list of enemy types per wave
		public float spawnInterval;

		public int TotalEnemies => enemies != null ? enemies.Sum(e => e.count) : 0;

		public string GetWaveSummary()
		{
			if (enemies == null || enemies.Count == 0) return "No enemies";

			return string.Join("\n", enemies.Select(e => $"{e.enemyTag} x {e.count}"));
		}
	}

	private void Awake()
	{
		if (Instance == null) Instance = this;
		else { Destroy(gameObject); return; }
	}

	private void Start()
	{
		if (startWaveButton != null)
		{
			startWaveButton.SetActive(true);
			EnemyTracker.Instance.ResetTracker(); // Reset enemy tracker
			uiManager.StartFirstWaveButton();
		}
	}

	private void Update()
	{
		if (isWaitingForPlayerStart || IsAllWavesComplete()) return;

		if (isSpawning)
		{
			spawnTimer -= Time.deltaTime;

			if (spawnTimer <= 0f)
			{
				bool spawned = TrySpawnNextEnemy();
				if (spawned)
					spawnTimer = GetCurrentWave().spawnInterval;
				else
					FinishCurrentWave();
			}
		}
	}

	private bool TrySpawnNextEnemy()
	{
		var wave = GetCurrentWave();

		if (wave == null || wave.enemies == null || wave.enemies.Count == 0)
			return false;

		if (enemyTypeIndex >= wave.enemies.Count)
			return false;

		var currentType = wave.enemies[enemyTypeIndex];

		// If finished this enemy type, move to next
		if (spawnedOfCurrentType >= currentType.count)
		{
			enemyTypeIndex++;
			spawnedOfCurrentType = 0;

			if (enemyTypeIndex >= wave.enemies.Count)
				return false;

			currentType = wave.enemies[enemyTypeIndex];
		}

		SpawnEnemy(currentType.enemyTag);
		spawnedOfCurrentType++;
		return true;
	}

	private void SpawnEnemy(string enemyTag)
	{
		Vector3 spawnPos = spawnPoint.position;
		Vector2 offset = new Vector2(0f, Random.Range(-0.3f, 0.5f));

		GameObject enemyObj = ObjectPool.Instance.SpawnFromPool(
			enemyTag,
			spawnPos,
			Quaternion.identity
		);

		if (enemyObj != null)
		{
			BaseEnemy enemy = enemyObj.GetComponent<BaseEnemy>();
			enemy.ResetEnemy();

			var movement = enemyObj.GetComponent<EnemyMovement>();
			if (movement != null)
			{
				movement.SetPathOffset(offset);
				movement.SetSnapToFirstWaypoint(true);
				movement.InitializePosition();
			}

			EnemyTracker.Instance.RegisterEnemy();
		}
	}

	public EnemyWave GetCurrentWave()
	{
		return waves[Mathf.Clamp(currentWaveIndex, 0, waves.Length - 1)];
	}

	private void StartWave()
	{
		if (IsAllWavesComplete()) return;

		spawnTimer = 0f;
		isSpawning = true;

		enemyTypeIndex = 0;
		spawnedOfCurrentType = 0;

		uiManager.SetWaves(currentWaveIndex + 1, waves.Length);
	}

	public bool IsAllWavesComplete()
	{
		bool isLastWave = currentWaveIndex == waves.Length - 1;
		bool noSpawning = !isSpawning;
		bool noWaiting = !isWaitingForNextWave && !isWaitingForPlayerStart;

		bool allSpawned = enemyTypeIndex >= GetCurrentWave().enemies.Count;

		return isLastWave && allSpawned && noSpawning && noWaiting;
	}

	private void FinishCurrentWave()
	{
		isSpawning = false;

		if (currentWaveIndex < waves.Length - 1)
		{
			isWaitingForNextWave = true;
			currentWaveIndex++;

			uiManager.StartCountdown(delayBetweenWaves, true);
		}
		else
		{
			isWaitingForNextWave = false;
			EnemyTracker.Instance.NotifyWavesCompleted();
		}
	}

	public void OnStartWaveClicked()
	{
		if (IsAllWavesComplete()) return;

		startWaveButton.SetActive(false);
		uiManager.ForceStopCountdown();

		isWaitingForPlayerStart = false;
		StartWave();
	}
}
