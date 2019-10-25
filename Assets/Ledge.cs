using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ledge : MonoBehaviour {

    public bool isFacingRight;

    private void OnTriggerEnter2D(Collider2D col) {
        if (col.CompareTag("Player"))
        {
            if(isFacingRight != col.gameObject.GetComponent<Player>().facingRight) {
                col.gameObject.GetComponent<PlayerMovement>().PlayerFlip();
            }
            //Ensuring the player can't/won't move when they grab a ledge until they jump/getup somehow
            col.gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
            col.gameObject.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            col.gameObject.GetComponent<Animator>().SetTrigger("LedgeGrab");
            col.gameObject.GetComponent<Animator>().SetBool("CanLandCancel",false);
            col.gameObject.GetComponent<PlayerMovement>().jumpsLeft = col.gameObject.GetComponent<PlayerMovement>().maxJumps;
            Debug.Log("GRAB!");
        }
    }
}
