using UnityEngine;
using System.Collections;

public class EnemyRPGAI : MonoBehaviour {

	public float attackDamage = 5.0f;

	public float attackDelayMin = 5.0f;
	public float attackDelayMax = 10.0f;
	private float _attackTimer = 0.0f;
	private bool _isAttacking = false;

	public float defendDelayMin = 3.0f;
	public float defendDelayMax = 10.0f;
	private float _defendTimer = 0.0f;
	private bool _isDefending = false;

	private float _currentHealth = 500.0f;
	public TextMesh healthTextMesh;

	private Animator _animator;

	// Use this for initialization
	void Start () {
		_animator = GetComponent<Animator>();
		if (_animator==null) // if Animator is missing
			Debug.LogError("Animator component missing from this gameobject");


		_attackTimer = Time.time + Random.Range (attackDelayMin, attackDelayMax);
		_defendTimer = Time.time + Random.Range (defendDelayMin, defendDelayMax);
	}
	
	// Update is called once per frame
	void Update () {
		if (!_isAttacking && !_isDefending) {
			if (_attackTimer <= Time.time) {
				Attack ();
			} else if (_defendTimer <= Time.time) {
				Defend ();
			}
		}
	}

	void Attack() {
		_attackTimer = Time.time + Random.Range (attackDelayMin, attackDelayMax);
		_animator.Play ("EnemyAttack", 0, 0);

		_isAttacking = true;
		Invoke ("EndAttack", 1.0f);
	}

	void EndAttack() {
		_isAttacking = false;

		RPGGameManager.gm_rpg.DamagePlayer(attackDamage, false);

		_animator.Play ("EnemyIdle", 0, 0);
	}

	void Defend() {
		_defendTimer = Time.time + Random.Range (defendDelayMin, defendDelayMax);
		_animator.Play ("EnemyDefend", 0, 0);

		_isDefending = true;
		Invoke ("EndDefend", 1.0f);
	}

	void EndDefend() {
		
		_isDefending = false;
		_animator.Play ("EnemyIdle", 0, 0);
	}

	// damage Enemy
	public void DamageEnemy(float damageAmount, bool ignoreShield) {
		if (!_isDefending || ignoreShield) {
			_currentHealth -= damageAmount;
			healthTextMesh.text = _currentHealth.ToString ("f0") + " / 500";
		}

	}
}
