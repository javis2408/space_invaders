using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.VFX;

public abstract class Enemy : MonoBehaviour, Damageable {
	[SerializeField] private int _initialHealth = 1;
	[SerializeField] private VisualEffect _deathVFX;
	
	protected GameObject _visual;
	public GameObject visual => _visual;

	public EnemyDelegate @delegate { protected get; set; }

	public int health { get; protected set; }
	public virtual void Reset(GameObject visual) {
		health = _initialHealth;
		_visual = visual;
		_visual.transform.SetParent(transform, false);
		_visual.transform.localPosition = Vector3.zero;
		
		_deathVFX.gameObject.SetActive(false);
	}
	protected abstract void Hit(int damage);
	protected virtual void Death() {
		@delegate?.EnemyDeath(this);
		_deathVFX.transform.SetParent(null, true);
		_deathVFX.gameObject.SetActive(true);
		_deathVFX.Play();
		WaitForVFX();
	}
	void Damageable.TakeDamage(int damage) {
		Hit(damage);
	}

	private async void WaitForVFX() {
		await Task.Delay(200);
		_deathVFX.transform.SetParent(transform, false);
		_deathVFX.transform.localPosition = Vector3.zero;
	}
}

public interface EnemyDelegate {
	public void EnemyDeath(Enemy enemy);
}