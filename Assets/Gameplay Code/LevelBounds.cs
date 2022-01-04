using System;
using UnityEngine;

[Flags]
public enum LevelBoundsResult {
	None,
	Left,
	Right
}

public class LevelBounds : MonoBehaviour {
	[SerializeField, Min(0f)] private float _horizontal;
	public float horizontal => _horizontal;
	public float horizontal_2 => _horizontal / 2f;

	[SerializeField, Min(0f)] private float _vertical;
	public float vertical => _vertical;

	public LevelBoundsResult Test(Vector3 position, Bounds bounds) {
		LevelBoundsResult result = LevelBoundsResult.None;
		
		float size_x_2 = bounds.size.x / 2f;
		if (position.x - size_x_2 < -horizontal_2) {
			result |= LevelBoundsResult.Left;
		}
		if (position.x + size_x_2 > horizontal_2) {
			result |= LevelBoundsResult.Right;
		}
		
		return result;
	}

	public Vector3 Clamp(Vector3 position, Bounds bounds) {
		float size_x_2 = bounds.size.x / 2f;
		if (position.x - size_x_2 < -horizontal_2) {
			position.x = -horizontal_2 + size_x_2;
		}
		if (position.x + size_x_2 > horizontal_2) {
			position.x = horizontal_2 - size_x_2;
		}
		return position;
	}

	#if UNITY_EDITOR
	private void OnDrawGizmos() {
		Gizmos.color = Color.yellow;
		// Left line
		Gizmos.DrawLine(
			from: new Vector3(-horizontal_2, 0f, 0f),
			to: new Vector3(-horizontal_2, _vertical, 0f)
		);
		// Right line
		Gizmos.DrawLine(
			from: new Vector3(horizontal_2, 0, 0f),
			to: new Vector3(horizontal_2, _vertical, 0f)
		);
		// Bottom Line
		Gizmos.DrawLine(
			from: new Vector3(-horizontal_2, 0f, 0f),
			to: new Vector3(horizontal_2, 0f, 0f)
		);
		// Top Line
		Gizmos.DrawLine(
			from: new Vector3(-horizontal_2, _vertical, 0f),
			to: new Vector3(horizontal_2, _vertical, 0f)
		);
	}
	#endif
}