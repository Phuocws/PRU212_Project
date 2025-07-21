using UnityEngine;

public class Slime : BaseEnemy
{
	[Header("Slime Settings")]
	public SlimeType slimeType;

	[SerializeField] private float specialSlimeSpawnChance = 0.3f;
	[SerializeField] private string specialSlimeTag = "SpecialSlime";
	[SerializeField] private string cloneSlimeTag = "CloneSlime";

	private int splitWaypointIndex;
	private bool stopSplit = false;

	protected override void Awake()
	{
		maxHealth = slimeType == SlimeType.Clone ? 30f : 50f;
		base.Awake();

		if (slimeType == SlimeType.Special)
			GetComponent<EnemyMovement>().enabled = false;
	}

	protected override void Update()
	{
		base.Update();
		if (!GameManager.IsGamePaused)
		{
			AudioManager.Instance.PlayLoop(AudioManager.Instance.slimeMoving);
		}
	}

	public override void TakeDamage(float amount)
	{
		base.TakeDamage(amount);
	}

	protected override void Die()
	{
		if (GameUIManager.Instance != null)
			GameUIManager.Instance.HideEnemyInfoIfSelected(this);

		if (slimeType == SlimeType.Special)
		{
			stopSplit = true;
			AudioManager.Instance.PlaySound(AudioManager.Instance.slimeDeath);
			base.Die(); 
			return;
		}

		if (slimeType == SlimeType.Clone)
		{
			AudioManager.Instance.PlaySound(AudioManager.Instance.slimeDeath);
			base.Die(); 
			return;
		}

		if (slimeType == SlimeType.Normal && Random.value < specialSlimeSpawnChance)
		{
			Vector3 spawnPos = transform.position;
			Vector2 direction = moveDirection;

			int pathIndex = 0;
			if (TryGetComponent<EnemyMovement>(out var currentMovement))
				pathIndex = currentMovement.GetCurrentWaypointIndex();

			gameObject.SetActive(false);

			GameObject special = ObjectPool.Instance.SpawnFromPool(specialSlimeTag, spawnPos, Quaternion.identity);
			if (special != null && special.TryGetComponent<Slime>(out var specialSlime))
			{
				specialSlime.ResetEnemy();
				specialSlime.slimeType = SlimeType.Special;
				EnemyTracker.Instance.RegisterEnemy();
				specialSlime.StartSplit(direction, pathIndex);
			}

			AudioManager.Instance.PlaySound(AudioManager.Instance.slimeDeath);
			base.Die();
		}
		else
		{
			AudioManager.Instance.PlaySound(AudioManager.Instance.slimeDeath);
			base.Die();
		}
	}

	public void StartSplit(Vector2 inheritedDirection, int pathIndex)
	{
		moveDirection = inheritedDirection;
		splitWaypointIndex = pathIndex;
		stopSplit = false;

		animator.SetFloat("MoveX", moveDirection.x);
		animator.SetFloat("MoveY", moveDirection.y);
		animator.SetTrigger("Split");
	}

	public void OnSplitAnimationComplete()
	{
		if (stopSplit)
		{
			return;
		}

		SpawnClone(transform.position + new Vector3(-0.3f, 0), -0.15f);
		SpawnClone(transform.position + new Vector3(0.3f, 0), 0.15f);

		EnemyTracker.Instance.UnregisterEnemy();
		gameObject.SetActive(false);
		GameUIManager.Instance?.HideEnemyInfoIfSelected(this);
	}

	private void SpawnClone(Vector3 pos, float yOffset)
	{
		GameObject clone = ObjectPool.Instance.SpawnFromPool(cloneSlimeTag, pos, Quaternion.identity);
		if (clone != null && clone.TryGetComponent<Slime>(out var slime))
		{
			slime.slimeType = SlimeType.Clone;
			slime.ResetEnemy();
			EnemyTracker.Instance.RegisterEnemy();
			float healthPercent = currentHealth / maxHealth;
			slime.currentHealth = slime.maxHealth * healthPercent;
			slime.UpdateHealthBar();

			if (clone.TryGetComponent<EnemyMovement>(out var movement))
			{
				movement.SetSnapToFirstWaypoint(false);
				movement.SetPathOffset(new Vector2(0f, yOffset), false);
				movement.InitializePosition();
				movement.SetWaypointIndex(splitWaypointIndex);
			}
		}
	}

	public override void ResetEnemy()
	{
		base.ResetEnemy();
		stopSplit = false;
		GetComponent<EnemyMovement>().enabled = (slimeType != SlimeType.Special);
		EnableColliders(); 
	}
}