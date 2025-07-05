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
		// Additional Wolf-specific update logic can go here
	}
}
