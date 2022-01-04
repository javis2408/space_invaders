using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.VFX;

public abstract class Weapon : PoolDelegate<Bullet>, PoolProvider<Bullet>, PoolDelegate<VisualEffect>, PoolProvider<VisualEffect>, BulletDelegate {
	protected Pool<Bullet> _pool;
	protected Pool<VisualEffect> _vfxPool;
	protected Bullet _prefab;
	protected VisualEffect _vfxPrefab;
	protected WeaponProvider _provider;

	protected float _timeSinceLastShot;
	protected LevelBounds _levelBounds;
	protected float _sourceOffset;

	protected BulletAsset _bulletAsset;

	public Weapon(WeaponProvider provider) {
		_provider = provider;
		_provider.WeaponUpdate += Update;
	}

	public virtual async Task Init(BulletAsset bulletAsset, LevelBounds levelBounds, float sourceOffset) {
		// Load bullet
		{
			if (_prefab == null) {
				var operation_handle = Addressables.LoadAssetAsync<GameObject>(bulletAsset.prefabReference);
				await operation_handle.Task;
				if (operation_handle.Status != AsyncOperationStatus.Succeeded) {
					Debug.LogError(
						$"Loading the Bullet Prefab Reference failed: {operation_handle.OperationException}");
				}

				_prefab = operation_handle.Result.GetComponent<Bullet>();
			}

			_pool ??= new Pool<Bullet> {
				@delegate = this,
				provider = this
			};
		}
		
		// Load bullet muzzle vfx
		{
			if (_vfxPrefab == null) {
				var operation_handle = Addressables.LoadAssetAsync<GameObject>(bulletAsset.muzzleVFXReference);
				await operation_handle.Task;
				if (operation_handle.Status != AsyncOperationStatus.Succeeded) {
					Debug.LogError(
						$"Loading the Bullet VFX Prefab Reference failed: {operation_handle.OperationException}");
				}

				_vfxPrefab = operation_handle.Result.GetComponent<VisualEffect>();
			}
			
			_vfxPool ??= new Pool<VisualEffect>() {
				@delegate = this,
				provider = this
			};
		}

		_levelBounds = levelBounds;
		_sourceOffset = sourceOffset;
		_bulletAsset = bulletAsset;
	}

	public virtual void Fini() {
		_provider.WeaponUpdate -= Update;
	}

	public virtual void Shoot(Vector3 source) {
		_timeSinceLastShot = 0f;
	}

	public virtual void BulletHit(Bullet bullet) {
		_pool.Return(bullet);
	}

	protected virtual void Update() {
		_timeSinceLastShot += Time.deltaTime;
	}

	void PoolDelegate<Bullet>.Retrieved(Bullet bullet) {
		bullet.gameObject.SetActive(true);
	}

	void PoolDelegate<Bullet>.Returned(Bullet bullet) {
		bullet.gameObject.SetActive(false);
		bullet.Reset();
	}
	
	Func<Bullet> PoolProvider<Bullet>.Getter => bullet_getter;
	private Bullet bullet_getter() {
		return GameObject.Instantiate(_prefab);
	}

	void PoolDelegate<VisualEffect>.Retrieved(VisualEffect vfx) {
		vfx.gameObject.SetActive(true);
	}

	void PoolDelegate<VisualEffect>.Returned(VisualEffect vfx) {
		vfx.gameObject.SetActive(false);
	}

	Func<VisualEffect> PoolProvider<VisualEffect>.Getter => vfx_getter;

	private VisualEffect vfx_getter() {
		return GameObject.Instantiate(_vfxPrefab);
	}
}

public interface WeaponProvider {
	public Action WeaponUpdate { get; set; }
}

public class DefaultWeapon : Weapon {
	private float bulletSpeed = 0.25f;
	private float rateOfFire = 0.15f;

	public DefaultWeapon(WeaponProvider provider) : base(provider) {
		
	}
	
	public override void Shoot(Vector3 source) {
		if (_timeSinceLastShot < rateOfFire) return;

		var relative_source = new Vector3(source.x, source.y + _sourceOffset, source.z);		
		
		var bullet = _pool.Get();
		bullet.transform.position = relative_source;
		bullet.@delegate = this;
		bullet.Shoot(bulletSpeed, _levelBounds.vertical, _bulletAsset.data.damage);

		var vfx = _vfxPool.Get();
		vfx.transform.position = relative_source;
		vfx.Play();
		WaitForVFX(vfx);
	}

	// having a proper Visual Effect lifestyle would require some extra work that is not worth for this project size 
	private async void WaitForVFX(VisualEffect vfx) {
		await Task.Delay(100);
		
		while (vfx.aliveParticleCount > 0)
			await Task.Delay(20);

		vfx.Stop();
		_vfxPool.Return(vfx);
	}
}