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
		// Additional Orc-specific update logic can go here
	}
}
