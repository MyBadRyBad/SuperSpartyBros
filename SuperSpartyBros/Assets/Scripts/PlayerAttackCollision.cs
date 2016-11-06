using UnityEngine;
using System.Collections;

public class PlayerAttackCollision : MonoBehaviour {

	private PlayerControllerRPG _playerControllerRPG;

	// Use this for initialization
	void Start () {
		_playerControllerRPG = gameObject.transform.parent.gameObject.GetComponent<PlayerControllerRPG> ();
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (other.gameObject.tag == "Enemy") {
			Debug.Log ("Did Trigger");
			_playerControllerRPG.ExecuteAttack ();
		}
	}

	void OnCollisionEnter2D(Collision2D other)
	{
		Debug.Log ("DidCollide");
		if (other.gameObject.tag == "Enemy")
		{
			Debug.Log ("Did collide");
			this.GetComponentInParent<PlayerControllerRPG> ().ExecuteAttack();

			// make the character bounce off the enemy AKA Jump
	//		other.gameObject.GetComponent<CharacterController2D>().enemyBounce();
		}
	} 
}
