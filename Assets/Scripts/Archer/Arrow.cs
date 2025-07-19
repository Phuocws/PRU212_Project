using UnityEngine;

public class Arrow : MonoBehaviour
{
	private ArrowTierData tierData;
	private BaseEnemy target;

	private Vector2 startPoint;
	private Vector2 targetPoint;

	private float flightDuration;
	private float elapsedTime;
	private float arcHeight;

	private bool canHit = false;

	private BoxCollider2D boxCollider;
	[SerializeField] private SpriteRenderer spriteRenderer;
	[SerializeField] public Sprite[] directionSprites;
	[SerializeField] private GameObject bloodPrefab;

	private void Awake()
	{
		boxCollider = GetComponent<BoxCollider2D>();
		boxCollider.enabled = false; // Disable at start
	}

	private void OnEnable()
	{
		elapsedTime = 0f;
		canHit = false;

		if (boxCollider != null)
			boxCollider.enabled = false;

		// Optional: Reset visuals or disable trail
	}

	private void Update()
	{
		if (target == null || !target.gameObject.activeInHierarchy)
		{
			gameObject.SetActive(false);
			return;
		}

		// Always update to current enemy position
		targetPoint = target.transform.position;

		elapsedTime += Time.deltaTime;
		float t = Mathf.Clamp01(elapsedTime / flightDuration);

		// Enable collision once it's falling
		if (!canHit && t > 0.5f)
		{
			canHit = true;
			boxCollider.enabled = true;
		}

		// Linear + Arc motion
		Vector2 linearPos = Vector2.Lerp(startPoint, targetPoint, t);
		float heightOffset = 4f * arcHeight * t * (1 - t);
		heightOffset = Mathf.Max(heightOffset, 0.1f);
		heightOffset *= 1.5f;
		Vector2 curvedPos = linearPos + Vector2.up * heightOffset;

		// Direction & rotation
		Vector2 moveDir = (curvedPos - (Vector2)transform.position).normalized;
		SetSpriteByDirection(moveDir);
		UpdateColliderSize(moveDir);

		// Move arrow
		transform.position = curvedPos;

		// Hit on landing (failsafe)
		if (t >= 1f)
		{
			if (target.TryGetComponent<BaseEnemy>(out var enemy))
			{
				Vector3 effectOffset = GetEffectOffset();
				if (Random.value <= tierData.accuracy)
				{
					int rawDamage = Random.Range(tierData.minDamage, tierData.maxDamage + 1);
					bool isCrit = Random.value <= tierData.critChance;
					if (isCrit)
					{
						Vector3 critPos = target.transform.position + effectOffset;

						// If attacking from the left, move effect slightly to the left
						if ((target.transform.position - transform.position).x < 0f)
						{
							critPos.x -= 0.3f; // Adjust this value for desired offset
						}
						else
						{
							critPos.x += 0.3f; // Optional: keep symmetry for right side
						}

						GameObject crit = ObjectPool.Instance.SpawnFromPool("CritEffect", critPos, Quaternion.identity);

						// Flip the X scale of the effect if attacking from the left
						Vector3 scale = crit.transform.localScale;
						scale.x = ((target.transform.position - transform.position).x < 0f)
							? -Mathf.Abs(scale.x)
							: Mathf.Abs(scale.x);
						crit.transform.localScale = scale;

						rawDamage = Mathf.RoundToInt(rawDamage * tierData.critMultiplier);
					}
					int finalDamage = Mathf.Max(1, rawDamage - enemy.armor);

					enemy.TakeDamage(finalDamage);

					if (bloodPrefab != null)
					{
						GameObject blood = ObjectPool.Instance.SpawnFromPool("BloodEffect", target.transform.position, Quaternion.identity);
					}
				}
				else
				{
					GameObject miss = ObjectPool.Instance.SpawnFromPool("MissEffect", target.transform.position + effectOffset, Quaternion.identity);
					// Blood effect is NOT spawned on miss
				}
			}
			gameObject.SetActive(false);
		}
	}

