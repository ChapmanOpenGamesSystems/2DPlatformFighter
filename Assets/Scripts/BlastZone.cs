using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlastZone : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Function that detects the object, determines if it is a player, and if it is kills them
        if(collision.gameObject.tag != "Player")
        {
            return;
        }
        GameManager.Instance.PlayerKill(collision.gameObject.GetComponent<Player>().playerNum);
        //Destroy(collision.gameObject);
    }
}
