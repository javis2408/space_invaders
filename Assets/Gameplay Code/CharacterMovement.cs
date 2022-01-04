using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

internal enum CharacterMovementState {
    None = -1,
    Accelerating = 0,
    Moving = 1,
    Decelerating = 2,
}

[Flags]
internal enum CharacterMovementKeys {
    None,
    Left,
    Right
}

public class CharacterMovement : MonoBehaviour {
    [SerializeField] private Renderer _renderer;
    
    private InputManager _inputManager;
    private CharacterMovementSettings _settings;

    private LevelBounds _levelBounds;
    
    private Dictionary<CharacterMovementState, AnimationCurve> _curves;
    private Dictionary<CharacterMovementState, float> _durations;

    private CharacterMovementKeys _pressedKeys = CharacterMovementKeys.None;
    private float _direction;
    private float _stateElapsed;
    private float _decelerationFactor = 1f;
    private CharacterMovementState _state = CharacterMovementState.None;
    
    public void Init(InputManager inputManager, CharacterMovementSettings settings, LevelBounds levelBounds) {
        _inputManager = inputManager;
        _settings = settings;
        _levelBounds = levelBounds;

        _inputManager.leftMovement.performed += LeftMovementPerformed;
        _inputManager.leftMovement.canceled += LeftMovementCanceled;
        
        _inputManager.rightMovement.performed += RightMovementPerformed;
        _inputManager.rightMovement.canceled += RightMovementCanceled;

        _curves = new Dictionary<CharacterMovementState, AnimationCurve> {
            {CharacterMovementState.Accelerating, _settings.accelerationCurve},
            {CharacterMovementState.Moving, _settings.movingCurve},
            {CharacterMovementState.Decelerating, _settings.decelerationCurve},
        };

        _durations = new SerializedDictionary<CharacterMovementState, float> {
            {CharacterMovementState.Accelerating, _settings.accelerationTime},
            {CharacterMovementState.Moving, 0f},
            {CharacterMovementState.Decelerating, _settings.decelerationTime},
        };
    }

    private void Update() {
        Move();
    }

    private void LeftMovementPerformed(InputAction.CallbackContext callbackContext) {
        _pressedKeys |= CharacterMovementKeys.Left;
        _direction = -1f;
        _state = CharacterMovementState.Accelerating;
        _stateElapsed = 0f;
    }

    private void LeftMovementCanceled(InputAction.CallbackContext callbackContext) {
        _pressedKeys &= ~CharacterMovementKeys.Left;

        if (_pressedKeys.HasFlag(CharacterMovementKeys.Right))
            _direction = 1f;
    }

    private void RightMovementPerformed(InputAction.CallbackContext callbackContext) {
        _pressedKeys |= CharacterMovementKeys.Right;
        _direction = 1f;
        _state = CharacterMovementState.Accelerating;
        _stateElapsed = 0f;
    }
    
    private void RightMovementCanceled(InputAction.CallbackContext callbackContext) {
        _pressedKeys &= ~CharacterMovementKeys.Right;
        
        if (_pressedKeys.HasFlag(CharacterMovementKeys.Left))
            _direction = -1f;
    }

    private void Move() {
        float elapsed_normalized = _state == CharacterMovementState.None ? 
            0f : 
            math.clamp(_stateElapsed / _durations[_state], 0f, 1f);

        _stateElapsed += Time.deltaTime;
        if (_state == CharacterMovementState.Accelerating && _pressedKeys != CharacterMovementKeys.None) {
            if (_stateElapsed >= _settings.accelerationTime) {
                // We finished accelerating, start moving
                _stateElapsed = 0f;
                _state = CharacterMovementState.Moving;
            }
        }
        else if (_state == CharacterMovementState.Decelerating) {
            if (_stateElapsed >= _settings.decelerationTime) {
                // Debug.Log("finished decelerating");
                // We finished decelerating, stop movement
                _stateElapsed = 0f;
                _state = CharacterMovementState.None;
                _decelerationFactor = 1f;
                _direction = 0f;
                return;
            }
        }
        else if (_pressedKeys == CharacterMovementKeys.None) {
            if (_state == CharacterMovementState.Accelerating) {
                _decelerationFactor = _curves[CharacterMovementState.Accelerating].Evaluate(elapsed_normalized);
                // Debug.Log($"decelerationFactor: {_decelerationFactor}");
                _stateElapsed = 0f;
                _state = CharacterMovementState.Decelerating;
            }
            else if (_state == CharacterMovementState.Moving) {
                _stateElapsed = 0f;
                _state = CharacterMovementState.Decelerating;
            }
        }

        float current_acceleration = _state == CharacterMovementState.None ? 
            0f : 
            _curves[_state].Evaluate(elapsed_normalized);
        
        var bounds_result = _levelBounds.Test(transform.position, _renderer.bounds);
        if (bounds_result == LevelBoundsResult.None ||
            (bounds_result.HasFlag(LevelBoundsResult.Left) && _direction > 0f) ||
            (bounds_result.HasFlag(LevelBoundsResult.Right) && _direction < 0f)
        ) {
            var t = transform;
            t.Translate(
                new Vector3(
                    current_acceleration * _settings.speed * _direction * _decelerationFactor * Time.deltaTime,
                    0f, 
                    0f
                ),
                Space.World
            );
            t.position = _levelBounds.Clamp(t.position, _renderer.bounds);
        }
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos() {
        UnityEditor.Handles.Label(transform.position, $"{_state}");
    }
    #endif
}