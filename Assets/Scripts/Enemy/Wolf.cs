public class Wolf : BaseEnemy
{
	protected override void Awake()
	{
		maxHealth = 50f;
		base.Awake();
	}

	protected override void Update()
	{
		base.Update();
		if (!GameManager.IsGamePaused)
		{
			AudioManager.Instance.PlayLoop(AudioManager.Instance.wolfMoving);
		}
	}

	protected override void Die()
	{
		// Play Wolf death sound
		AudioManager.Instance.PlaySound(AudioManager.Instance.wolfDeath);
		base.Die();
	}
}