	public void Initialize(BaseEnemy target, Vector2 origin, ArrowTierData tierData, float angleOffset = 0f)
	{
		this.tierData = tierData;
		this.startPoint = origin;
		this.target = target;
		this.elapsedTime = 0f;
		this.directionSprites = tierData.directionSprites;

		transform.position = origin;
		this.targetPoint = target != null && target.gameObject.activeInHierarchy
			? target.transform.position
			: origin + Vector2.right * 3f; // fallback

		Vector2 baseDirection = (targetPoint - origin).normalized;

		// Get perpendicular vector for lateral spread
		Vector2 perpendicular = new Vector2(-baseDirection.y, baseDirection.x);

		// Adjust spread based on perpendicular direction
		Vector2 spreadOffset = perpendicular * Mathf.Tan(angleOffset * Mathf.Deg2Rad); // small angle = small lateral shift

		// Apply spread in local direction
		Vector2 adjustedDirection = (baseDirection + spreadOffset).normalized;

		// Simulate forward distance to emphasize spread (not just stop at target)
		float simulatedDistance = Mathf.Max(Vector2.Distance(origin, targetPoint), 5f);
		targetPoint = origin + adjustedDirection * simulatedDistance;

		// Dynamic flightDuration
		float distance = Vector2.Distance(origin, targetPoint);
		flightDuration = Mathf.Clamp(distance / tierData.speed, 0.5f, 1.5f);

		// Arc height
		float baseArc = Mathf.Clamp(distance * 0.35f, 1.5f, 3.5f);
		float enemyHeight = (target != null && target.TryGetComponent(out Collider2D col))
			? Mathf.Clamp(col.bounds.size.y * 0.25f, 0.25f, 1f)
			: 1f;
		float verticalBoost = Mathf.Clamp((targetPoint.y - origin.y) * 0.2f, -0.5f, 1f);
		this.arcHeight = Mathf.Clamp(baseArc + enemyHeight + verticalBoost, 2f, 4f);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!canHit) return;
		if (target == null) return;

		if (collision.gameObject != target.gameObject) return;

		if (!collision.TryGetComponent<BaseEnemy>(out var enemy)) return;

		Vector3 effectOffset = GetEffectOffset();
		if (Random.value <= tierData.accuracy)
		{
			int rawDamage = Random.Range(tierData.minDamage, tierData.maxDamage + 1);
			bool isCrit = Random.value <= tierData.critChance;
			if (isCrit)
			{
				Vector3 critPos = target.transform.position + effectOffset;

				// If attacking from the left, move effect slightly to the left
				if ((target.transform.position - transform.position).x < 0f)
				{
					critPos.x -= 0.3f; // Adjust this value for desired offset
				}
				else
				{
					critPos.x += 0.3f; // Optional: keep symmetry for right side
				}

				GameObject crit = ObjectPool.Instance.SpawnFromPool("CritEffect", critPos, Quaternion.identity);

				// Flip the X scale of the effect if attacking from the left
				Vector3 scale = crit.transform.localScale;
				scale.x = ((target.transform.position - transform.position).x < 0f)
					? -Mathf.Abs(scale.x)
					: Mathf.Abs(scale.x);
				crit.transform.localScale = scale;

				rawDamage = Mathf.RoundToInt(rawDamage * tierData.critMultiplier);
			}
			int finalDamage = Mathf.Max(1, rawDamage - enemy.armor);

			enemy.TakeDamage(finalDamage);

			if (bloodPrefab != null)
			{
				GameObject blood = ObjectPool.Instance.SpawnFromPool("BloodEffect", target.transform.position, Quaternion.identity);
			}
		}
		else
		{
			GameObject miss = ObjectPool.Instance.SpawnFromPool("MissEffect", target.transform.position + effectOffset, Quaternion.identity);
			// Blood effect is NOT spawned on miss
		}

		gameObject.SetActive(false);
	}

	private void SetSpriteByDirection(Vector2 dir)
	{
		if (directionSprites == null || directionSprites.Length == 0) return;

		float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
		angle = (angle + 360f) % 360f;
		angle = (angle + 270f) % 360f;

		bool flipX = false;
		bool flipY = false;
		float baseAngle = angle;

		if (angle <= 90f) flipX = true;
		else if (angle <= 180f) { baseAngle = 180f - angle; flipX = true; flipY = true; }
		else if (angle <= 270f) { baseAngle = angle - 180f; flipY = true; }
		else baseAngle = 360f - angle;

		int index = Mathf.Clamp(Mathf.RoundToInt(baseAngle / (90f / (directionSprites.Length - 1))), 0, directionSprites.Length - 1);
		spriteRenderer.sprite = directionSprites[index];
		spriteRenderer.flipX = flipX;
		spriteRenderer.flipY = flipY;
	}

	private void UpdateColliderSize(Vector2 dir)
	{
		if (boxCollider == null) return;

		float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
		angle = (angle + 360f) % 360f;

		bool isVertical = (angle > 45f && angle < 135f) || (angle > 225f && angle < 315f);
		boxCollider.size = isVertical ? new Vector2(0.1f, 0.5f) : new Vector2(0.5f, 0.1f);
	}

	// Helper method to calculate effect offset based on arrow direction
	private Vector3 GetEffectOffset()
	{
		// Use the direction from arrow to target
		Vector2 direction = (target.transform.position - transform.position).normalized;
		float horizontalOffset = 1f; // Adjust this value for more/less offset
		Vector3 offset = Vector3.up * 0.5f; // Keep the vertical offset

		if (direction.x > 0.1f)
			offset += Vector3.right * horizontalOffset;
		else if (direction.x < -0.1f)
			offset += Vector3.left * horizontalOffset;

		return offset;
	}
}
