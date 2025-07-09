using UnityEngine;

public class Slime : BaseEnemy
{
	[Header("Slime Settings")]
	public SlimeType slimeType;

	[SerializeField] private float specialSlimeSpawnChance = 0.3f;
	[SerializeField] private string specialSlimeTag = "SpecialSlime";
	[SerializeField] private string cloneSlimeTag = "CloneSlime";

	private int splitWaypointIndex;
	private bool hasSplit = false;
	private bool isSplitting = false;

	protected override void Awake()
	{
		maxHealth = slimeType == SlimeType.Clone ? 30f : 50f;
		base.Awake();

		if (slimeType == SlimeType.Special)
			GetComponent<EnemyMovement>().enabled = false;
	}

	public override void TakeDamage(float amount)
	{
		base.TakeDamage(amount);
	}

	protected override void Die()
	{
		if (slimeType == SlimeType.Special && isSplitting && !hasSplit)
		{
			// Don't spawn clones
			isSplitting = false;
			hasSplit = true; // Mark it to prevent double trigger
		}


		if (slimeType != SlimeType.Normal)
		{
			base.Die();
			return;
		}

		if (Random.value < specialSlimeSpawnChance)
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
				specialSlime.StartSplit(direction, pathIndex);
			}
		}
		else
		{
			base.Die();
		}
	}

	public void StartSplit(Vector2 inheritedDirection, int pathIndex)
	{
		moveDirection = inheritedDirection;
		splitWaypointIndex = pathIndex;
		hasSplit = false;
		isSplitting = true;

		animator.SetFloat("MoveX", moveDirection.x);
		animator.SetFloat("MoveY", moveDirection.y);
		animator.SetTrigger("Split");

		DisableColliders();
	}

	// Animation event should call this at the END of the Split animation
	public void OnSplitAnimationComplete()
	{
		//if (hasSplit) return;
		if (hasSplit || currentHealth <= 0f)
		{
			isSplitting = false;
			return; // Already dead or split done
		}

		hasSplit = true;
		isSplitting = false;

		SpawnClone(transform.position + new Vector3(-0.3f, 0), -0.15f);
		SpawnClone(transform.position + new Vector3(0.3f, 0), 0.15f);

		gameObject.SetActive(false); // Disable AFTER clone spawn
	}

	private void SpawnClone(Vector3 pos, float yOffset)
	{
		GameObject clone = ObjectPool.Instance.SpawnFromPool(cloneSlimeTag, pos, Quaternion.identity);
		if (clone != null && clone.TryGetComponent<Slime>(out var slime))
		{
			slime.slimeType = SlimeType.Clone;
			slime.ResetEnemy();
			float healthPercent = currentHealth / maxHealth;
			slime.currentHealth = slime.maxHealth * healthPercent;
			slime.UpdateHealthBar();

			if (clone.TryGetComponent<EnemyMovement>(out var movement))
			{
				movement.SetSnapToFirstWaypoint(false);
				movement.SetPathOffset(new Vector2(0f, yOffset), false);
				movement.InitializePosition(); // Keeps current position
				movement.SetWaypointIndex(splitWaypointIndex);
			}
		}
	}

	public override void ResetEnemy()
	{
		base.ResetEnemy();
		isSplitting = false;
		hasSplit = false;
		GetComponent<EnemyMovement>().enabled = (slimeType != SlimeType.Special);
	}
}