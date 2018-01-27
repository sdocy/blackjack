using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class gameover : MonoBehaviour {
    public Text playerScoreText;

	// Use this for initialization
	void Start () {
        playerScoreText.text = globals.playerScore.ToString();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
