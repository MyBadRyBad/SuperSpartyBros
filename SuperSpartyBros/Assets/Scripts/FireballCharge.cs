using UnityEngine;
using System.Collections;

public class FireballCharge : MonoBehaviour {
	public float chargeTime = 4.0f;
	public float scaleFactor = 2.0f;
	public GameObject fireBall;
	private float _currentChargeTime = 0.0f;
	private bool _shouldCreateFireball = false;


	// Use this for initialization
	void Start () {
		_currentChargeTime = chargeTime;
	}
	
	// Update is called once per frame
	void Update () {
		if (!_shouldCreateFireball && _currentChargeTime >= 0) {
			_currentChargeTime -= Time.deltaTime;
			RotateAndScale ();

			if (_currentChargeTime < 0) {
				_shouldCreateFireball = true;

				Instantiate (fireBall, gameObject.transform.position, Quaternion.identity);
				Invoke ("DestroySelf", 0.1f);
			}
		}
	}

	void RotateAndScale() {
		transform.localScale += new Vector3 (scaleFactor * Time.deltaTime, scaleFactor * Time.deltaTime, 0.0f);
		transform.Rotate (0, 0, 180 * Time.deltaTime);
	}

	public void DestroySelf () {
		Destroy (gameObject);
	}

}
