using UnityEngine;
using System.Collections;

public class ArcherAnimationController : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Animator animator;
	[SerializeField] private SpriteRenderer spriteRenderer;
	[SerializeField] private Transform arrowSpawnPoint;

	[Header("Target & Offset")]
	[SerializeField] private Transform target;
	[SerializeField] private Vector2 offsetUp;
	[SerializeField] private Vector2 offsetDown;
	[SerializeField] private Vector2 offsetSide;

	[Header("State")]
	public ArcherAction currentAction = ArcherAction.Idle;
	private ArcherDirection currentDirection = ArcherDirection.Down;

	private Coroutine idleResetCoroutine;

	#region Public API

	/// <summary>
	/// Set current action (Idle, Attack, etc.) and refresh animator.
	/// </summary>
	public void SetAction(ArcherAction action)
	{
		currentAction = action;
		UpdateDirection();
		UpdateAnimator();
	}

	/// <summary>
	/// Set target for directional aiming and animation.
	/// </summary>
	public void SetTarget(BaseEnemy newTarget)
	{
		if (newTarget == null)
		{
			target = null;
			if (gameObject.activeInHierarchy)
				StartDelayedIdleReset();
			return;
		}

		target = newTarget.transform;
		SetDirectionOnly();
		CancelIdleReset();
	}

	/// <summary>
	/// Only update facing direction without changing animation state.
	/// </summary>
	public void SetDirectionOnly()
	{
		UpdateDirection();
	}

	#endregion

	#region Internal Direction & Animator

	private void UpdateDirection()
	{
		if (target == null) return;

		Vector2 dir = (target.position - transform.position).normalized;

		if (Mathf.Abs(dir.y) > Mathf.Abs(dir.x))
		{
			currentDirection = dir.y > 0 ? ArcherDirection.Up : ArcherDirection.Down;
			if (arrowSpawnPoint != null)
				arrowSpawnPoint.localPosition = dir.y > 0 ? offsetUp : offsetDown;
		}
		else
		{
			currentDirection = ArcherDirection.Side;

			if (spriteRenderer != null)
			{
				bool isFacingRight = dir.x > 0;
				spriteRenderer.flipX = isFacingRight;

				if (arrowSpawnPoint != null)
				{
					arrowSpawnPoint.localPosition = new Vector2(
						isFacingRight ? Mathf.Abs(offsetSide.x) : -Mathf.Abs(offsetSide.x),
						offsetSide.y
					);
				}
			}
		}

		UpdateAnimator();
	}

	private void UpdateAnimator()
	{
		animator.SetInteger("Action", (int)currentAction);
		animator.SetFloat("Direction", (float)currentDirection / 100f);
	}

	#endregion

	#region Idle Fallback

	private void StartDelayedIdleReset()
	{
		CancelIdleReset();
		idleResetCoroutine = StartCoroutine(IdleResetAfterDelay(4f));
	}

	private void CancelIdleReset()
	{
		if (idleResetCoroutine != null)
		{
			StopCoroutine(idleResetCoroutine);
			idleResetCoroutine = null;
		}
	}

	private IEnumerator IdleResetAfterDelay(float delay)
	{
		yield return new WaitForSeconds(delay);

		currentDirection = ArcherDirection.Down;
		currentAction = ArcherAction.Idle;
		UpdateAnimator();

		idleResetCoroutine = null;
	}

	#endregion
}
