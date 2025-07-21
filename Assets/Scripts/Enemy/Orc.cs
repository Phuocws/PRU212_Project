using UnityEngine;

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
		// Randomly choose between orcDeath and oof
		AudioSource[] deathSounds = { AudioManager.Instance.orcDeath, AudioManager.Instance.oof };
		int randomIndex = UnityEngine.Random.Range(0, deathSounds.Length);
		AudioManager.Instance.PlaySound(deathSounds[randomIndex]);
		base.Die();
	}
}
