using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class Level : EnemyHoardProvider, EnemyDelegate {
	public LevelDelegate @delegate { private get; set; }
	
	private LevelProvider _provider;
	
	private EnemyHoardProvider _enemyHoardProvider;
	private EnemyHoard _enemyHoard;

	private Enemy[] _enemies;
	private int _enemiesKilled;
	
	private Enemy _enemyPrefab;
	private Pool<Enemy> _baseEnemyPool;
	private DefaultPoolDelegate<Enemy> _baseEnemyPoolDelegate;
	private DefaultPoolProvider<Enemy> _baseEnemyPoolProvider;

	private GameObject[] _enemyVisualPrefabs;
	private Pool<GameObject>[] _enemyVisualPools;
	private DefaultPoolDelegate<GameObject>[] _enemyVisualPoolDelegates;
	private DefaultPoolProvider<GameObject>[] _enemyVisualPoolProviders;

	private Dictionary<GameObject, Pool<GameObject>> _enemyVisualPoolOwner;

	public async Task Init(LevelData levelData, LevelProvider provider) {
		_enemiesKilled = 0;
		
		_provider = provider;
		_provider.LevelUpdate += Update;
		
		// Enemy Base Prefab Loading
		{
			var operation_handle = Addressables.LoadAssetAsync<GameObject>(levelData.enemyPrefabReference);
			await operation_handle.Task;
			if (operation_handle.Status != AsyncOperationStatus.Succeeded) {
				Debug.LogError($"Loading the Enemy Prefab Reference failed: {operation_handle.OperationException}");
			}
			_enemyPrefab = operation_handle.Result.GetComponent<Enemy>();
		}
		
		_baseEnemyPoolDelegate ??= new DefaultPoolDelegate<Enemy>(EnemyRetrieved, EnemyReturned);
		_baseEnemyPoolProvider ??= new DefaultPoolProvider<Enemy>(EnemyGetter);
		_baseEnemyPool ??= new Pool<Enemy>();
		_baseEnemyPool.@delegate = _baseEnemyPoolDelegate;
		_baseEnemyPool.provider = _baseEnemyPoolProvider;
		
		_enemyVisualPrefabs ??= new GameObject[levelData.enemyData.Length];
		// Enemy Visuals Loading
		for (var i = 0; i < levelData.enemyData.Length; i++) {
			var operation_handle = Addressables.LoadAssetAsync<GameObject>(levelData.enemyData[i].prefabReference);
			await operation_handle.Task;
			if (operation_handle.Status != AsyncOperationStatus.Succeeded) {
				Debug.LogError($"Loading the Enemy Visual Prefab Reference {i} failed: {operation_handle.OperationException}");
			}
			_enemyVisualPrefabs[i] = operation_handle.Result;
		}

		_enemyVisualPoolOwner ??= new Dictionary<GameObject, Pool<GameObject>>();
		_enemyVisualPoolDelegates ??= new DefaultPoolDelegate<GameObject>[levelData.enemyData.Length];
		_enemyVisualPoolProviders ??= new DefaultPoolProvider<GameObject>[levelData.enemyData.Length];
		_enemyVisualPools ??= new Pool<GameObject>[levelData.enemyData.Length];
		for (var i = 0; i < levelData.enemyData.Length; i++) {
			_enemyVisualPoolDelegates[i] ??= new DefaultPoolDelegate<GameObject>(EnemyVisualRetrieved, EnemyVisualReturned);
			var _i = i;
			_enemyVisualPoolProviders[i] ??= new DefaultPoolProvider<GameObject>(() => EnemyVisualGetter(_i));
			_enemyVisualPools[i] ??= new Pool<GameObject>();
			_enemyVisualPools[i].@delegate = _enemyVisualPoolDelegates[i];
			_enemyVisualPools[i].provider = _enemyVisualPoolProviders[i];
		}

		List<Enemy> enemies_list = new List<Enemy>();
		for (int i = 0; i < levelData.enemyRows.Length; i++) {
			var s_enemy_indexes = levelData.enemyRows[i].Split(',');
			for (int j = 0; j < s_enemy_indexes.Length; j++) {
				var enemy_index = int.Parse(s_enemy_indexes[j]);
				var enemy = _baseEnemyPool.Get();
				var enemy_visual = _enemyVisualPools[enemy_index].Get();
				_enemyVisualPoolOwner[enemy_visual] = _enemyVisualPools[enemy_index];
				enemy.Reset(enemy_visual);
				enemy.@delegate = this;
				enemies_list.Add(enemy);
				
				// Set enemy position
				var position = new Vector3(
					(-s_enemy_indexes.Length * levelData.enemySeparation.x / 2f) + (j * levelData.enemySeparation.x) + (levelData.enemySeparation.x / 2f),
					levelData.enemyOffset - (i * levelData.enemySeparation.y),
					0f
				);
				enemy.transform.position = position;
			}
		}
		_enemies = enemies_list.ToArray();

		_enemyHoardProvider = this;
		
		if (_enemyHoard == null) {
			_enemyHoard = new EnemyHoard();
			_enemyHoard.Init(_enemyHoardProvider);
		}
	}

	public void Fini() {
		_provider.LevelUpdate -= Update;
		_enemyHoard.Fini();
		_enemyHoard = null;
	}

	private void Update() {
		_enemyHoardProvider?.EnemyHoardUpdate?.Invoke();
	}

	Action EnemyHoardProvider.EnemyHoardUpdate { get; set; }
	Enemy[] EnemyHoardProvider.Enemies() {
		return _enemies;
	}

	void EnemyDelegate.EnemyDeath(Enemy enemy) {
		enemy.visual.transform.SetParent(null, false);
		_enemyVisualPoolOwner[enemy.visual].Return(enemy.visual);
		_baseEnemyPool.Return(enemy);

		_enemiesKilled++;
		if (_enemiesKilled >= _enemies.Length) {
			@delegate?.EnemiesKilled();
		}
	}

	private void EnemyRetrieved(Enemy enemy) {
		enemy.gameObject.SetActive(true);
	}

	private void EnemyReturned(Enemy enemy) {
		enemy.gameObject.SetActive(false);
	}

	private Enemy EnemyGetter() {
		return GameObject.Instantiate(_enemyPrefab);
	}

	private void EnemyVisualRetrieved(GameObject gameObject) {
		gameObject.SetActive(true);
	}

	private void EnemyVisualReturned(GameObject gameObject) {
		gameObject.SetActive(false);
	}

	private GameObject EnemyVisualGetter(int index) {
		return GameObject.Instantiate(_enemyVisualPrefabs[index]);
	}
}

public interface LevelProvider {
	public Action LevelUpdate { get; set; }
}

public interface LevelDelegate {
	public void EnemiesKilled();
}