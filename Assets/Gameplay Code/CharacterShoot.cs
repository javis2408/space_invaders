using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

public class CharacterShoot : MonoBehaviour, WeaponProvider {
	[SerializeField] private float _sourceOffset;
	
	private InputManager _inputManager;
	private Weapon _weapon;
	private LevelBounds _levelBounds;

	public Action WeaponUpdate { get; set; }
	
	public async Task Init(InputManager inputManager, BulletAsset bulletAsset, LevelBounds levelBounds) {
		_inputManager = inputManager;
		_levelBounds = levelBounds;

		_weapon = new DefaultWeapon(this);
		await _weapon.Init(bulletAsset, _levelBounds, _sourceOffset);

		_inputManager.shoot.performed += ShootPerformed;
		_inputManager.shoot.canceled += ShootCanceled;
	}

	private void Update() {
		WeaponUpdate?.Invoke();
	}

	private void ShootPerformed(InputAction.CallbackContext callbackContext) {
		_weapon.Shoot(transform.position);
	}
	
	private void ShootCanceled(InputAction.CallbackContext callbackContext) {
		
	}

#if UNITY_EDITOR
	private void OnDrawGizmos() {
		// Draw muzzle
		float crosshair_size = 0.5f;
		Vector3 position = transform.position;
		Gizmos.color = Color.red;
		Gizmos.DrawLine(
			from: new Vector3(position.x + -crosshair_size, position.y + _sourceOffset, 0f),
			to: new Vector3(position.x + crosshair_size, position.y + _sourceOffset, 0f)
		);
		Gizmos.DrawLine(
			from: new Vector3(position.x, position.y + _sourceOffset - crosshair_size, 0f),
			to: new Vector3(position.x, position.y + _sourceOffset + crosshair_size, 0f)
		);
	}
#endif
}