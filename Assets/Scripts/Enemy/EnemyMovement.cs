using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
	[SerializeField] private float moveSpeed = 0.2f;
	private Path path;
	private int currentWaypointIndex = 0;
	private Vector2 pathOffset;
	private bool snapToFirstWaypoint = true;
	private bool overrideStartPosition = true;
	private bool initialized = false;

	public int GetCurrentWaypointIndex() => currentWaypointIndex;

	public void SetPathOffset(Vector2 offset, bool overrideStart = true)
	{
		pathOffset = offset;
		overrideStartPosition = overrideStart;
	}

	public void SetSnapToFirstWaypoint(bool value) => snapToFirstWaypoint = value;

	public void InitializePosition()
	{
		if (path == null)
			path = FindFirstObjectByType<Path>();

		currentWaypointIndex = 0;
		initialized = true;

		// Apply only if snapToFirstWaypoint is true
		if (path != null && path.WaypointCount > 0 && overrideStartPosition && snapToFirstWaypoint)
		{
			transform.position = path.GetWaypoint(0).position + (Vector3)pathOffset;
		}
	}

	public void SetWaypointIndex(int index)
	{
		currentWaypointIndex = index;
	}

	public void SetWaypointIndexFromClosestPoint()
	{
		if (path == null || path.WaypointCount == 0)
			return;

		float closestDist = float.MaxValue;
		int closestIndex = 0;

		for (int i = 0; i < path.WaypointCount; i++)
		{
			float dist = Vector2.Distance(
				(Vector2)transform.position,
				(Vector2)path.GetWaypoint(i).position + pathOffset
			);

			if (dist < closestDist)
			{
				closestDist = dist;
				closestIndex = i;
			}
		}

		currentWaypointIndex = closestIndex;
	}


	private void Update()
	{
		if (!initialized || path == null || currentWaypointIndex >= path.WaypointCount)
			return;

		Transform targetWaypoint = path.GetWaypoint(currentWaypointIndex);
		Vector3 offsetTarget = targetWaypoint.position + (Vector3)pathOffset;

		transform.position = Vector2.MoveTowards(transform.position, offsetTarget, moveSpeed * Time.deltaTime);

		if (Vector2.Distance(transform.position, offsetTarget) < 0.1f)
		{
			currentWaypointIndex++;
		}
	}
}
