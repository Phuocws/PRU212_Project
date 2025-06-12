using UnityEngine;

public class Wolf : BaseEnemy
{
	protected override void Awake()
	{
		maxHealth = 150f;
		base.Awake();
	}

	protected override void Update()
	{
		base.Update();
		// Additional Wolf-specific update logic can go here
		base.TakeDamage(Time.deltaTime * 10f); // Example of health decay over time
	}
}
