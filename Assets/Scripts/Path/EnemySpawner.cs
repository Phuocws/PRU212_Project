using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
	[System.Serializable]
	public class EnemyWave
	{
		public GameObject enemyPrefab;
		public int count;
		public float spawnInterval;
	}

	[SerializeField] private EnemyWave[] waves;
	[SerializeField] private Transform spawnPoint;
	[SerializeField] private float delayBetweenWaves = 5f;

	private int currentWaveIndex = 0;
	private int enemiesSpawned = 0;
	private float spawnTimer = 0f;
	private float waveDelayTimer = 0f;
	private bool spawning = false;
	private bool waitingForNextWave = false;

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
			Debug.Log("All waves completed!");
			// Trigger win condition here
		}
	}

	private void SpawnEnemy()
	{
		Vector3 spawnPos = spawnPoint.position;

		// Generate lane-style offset (horizontal or vertical)
		float laneOffset = Random.Range(-0.3f, 0.5f); // You can adjust this range
		Vector2 pathOffset = new Vector2(0f, laneOffset); // vertical lanes

		GameObject enemyObj = Instantiate(
			waves[currentWaveIndex].enemyPrefab,
			spawnPos,
			Quaternion.identity
		);

		EnemyMovement movement = enemyObj.GetComponent<EnemyMovement>();
		if (movement != null)
		{
			movement.SetPathOffset(pathOffset);
		}

		enemiesSpawned++;
	}
}
