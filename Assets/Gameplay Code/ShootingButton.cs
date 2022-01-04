using System;
using UnityEngine;

public class ShootingButton : MonoBehaviour, Damageable {
	private Action _shoot;

	public void Init(Action shoot) {
		_shoot = shoot;
	}

	public void TakeDamage(int damage) {
		_shoot?.Invoke();
	}
}