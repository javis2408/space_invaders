using UnityEngine;

public class DefaultEnemy : Enemy {
	protected override void Hit(int damage) {
		health -= damage;
		if (health <= 0) {
			Death();
		}
	}
}