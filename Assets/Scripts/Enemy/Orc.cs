public class Orc : BaseEnemy
{
	protected override void Awake()
	{
		maxHealth = 100f;
		base.Awake();
	}

	protected override void Update()
	{
		base.Update();
		if (!GameManager.IsGamePaused)
		{
			AudioManager.Instance.PlayLoop(AudioManager.Instance.orcMoving);
		}
	}

	protected override void Die()
	{
		AudioManager.Instance.PlaySound(AudioManager.Instance.orcDeath);
		base.Die();
	}
}
