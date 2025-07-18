using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseEnemy : MonoBehaviour
{
	[Header("Common Settings")]
	[SerializeField] public Sprite avatar;
	public Sprite Avatar => avatar; // Public getter	
	[SerializeField] public string typeName;
	[SerializeField] public int minAttack;
	[SerializeField] public int maxAttack;
	[SerializeField] public int armor;
	[SerializeField] public float maxHealth = 50f;
	[SerializeField] protected Image healthValue;
	[SerializeField] protected GameObject healthBar; // Health bar UI element
	[SerializeField] public int damageAtEndOfPath = 1; // Damage to player base when enemy reaches the end
	[SerializeField] public int coinValue = 10; // Score awarded for defeating this enemy

	public float currentHealth;
	protected Animator animator;
	protected SpriteRenderer spriteRenderer;

	protected Vector2 lastPosition;
	protected Vector2 moveDirection;
	protected Vector2 lastValidMoveDirection = Vector2.down;

	[SerializeField] protected CapsuleCollider2D frontCollider;
	[SerializeField] protected CapsuleCollider2D backCollider;
	[SerializeField] protected CapsuleCollider2D sideCollider;

	private bool isDead = false;

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

		if (currentHealth <= 0 && !isDead)
		{
			Die();
		}
	}

	protected virtual void Die()
	{
		if (isDead) return; 

		Vector2 dir = lastValidMoveDirection;

		animator.SetFloat("MoveX", dir.x);
		animator.SetFloat("MoveY", dir.y);

		if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
			spriteRenderer.flipX = dir.x > 0;

		animator.SetTrigger("Die");
		isDead = true;
		EnemyTracker.Instance.UnregisterEnemy();

		DisableColliders();
		healthBar.SetActive(false);
		GetComponent<EnemyMovement>().enabled = false;

		if (gameObject.activeInHierarchy)
			StartCoroutine(DeactivateAfterDelay());

		GameManager.Instance.AddCoins(coinValue);
	}

	private IEnumerator DeactivateAfterDelay()
	{
		yield return new WaitForSeconds(1f);
		gameObject.SetActive(false);
	}

	public virtual void ResetEnemy()
	{
		gameObject.SetActive(true);
		isDead = false;
		currentHealth = maxHealth;
		UpdateHealthBar();
		EnableColliders();
		healthBar.SetActive(true);
		GetComponent<EnemyMovement>().enabled = true;
		animator.ResetTrigger("Die");
	}

	protected void UpdateHealthBar()
	{
		if (healthValue != null)
			healthValue.fillAmount = currentHealth / maxHealth;
	}
}