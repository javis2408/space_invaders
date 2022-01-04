using UnityEngine;

[CreateAssetMenu(fileName = "Character Movement Settings", menuName = "Settings/Character Movement")]
public class CharacterMovementSettings : ScriptableObject {
	[SerializeField] private float _speed = 1f;
	public float speed => _speed;

	[Header("Curves")]
	[SerializeField] private AnimationCurve _accelerationCurve;
	public AnimationCurve accelerationCurve => _accelerationCurve;

	[SerializeField] private AnimationCurve _movingCurve;
	public AnimationCurve movingCurve => _movingCurve;
	
	[SerializeField] private AnimationCurve _decelerationCurve;
	public AnimationCurve decelerationCurve => _decelerationCurve;

	[Header("Durations")]
	[SerializeField] private float _accelerationTime = 0.5f;
	public float accelerationTime => _accelerationTime;
	
	[SerializeField] private float _decelerationTime = 0.25f;
	public float decelerationTime => _decelerationTime;
}