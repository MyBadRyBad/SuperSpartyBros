using UnityEngine;
using System.Collections;

public class BattleTextIndicator : MonoBehaviour {

	private TextMesh _textMesh;
	private Rigidbody _rb;
	public Color textColor;
	public string text;

	// Use this for initialization
	void Start () {
		_textMesh = gameObject.GetComponent<TextMesh> ();
		_rb = gameObject.GetComponent<Rigidbody> ();
		
		if (_textMesh == null) {
			Debug.LogError("BattleTextIndicator doesn't have a Text Mesh");
		}

		if (_rb == null) {
			_rb = gameObject.AddComponent<Rigidbody> ();
			_rb.isKinematic = true;
		}

		_textMesh.text = text;
		_textMesh.color = textColor;


	}
	
	// Update is called once per frame
	void Update () {
		DimOpacity ();
	}
		

	void FixedUpdate() {
		_rb.MovePosition(transform.position + transform.up * Time.deltaTime);
	}

	void DimOpacity() {
		float newOpacity = _textMesh.color.a;
		newOpacity -= Time.deltaTime;
		if (newOpacity < 0) {
			newOpacity = 0;
		}

		_textMesh.color = new Color (_textMesh.color.r, _textMesh.color.g, _textMesh.color.b, newOpacity);

	}

	void BrightenOpacity() {

	}
}
