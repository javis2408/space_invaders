using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Serializable]
public struct BulletData {
	public int damage;
}

[CreateAssetMenu(fileName = "New Bullet Asset", menuName = "Gameplay/Bullet Asset")]
public class BulletAsset : ScriptableObject {
	public BulletData data;
	public AssetReference prefabReference;
	public AssetReference muzzleVFXReference;
}