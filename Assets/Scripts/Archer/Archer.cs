using UnityEngine;

public class Archer : MonoBehaviour
{
	private ArcherAnimationController animController;
	[SerializeField] private float range = 5f;
	[SerializeField] private LayerMask enemyLayer;

	private Transform currentTarget;

	private void Start()
	{
		animController = GetComponent<ArcherAnimationController>();
		InvokeRepeating(nameof(FindTarget), 0f, 0.5f); // Check every 0.5 sec
	}

	private void Update()
	{
		if (currentTarget != null)
		{
			Shoot(); // You can later add cooldown or rate-of-fire
		}
	}

	private void FindTarget()
	{
		Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range, enemyLayer);

		float closestDistance = float.MaxValue;
		Transform closestEnemy = null;

		foreach (var hit in hits)
		{
			float distance = Vector2.Distance(transform.position, hit.transform.position);
			if (distance < closestDistance)
			{
				closestDistance = distance;
				closestEnemy = hit.transform;
			}
		}

		currentTarget = closestEnemy;
	}

	private void Shoot()
	{
		animController.SetTarget(currentTarget);
		animController.SetAction(ArcherAction.PreAttack);
		Invoke(nameof(FinishAttack), 0.2f);
	}

	private void FinishAttack()
	{
		animController.SetAction(ArcherAction.Attack);
		// TODO: Instantiate arrow/projectile here
		Invoke(nameof(ResetToIdle), 0.2f);
	}

	private void ResetToIdle()
	{
		animController.SetAction(ArcherAction.Idle);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, range);
	}
}
