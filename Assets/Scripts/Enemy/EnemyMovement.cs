using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
	[SerializeField] private float moveSpeed = 1f;
	private Path path;
	private int currentWaypointIndex = 0;

	private Vector2 pathOffset; // NEW: offset per enemy

	public void SetPathOffset(Vector2 offset)
	{
		pathOffset = offset;
	}

	void Start()
	{
		path = FindFirstObjectByType<Path>();
	}

	void Update()
	{
		if (currentWaypointIndex >= path.WaypointCount) return;

		Transform targetWaypoint = path.GetWaypoint(currentWaypointIndex);
		Vector3 offsetTarget = targetWaypoint.position + (Vector3)pathOffset;

		transform.position = Vector2.MoveTowards(
			transform.position,
			offsetTarget,
			moveSpeed * Time.deltaTime
		);

		if (Vector2.Distance(transform.position, offsetTarget) < 0.1f)
		{
			currentWaypointIndex++;
		}
	}
}
