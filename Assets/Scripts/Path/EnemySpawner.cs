using UnityEngine;

public class EnemySpawner : MonoBehaviour
{

	[SerializeField] private EnemyWave[] waves;
	[SerializeField] private Transform spawnPoint;
	[SerializeField] private float delayBetweenWaves = 5f;

	private int currentWaveIndex = 0;
	private int enemiesSpawned = 0;
	private float spawnTimer = 0f;
	private float waveDelayTimer = 0f;
	private bool spawning = false;
	private bool waitingForNextWave = false;

	[System.Serializable]
	public class EnemyWave
	{
		public string enemyTag; // Match with Pool tag
		public int count;
		public float spawnInterval;
	}

	private void SpawnEnemy()
	{
		Vector3 spawnPos = spawnPoint.position;
		float laneOffset = Random.Range(-0.3f, 0.5f);
		Vector2 pathOffset = new Vector2(0f, laneOffset);

		GameObject enemyObj = ObjectPool.Instance.SpawnFromPool(
			waves[currentWaveIndex].enemyTag,
			spawnPos,
			Quaternion.identity
		);

		if (enemyObj != null)
		{
			BaseEnemy enemy = enemyObj.GetComponent<BaseEnemy>();
			enemy.ResetEnemy(); // Create this method to reset health, animation state, etc.

			EnemyMovement movement = enemyObj.GetComponent<EnemyMovement>();
			if (movement != null)
			{
				movement.SetPathOffset(pathOffset);
				movement.SetSnapToFirstWaypoint(true);
				movement.InitializePosition(); // force it after setting position
			}

			enemiesSpawned++;
		}
	}

	void Start()
	{
		StartWave();
	}

	void Update()
	{
		if (spawning)
		{
			spawnTimer -= Time.deltaTime;

			if (spawnTimer <= 0f && enemiesSpawned < waves[currentWaveIndex].count)
			{
				SpawnEnemy();
				spawnTimer = waves[currentWaveIndex].spawnInterval;
			}

			if (enemiesSpawned >= waves[currentWaveIndex].count)
			{
				spawning = false;
				waitingForNextWave = true;
				waveDelayTimer = delayBetweenWaves;
			}
		}
		else if (waitingForNextWave)
		{
			waveDelayTimer -= Time.deltaTime;
			if (waveDelayTimer <= 0f)
			{
				waitingForNextWave = false;
				AdvanceToNextWave();
			}
		}
	}

	private void StartWave()
	{
		if (currentWaveIndex >= waves.Length) return;

		enemiesSpawned = 0;
		spawnTimer = 0f;
		spawning = true;
	}

	private void AdvanceToNextWave()
	{
		currentWaveIndex++;
		if (currentWaveIndex < waves.Length)
		{
			StartWave();
		}
		else
		{
			//Debug.Log("All waves completed!");
			// Trigger win condition here
		}
	}
}
