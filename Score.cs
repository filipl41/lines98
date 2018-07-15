using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour {

	public Transform go;
	private BoardGenerator bg; 
	public Text score;

	void Start(){
			bg = go.GetComponent<BoardGenerator>();
	}
	
	// Update is called once per frame
	void Update () {
		
		score.text = "SCORE\n" + bg.score.ToString();
	}
}
