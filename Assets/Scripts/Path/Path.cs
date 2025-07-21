using UnityEngine;

public class Path : MonoBehaviour
{
	[SerializeField] private Transform[] waypoints;

	public Transform GetWaypoint(int index)
	{
		if (index < waypoints.Length)
			return waypoints[index];
		return null;
	}

	public int WaypointCount => waypoints.Length;
}