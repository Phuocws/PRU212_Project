using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseEnemy : MonoBehaviour
{
	[Header("Common Settings")]
	[SerializeField] protected float maxHealth = 50f;
	[SerializeField] protected Image healthBar;

	public float currentHealth;
	protected Animator animator;
	protected SpriteRenderer spriteRenderer;

	protected Vector2 lastPosition;
	protected Vector2 moveDirection;
	protected Vector2 lastValidMoveDirection = Vector2.down;

	[SerializeField] protected CapsuleCollider2D frontCollider;
	[SerializeField] protected CapsuleCollider2D backCollider;
	[SerializeField] protected CapsuleCollider2D sideCollider;

	public float SpawnTime { get; private set; }

	protected virtual void Awake()
	{
		animator = GetComponent<Animator>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		currentHealth = maxHealth;
		lastPosition = transform.position;

		UpdateHealthBar();
	}

	private void Start()
	{
		SpawnTime = Time.time;
	}

	protected virtual void Update()
	{
		HandleDirectionAnimation();
	}

	protected void HandleDirectionAnimation()
	{
		Vector2 currentPosition = transform.position;
		moveDirection = currentPosition - lastPosition;

		if (moveDirection.sqrMagnitude > 0f)
		{
			Vector2 dir = moveDirection.normalized;
			lastValidMoveDirection = dir;

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
		else
			EnableOnly(dir.y > 0 ? backCollider : frontCollider);
	}

	private void EnableOnly(Collider2D active)
	{
		if (frontCollider != null) frontCollider.enabled = active == frontCollider;
		if (backCollider != null) backCollider.enabled = active == backCollider;
		if (sideCollider != null) sideCollider.enabled = active == sideCollider;
	}

	protected void EnableColliders()
	{
		if (frontCollider != null) frontCollider.enabled = true;
		if (backCollider != null) backCollider.enabled = true;
		if (sideCollider != null) sideCollider.enabled = true;
	}

	protected virtual void DisableColliders()
	{
		if (frontCollider != null) frontCollider.enabled = false;
		if (backCollider != null) backCollider.enabled = false;
		if (sideCollider != null) sideCollider.enabled = false;
	}

	public virtual void TakeDamage(float amount)
	{
		currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
		UpdateHealthBar();

		if (currentHealth <= 0)
			Die();
	}

	protected virtual void Die()
	{
		Vector2 dir = lastValidMoveDirection;

		animator.SetFloat("MoveX", dir.x);
		animator.SetFloat("MoveY", dir.y);

		if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
			spriteRenderer.flipX = dir.x > 0;

		animator.SetTrigger("Die");

		DisableColliders();
		GetComponent<EnemyMovement>().enabled = false;

		StartCoroutine(DeactivateAfterDelay());
	}

	private IEnumerator DeactivateAfterDelay()
	{
		yield return new WaitForSeconds(1f);
		gameObject.SetActive(false);
	}

	public virtual void ResetEnemy()
	{
		gameObject.SetActive(true);
		currentHealth = maxHealth;
		UpdateHealthBar();
		EnableColliders();

		GetComponent<EnemyMovement>().enabled = true;
		animator.ResetTrigger("Die");
	}

	protected void UpdateHealthBar()
	{
		if (healthBar != null)
			healthBar.fillAmount = currentHealth / maxHealth;
	}
}
