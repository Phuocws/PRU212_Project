using UnityEngine;
using System.Collections;

public class ArcherAnimationController : MonoBehaviour
{
	[SerializeField] private Animator animator;
	[SerializeField] private SpriteRenderer spriteRenderer;
	[SerializeField] private Transform arrowSpawnPoint;

	[SerializeField] private Vector2 offsetUp;
	[SerializeField] private Vector2 offsetDown;
	[SerializeField] private Vector2 offsetSide;

	public ArcherAction currentAction = ArcherAction.Idle;
	private ArcherDirection currentDirection = ArcherDirection.Down;
	private Transform target;
	private Coroutine idleResetCoroutine;

	public void SetTarget(BaseEnemy newTarget)
	{
		target = newTarget?.transform;
		if (target == null)
		{
			StartIdleReset();
		}
		else
		{
			UpdateDirection();
			CancelIdleReset();
		}
	}

	public void SetAction(ArcherAction action)
	{
		currentAction = action;
		UpdateAnimator();
	}

	private void UpdateDirection()
	{
		if (target == null) return;

		Vector2 dir = (target.position - transform.position).normalized;

		if (Mathf.Abs(dir.y) > Mathf.Abs(dir.x))
		{
			currentDirection = dir.y > 0 ? ArcherDirection.Up : ArcherDirection.Down;
			arrowSpawnPoint.localPosition = dir.y > 0 ? offsetUp : offsetDown;
		}
		else
		{
			currentDirection = ArcherDirection.Side;
			bool facingRight = dir.x > 0;
			spriteRenderer.flipX = facingRight;
			arrowSpawnPoint.localPosition = new Vector2(facingRight ? offsetSide.x : -offsetSide.x, offsetSide.y);
		}
	}

	private void UpdateAnimator()
	{
		animator.SetInteger("Action", (int)currentAction);
		animator.SetFloat("Direction", (float)currentDirection / 100f);
	}

	private void StartIdleReset()
	{
		CancelIdleReset();
		if (isActiveAndEnabled)
			idleResetCoroutine = StartCoroutine(IdleAfterDelay(4f));
	}

	private void CancelIdleReset()
	{
		if (idleResetCoroutine != null)
		{
			StopCoroutine(idleResetCoroutine);
			idleResetCoroutine = null;
		}
	}

	private IEnumerator IdleAfterDelay(float delay)
	{
		yield return new WaitForSeconds(delay);
		SetAction(ArcherAction.Idle);
		currentDirection = ArcherDirection.Down;
		UpdateAnimator();
	}
}
