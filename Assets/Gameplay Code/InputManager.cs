using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager {
	public readonly InputAction leftMovement;
	public readonly InputAction rightMovement;
	public readonly InputAction shoot;

	public InputManager() {
		leftMovement = new InputAction("left_movement", binding: "keyboard/leftArrow");
		rightMovement = new InputAction("right_movement", binding: "keyboard/rightArrow");
		shoot = new InputAction("fire", binding: "keyboard/upArrow");
		
		leftMovement.Enable();
		rightMovement.Enable();
		shoot.Enable();
	}
}