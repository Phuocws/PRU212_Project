using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Archer : MonoBehaviour
{
	[Header("Combat Settings")]
	[SerializeField] private LayerMask enemyLayer;
	[SerializeField] private float fireRate;
	[SerializeField] private float range;

	[Header("Arrow Settings")]
	[SerializeField] private Transform arrowSpawnPoint;

	private ArcherAnimationController animController;
	private BaseEnemy currentTarget;
	private float cooldownTimer;
	private bool isPreparingToShoot = false;

	private ArcherTierData archerTierData;
	private ArrowTierData arrowTierData;

	private void Update()
	{
		if (!HasValidTarget())
		{
			TryAcquireNewTarget();
			return;
		}

		cooldownTimer -= Time.deltaTime;

		if (cooldownTimer <= 0f && !isPreparingToShoot)
		{
			PrepareToShoot();
		}
	}

	/// <summary> Initialize the archer after pooling or upgrading. </summary>
	public void Initialize(ArcherTierData archerTier, ArrowTierData arrowTier, float range)
	{
		archerTierData = archerTier;
		arrowTierData = arrowTier;
		fireRate = archerTierData.shootSpeed;
		this.range = range;

		if (animController == null)
			animController = GetComponent<ArcherAnimationController>();

		ResetLogic();
		gameObject.SetActive(true); // Must be active before rebinding

		var animator = GetComponent<Animator>();
		if (animator != null)
		{
			animator.Rebind();
		}

		// Delay one frame before searching for target (let physics settle)
		StartCoroutine(InitializeTargetAfterFrame());
	}

	private IEnumerator InitializeTargetAfterFrame()
	{
		yield return null;

		currentTarget = FindSmartTargetInRange();
		animController.SetTarget(currentTarget);

		if (HasValidTarget())
		{
			PerformAttack();
		}
	}

	/// <summary> Reset state when reused. </summary>
	public void ResetArcher()
	{
		CancelInvoke();
		StopAllCoroutines();
		ResetLogic();
		gameObject.SetActive(false);
	}

	/// <summary> Called by animation event. Instantiates arrow(s). </summary>
	public void FireArrow()
	{
		if (!HasValidTarget()) return;

		cooldownTimer = fireRate;
		List<BaseEnemy> targets = GetValidTargets();
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

			if (arrowGO.TryGetComponent<Arrow>(out var arrow))
			{
				BaseEnemy target = i < targets.Count ? targets[i] : currentTarget;
				arrow.Initialize(target, arrowSpawnPoint.position, arrowTierData, angle);
			}
		}
	}

	private void ResetLogic()
	{
		cooldownTimer = 0f;
		isPreparingToShoot = false;
		currentTarget = null;
		animController.SetTarget(null);
		animController.SetAction(ArcherAction.Idle);
	}

	private void TryAcquireNewTarget()
	{
		currentTarget = FindSmartTargetInRange();
		animController.SetTarget(currentTarget);

		if (!HasValidTarget())
		{
			if (animController.currentAction == ArcherAction.Attack)
				StartCoroutine(TransitionToIdle());
			else
				animController.SetAction(ArcherAction.Idle);

			isPreparingToShoot = false;
		}
	}

	private void PrepareToShoot()
	{
		if (!HasValidTarget())
		{
			StartCoroutine(TransitionToIdle());
			return;
		}

		isPreparingToShoot = true;
		animController.SetTarget(currentTarget);
		animController.SetAction(ArcherAction.PreAttack);
		Invoke(nameof(PerformAttack), 0.1f);
	}

	private void PerformAttack()
	{
		if (!HasValidTarget())
		{
			isPreparingToShoot = false;
			StartCoroutine(TransitionToIdle());
			return;
		}

		isPreparingToShoot = false;
		animController.SetAction(ArcherAction.Attack);
	}

	private BaseEnemy FindSmartTargetInRange()
	{
		var hits = Physics2D.OverlapCircleAll(transform.position, range, enemyLayer);
		BaseEnemy bestTarget = null;
		float bestScore = float.MaxValue;

		foreach (var hit in hits)
		{
			if (hit.TryGetComponent(out BaseEnemy enemy) && IsValidTarget(enemy))
			{
				float score = enemy.currentHealth + Vector2.Distance(transform.position, enemy.transform.position) * 5f;

				if (score < bestScore)
				{
					bestScore = score;
					bestTarget = enemy;
				}
			}
		}
		return bestTarget;
	}

	private List<BaseEnemy> GetValidTargets()
	{
		var hits = Physics2D.OverlapCircleAll(transform.position, range, enemyLayer);
		List<BaseEnemy> valid = new();

		foreach (var hit in hits)
		{
			if (hit.TryGetComponent(out BaseEnemy enemy) && IsValidTarget(enemy))
				valid.Add(enemy);
		}

		valid.Sort((a, b) =>
		{
			float scoreA = a.currentHealth + Vector2.Distance(transform.position, a.transform.position) * 5f;
			float scoreB = b.currentHealth + Vector2.Distance(transform.position, b.transform.position) * 5f;
			return scoreA.CompareTo(scoreB);
		});

		return valid;
	}

	private IEnumerator TransitionToIdle()
	{
		animController.SetAction(ArcherAction.PreAttack);
		yield return new WaitForSeconds(0.1f);
		animController.SetAction(ArcherAction.Idle);
	}

	private bool HasValidTarget() => IsValidTarget(currentTarget) && InRange(currentTarget);

	private bool IsValidTarget(BaseEnemy target) => target != null && target.gameObject.activeInHierarchy && target.currentHealth > 0;

	private bool InRange(BaseEnemy target) => Vector2.Distance(transform.position, target.transform.position) <= range;

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, range);
	}
}
