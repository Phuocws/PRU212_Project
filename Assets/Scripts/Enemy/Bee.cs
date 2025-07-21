using UnityEngine;

public class Bee : BaseEnemy
{
	private AudioSource moveAudioSource;

	protected override void Awake()
	{
		maxHealth = 50f;
		base.Awake();
		// No need to create a new AudioSource here
	}

	protected override void Update()
	{
		base.Update();
		if (!GameManager.IsGamePaused)
		{
			AudioManager.Instance.PlayLoop(AudioManager.Instance.flying);
		}
	}

	protected override void Die()
	{
		AudioManager.Instance.StopLoop(AudioManager.Instance.flying);
		AudioManager.Instance.PlaySound(AudioManager.Instance.beeDeath);
		base.Die();
	}
}
