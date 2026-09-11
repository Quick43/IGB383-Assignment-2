using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour {

    GameManager gameManager;

	public static AudioManager audioManager;

	public AudioSource SFX;

    public AudioClip[] music;

	// Use this for initialization
	void Start () {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
	}
	
	// Update is called once per frame
	void Update () {

        //Change Music when game starts
        if (gameManager.gameStarted == true) {
            SFX.clip = music[1];
            if (!SFX.isPlaying)
                SFX.Play();
        } 
        else
            SFX.clip = music[0];

    }

	public void PlayAudio(AudioClip sound){
		SFX.PlayOneShot (sound);
	}
}
