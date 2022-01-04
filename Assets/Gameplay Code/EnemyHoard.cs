using System;
using UnityEngine;

public class EnemyHoard {
	private float _timer = 5f;
	private float _elapsed = 0f;
	private float _distance = 2f;
	
	private EnemyHoardProvider _provider;
	private Enemy[] _enemies;

	private bool _initialized;

	public void Init(EnemyHoardProvider provider) {
		_provider = provider;
		_enemies = provider.Enemies();
		_provider.EnemyHoardUpdate += Update;
		_initialized = true;
	}

	public void Fini() {
		_provider.EnemyHoardUpdate -= Update;
		_enemies = null;

		_initialized = false;
	}

	private void Update() {
		if (!_initialized) return;

		_elapsed += Time.deltaTime;
		if (_elapsed < _timer) return;

		for (var i = 0; i < _enemies.Length; i++) {
			var enemy = _enemies[i];
			var enemy_transform = enemy.transform;
			var old_position = enemy_transform.position;
			enemy_transform.position = new Vector3(old_position.x, old_position.y - _distance, old_position.z);
		}
		_elapsed = 0f;
	}
}

public interface EnemyHoardProvider {
	public Action EnemyHoardUpdate { get; set; }
	public Enemy[] Enemies();
}