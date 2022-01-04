using System;
using UnityEngine;

public class Bullet : MonoBehaviour {
	private float _speed;
	private float _maxDistance;
	private int _damage;

	public BulletDelegate @delegate { private get; set; }

	public void Shoot(float speed, float maxDistance, int damage) {
		_speed = speed;
		_maxDistance = maxDistance;
		_damage = damage;
	}

	public void Reset() {
		_speed = 0f;
		_damage = 0;
		@delegate = null;
	}

	private void Update() {
		transform.Translate(transform.forward * _speed, Space.World);

		if (transform.position.y > _maxDistance) {
			@delegate?.BulletHit(this);
		}
	}

	private void OnTriggerEnter(Collider other) {
		var damageable = other.gameObject.GetComponent<Damageable>();
		if (damageable == null) return;
		damageable.TakeDamage(_damage);
		@delegate?.BulletHit(this);
	}
}

public interface BulletDelegate {
	public void BulletHit(Bullet bullet);
}

public interface Damageable {
	public void TakeDamage(int damage);
}