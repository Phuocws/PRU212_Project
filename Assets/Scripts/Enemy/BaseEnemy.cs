using UnityEngine;
using UnityEngine.UI;

public abstract class BaseEnemy : MonoBehaviour
{
	[Header("Common Settings")]
	[SerializeField] protected float maxHealth = 100f;
	[SerializeField] protected Image healthBar;

	protected float currentHealth;
	protected Animator animator;
	protected SpriteRenderer spriteRenderer;
	protected Vector2 lastPosition;
	protected Vector2 moveDirection;

	// Optional direction-based colliders
	[SerializeField] protected CapsuleCollider2D frontCollider;
	[SerializeField] protected CapsuleCollider2D backCollider;
	[SerializeField] protected CapsuleCollider2D sideCollider;

	protected virtual void Awake()
	{
		animator = GetComponent<Animator>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		currentHealth = maxHealth;
		UpdateHealthBar();
		lastPosition = transform.position;
	}

	protected virtual void Update()
	{
		HandleDirectionAnimation();
	}

	protected void HandleDirectionAnimation()
	{
		Vector2 currentPosition = transform.position;
		moveDirection = currentPosition - lastPosition;

		if (moveDirection.magnitude != 0.01f)
		{
			Vector2 dir = moveDirection.normalized;
			animator.SetFloat("MoveX", dir.x);
			animator.SetFloat("MoveY", dir.y);

			if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
				spriteRenderer.flipX = dir.x > 0;

			UpdateActiveCollider(dir);
		}

		lastPosition = currentPosition;
	}

	protected void UpdateActiveCollider(Vector2 dir)
	{
		if (frontCollider == null && backCollider == null && sideCollider == null)
			return;

		if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
			EnableOnly(sideCollider);
		else if (dir.y > 0)
			EnableOnly(backCollider);
		else
			EnableOnly(frontCollider);
	}

	private void EnableOnly(Collider2D active)
	{
		if (frontCollider != null) frontCollider.enabled = active == frontCollider;
		if (backCollider != null) backCollider.enabled = active == backCollider;
		if (sideCollider != null) sideCollider.enabled = active == sideCollider;
	}

	public virtual void TakeDamage(float amount)
	{
		currentHealth -= amount;
		currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
		UpdateHealthBar();

		if (currentHealth <= 0)
			Die();
	}

	protected virtual void Die()
	{
		animator.SetFloat("MoveX", moveDirection.x);
		animator.SetFloat("MoveY", moveDirection.y);

		if (Mathf.Abs(moveDirection.x) > Mathf.Abs(moveDirection.y))
			spriteRenderer.flipX = moveDirection.x > 0;

		animator.SetTrigger("Die");

		DisableColliders();

		GetComponent<EnemyMovement>().enabled = false;

		Destroy(gameObject, 2f);
	}

	protected virtual void DisableColliders()
	{
		if (frontCollider != null) frontCollider.enabled = false;
		if (backCollider != null) backCollider.enabled = false;
		if (sideCollider != null) sideCollider.enabled = false;
	}

	protected void UpdateHealthBar()
	{
		if (healthBar != null)
			healthBar.fillAmount = currentHealth / maxHealth;
	}
}
