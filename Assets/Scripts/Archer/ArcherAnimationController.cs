using UnityEngine;

public class ArcherAnimationController : MonoBehaviour
{
	[SerializeField] private Animator animator;
	[SerializeField] private Transform target;
	[SerializeField] private SpriteRenderer spriteRenderer; // << add this

	private ArcherAction currentAction = ArcherAction.Idle;
	private ArcherDirection currentDirection = ArcherDirection.Down;

	public void SetAction(ArcherAction action)
	{
		currentAction = action;
		UpdateAnimator();
	}

	public void SetTarget(Transform newTarget)
	{
		target = newTarget;
		UpdateDirection();
	}

	private void UpdateAnimator()
	{
		animator.SetInteger("Action", (int)currentAction);
		animator.SetInteger("Direction", (int)currentDirection);
	}

	private void UpdateDirection()
	{
		if (target == null) return;

		Vector2 dir = (target.position - transform.position).normalized;

		if (Mathf.Abs(dir.y) > Mathf.Abs(dir.x))
		{
			currentDirection = dir.y > 0 ? ArcherDirection.Up : ArcherDirection.Down;
		}
		else
		{
			currentDirection = ArcherDirection.Side;

			// Flip sprite if needed
			if (spriteRenderer != null)
			{
				spriteRenderer.flipX = dir.x > 0; // facing right? flip
			}
		}

		UpdateAnimator();
	}
}