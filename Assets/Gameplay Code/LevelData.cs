using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "New Level Data", menuName = "Gameplay/Level Data")]
public class LevelData : ScriptableObject {
	[SerializeField] private AssetReference _enemyPrefabReference;
	public AssetReference enemyPrefabReference => _enemyPrefabReference;
	
	[SerializeField] private LevelEnemyData[] _enemyData;
	public LevelEnemyData[] enemyData => _enemyData;

	[SerializeField] private string[] _enemyRows;
	public string[] enemyRows => _enemyRows;
	
	[SerializeField] private float _enemyOffset = 5f;
	public float enemyOffset => _enemyOffset;

	[SerializeField] private Vector2 _enemySeparation = Vector2.one;
	public Vector2 enemySeparation => _enemySeparation;
}

[Serializable]
public struct LevelEnemyData {
	public AssetReference prefabReference;
}