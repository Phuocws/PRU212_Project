using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Archer : MonoBehaviour
{
	private enum ArcherState
	{
		Idle = 1,
		PreAttack = 2,
		Attack = 3
	}

	[Header("Combat Settings")]
	[SerializeField] private LayerMask enemyLayer;
	[SerializeField] private float fireRate;

	[Header("Arrow Settings")]
	[SerializeField] private Transform arrowSpawnPoint;

	private ArcherState state = ArcherState.Idle;
	private ArcherAnimationController animController;
	private ArcherTierData archerTierData;
	private ArrowTierData arrowTierData;
	private BaseEnemy currentTarget;

	private float cooldownTimer;
	private bool isPreparingToShoot;

	private Transform towerCenter;
	private float towerRange;

	private void Update()
	{
		cooldownTimer -= Time.deltaTime;

		switch (state)
		{
			case ArcherState.Idle:
				if (SearchForTarget())
					TransitionToPreAttack();
				break;

			case ArcherState.PreAttack:
				// Wait for animation to trigger PerformAttack via Invoke
				break;

			case ArcherState.Attack:
				if (!HasValidTarget())
				{
					TransitionToIdle();
				}
				else if (cooldownTimer <= 0f && !isPreparingToShoot)
				{
					PrepareToShoot(); // Next volley
				}
				break;
		}
	}

	public void Initialize(ArcherTierData archerTier, ArrowTierData arrowTier, float range, Transform center)
	{
		archerTierData = archerTier;
		arrowTierData = arrowTier;
		towerRange = range;
		towerCenter = center;

		fireRate = archerTierData.fireRate;

		if (animController == null)
			animController = GetComponent<ArcherAnimationController>();

		gameObject.SetActive(true);
		ResetLogic();

		GetComponent<Animator>().Rebind();

		StartCoroutine(DelayedSearch());
	}

	private IEnumerator DelayedSearch()
	{
		yield return null;

		if (SearchForTarget())
			TransitionToPreAttack();
	}

	private bool SearchForTarget()
	{
		currentTarget = FindSmartTarget();

		if (HasValidTarget())
		{
			animController.SetTarget(currentTarget);
			return true;
		}
		return false;
	}

	private BaseEnemy FindSmartTarget()
	{
		var hits = Physics2D.OverlapCircleAll(towerCenter.position, towerRange, enemyLayer);
		BaseEnemy best = null;
		float bestScore = float.MaxValue;

		foreach (var hit in hits)
		{
			if (!hit.TryGetComponent(out BaseEnemy enemy)) continue;
			if (!IsValidTarget(enemy) || !IsInTowerRange(enemy)) continue;

			float score = enemy.currentHealth + Vector2.Distance(towerCenter.position, enemy.transform.position) * 5f;
			if (score < bestScore)
			{
				bestScore = score;
				best = enemy;
			}
		}
		return best;
	}

	private bool IsInTowerRange(BaseEnemy enemy)
	{
		if (enemy == null) return false;

		if (enemy.TryGetComponent<Collider2D>(out var col))
		{
			Vector2 closest = col.ClosestPoint(towerCenter.position);
			return Vector2.Distance(towerCenter.position, closest) <= towerRange * 0.95f;
		}
		return Vector2.Distance(towerCenter.position, enemy.transform.position) <= towerRange * 0.95f;
	}

	private void PrepareToShoot()
	{
		if (!HasValidTarget())
		{
			TransitionToIdle();
			return;
		}

		isPreparingToShoot = true;
		animController.SetTarget(currentTarget);
		animController.SetAction(ArcherAction.PreAttack);

		Invoke(nameof(PerformAttack), 0.3f); // Match animation pre-delay
	}

	private void PerformAttack()
	{
		if (!HasValidTarget())
		{
			TransitionToIdle();
			return;
		}

		TransitionToAttack();
	}

	private void TransitionToPreAttack()
	{
		state = ArcherState.PreAttack;
		isPreparingToShoot = true;

		animController.SetTarget(currentTarget);
		animController.SetAction(ArcherAction.PreAttack);

		Invoke(nameof(PerformAttack), 0.3f); // You can tweak based on your animation length
	}

	private void TransitionToAttack()
	{
		state = ArcherState.Attack;
		isPreparingToShoot = false;

		animController.SetAction(ArcherAction.Attack);
		cooldownTimer = fireRate;
	}

	private void TransitionToIdle()
	{
		state = ArcherState.Idle;
		currentTarget = null;
		isPreparingToShoot = false;
		cooldownTimer = fireRate;

		animController.SetTarget(null);
		animController.SetAction(ArcherAction.Idle);
	}

	// Called via animation event
	private void FireArrow()
	{
		var targets = GetValidTargets();
		int arrowCount = archerTierData.arrowsPerShoot;
		float spread = 40f;

		for (int i = 0; i < arrowCount; i++)
		{
			float angle = arrowCount > 1 ? Mathf.Lerp(-spread / 2f, spread / 2f, (float)i / (arrowCount - 1)) : 0f;

			GameObject arrowGO = ObjectPool.Instance.SpawnFromPool(
				arrowTierData.arrowPrefab.name,
				arrowSpawnPoint.position,
				Quaternion.identity
			);

			if (arrowGO.TryGetComponent(out Arrow arrow))
			{
				BaseEnemy target = i < targets.Count ? targets[i] : currentTarget;
				arrow.Initialize(target, arrowSpawnPoint.position, arrowTierData, angle);
			}
		}
	}

	private List<BaseEnemy> GetValidTargets()
	{
		var hits = Physics2D.OverlapCircleAll(towerCenter.position, towerRange, enemyLayer);
		List<BaseEnemy> valid = new();

		foreach (var hit in hits)
		{
			if (hit.TryGetComponent(out BaseEnemy enemy) && IsValidTarget(enemy))
				valid.Add(enemy);
		}

		valid.Sort((a, b) =>
		{
			float aScore = a.currentHealth + Vector2.Distance(towerCenter.position, a.transform.position) * 5f;
			float bScore = b.currentHealth + Vector2.Distance(towerCenter.position, b.transform.position) * 5f;
			return aScore.CompareTo(bScore);
		});

		return valid;
	}

	private bool IsValidTarget(BaseEnemy target)
	{
		return target != null && target.gameObject.activeInHierarchy && target.currentHealth > 0;
	}

	private bool HasValidTarget()
	{
		return IsValidTarget(currentTarget) && IsInTowerRange(currentTarget);
	}

	private void ResetLogic()
	{
		state = ArcherState.Idle;
		cooldownTimer = 0f;
		isPreparingToShoot = false;
		currentTarget = null;

		animController.SetTarget(null);
		animController.SetAction(ArcherAction.Idle);
	}

	public void ResetArcher()
	{
		CancelInvoke();
		StopAllCoroutines();
		ResetLogic();
		gameObject.SetActive(false);
	}
}
