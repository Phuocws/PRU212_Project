public class Bee : BaseEnemy
{
	protected override void Awake()
	{
		maxHealth = 50f; // Set the maximum health for the Bee
        base.Awake(); // Call the base class Awake method
	}

	protected override void Update()
    {
        base.Update(); // Call the base class Update method
	}
}
