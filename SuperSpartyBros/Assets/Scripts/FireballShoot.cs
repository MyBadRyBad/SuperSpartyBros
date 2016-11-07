using UnityEngine;
using System.Collections;

public class FireballShoot : MonoBehaviour {
	public float speed = 10.0f;
	private Rigidbody2D _rb;

	// Use this for initialization
	void Start () {
		_rb = this.gameObject.GetComponent<Rigidbody2D> ();
		if (!_rb) {
			_rb = gameObject.AddComponent<Rigidbody2D> ();
		}

		_rb.velocity = new Vector2 (speed, _rb.velocity.y);
	}

	void OnTriggerEnter2D(Collider2D collider) {
		if (collider.gameObject.tag == "Enemy") {
			RPGGameManager.gm_rpg.DamageEnemy1 (40.0f, true);
		}
	}
}
