    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageHazard : MonoBehaviour
{
    //public ThingToReference thing;
    public PlayerAttack.Attack hazard;

	void Start ()
    {

	}
	
	void Update ()
    {
		
	}

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            if (col.GetComponent<Player>().playerDamage >= 125)
            {
                GameManager.Instance.PlayerKill(col.GetComponent<Player>().playerNum);
            }
            else
            {
                col.GetComponent<Player>().PlayerStagger(hazard);
            }
        }
    }
}
