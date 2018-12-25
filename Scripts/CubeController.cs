using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CubeController : MonoBehaviour {

	public bool triggered;
	public int speedMod; 
	private float x;
	public bool alive;

    // Unity UI elements
    public Text score;

    // Use this for initialization
    void Start ()
    {
        alive = true;
        triggered = false;
        // SPOTIFY: change speed with song tempo
        speedMod = (int)(5 + (SpotifyVals.Tempo * .01));           
	}
	
	// Update is called once per frame
	void Update ()
    {
        speedMod = (int)(5 + (SpotifyVals.Tempo * .01));
        x = speedMod * Time.deltaTime;
		this.transform.Translate(x, 0, 0, Space.World);

        // Escape key loads main menu
		if(Input.GetKeyDown(KeyCode.Escape)) {
	        SceneManager.LoadScene(0);
		}

        // Calculate score based on distance through game
		score.text = "Score: " + Mathf.RoundToInt(this.transform.position.x).ToString();
	}

    // Game over if you collide with an obstacle
	void OnTriggerEnter(Collider other)
    {
		if(other.gameObject.CompareTag("edge")) {
			GameObject gameover = GameObject.FindWithTag("Gameover");
			gameover.SetActive(true);
			triggered = true;
			x = 0;
		}
	}
}
