using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitDetection : MonoBehaviour {

    Player p;
    Animator anim;

	// Use this for initialization
	void Start () {
        p = transform.parent.GetComponent<Player>();
        anim = transform.parent.GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
		
        if(!anim.GetCurrentAnimatorStateInfo(0).IsName("Grabbed") && IsInvoking("EscapeGrab"))
        {
            CancelInvoke(); //Cancel EscapeGrab if the player isn't grabbed.
        }

	}

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Attack")
        {
            PlayerAttack attacker = col.transform.parent.GetComponent<PlayerAttack>();
            PlayerAttack.Attack attack;
            if (col.name.Equals("Bair"))
            {
                attack = attacker.Bair;
            }
            else if (col.name.Equals("Fair"))
            {
                attack = attacker.Fair;
            }
            else if(col.name.Equals("Uair"))
            {
                attack = attacker.Uair;
            }
            else if (col.name.Equals("Dair"))
            {
                attack = attacker.Dair;
            }
            else if(col.name.Equals("Nair"))
            {
                attack = attacker.Nair;
            }
            else if (col.name.Equals("Ftilt"))
            {
                attack = attacker.Ftilt;
            }
            else if(col.name.Equals("Utilt"))
            {
                attack = attacker.Utilt;
            }
            else if (col.name.Equals("Dtilt"))
            {
                attack = attacker.Dtilt;
            }
            else if(col.name.Equals("Jab"))
            {
                attack = attacker.Jab;
            }
            else
            {
                Debug.Log("UNKNOWN ATTACK: " + col.name);
                Debug.Log("Defaulting to attacker's jab");
                attack = attacker.Jab;
            }
            //If attacking player is flipped, flip attack knockback direction as well
            if(!col.transform.parent.GetComponent<Player>().facingRight)
            {
                attack.knockbackDirection = new Vector2(attack.knockbackDirection.x * -1, attack.knockbackDirection.y);
            }

            p.PlayerStagger(attack);
        }

        if (col.tag == "Projectile")
        {
            PlayerAttack attacker = col.GetComponent<ProjectileScript>().shooter.GetComponent<PlayerAttack>();
            PlayerAttack.Attack attack = new PlayerAttack.Attack();
            attack.hitbox = col.gameObject;
            attack.attackDamage = attacker.projectileDamage;
            attack.knockbackAmount = attacker.projectileKnockback;
            attack.knockbackDirection = col.GetComponent<Rigidbody2D>().velocity.normalized;
            p.PlayerStagger(attack);
        }

        if (col.tag == "Grab")
        {
            Debug.Log("Player Grabbed!");
            this.transform.parent.parent = col.transform.parent;
            col.transform.parent.GetComponent<Animator>().SetTrigger("Grab Success");
            col.transform.parent.GetComponent<Animator>().ResetTrigger("Grab Release");
            anim.SetTrigger("Grabbed");
            p.rb.simulated = false;
            Invoke("EscapeGrab", 5); //HOW DO CANCEL INVOKE?
            return;
        }
    }

    void EscapeGrab()
    {
        //Players only escape the grab if they're still grabbed
        if(anim.GetCurrentAnimatorStateInfo(0).IsName("Grabbed"))
        {
            this.transform.parent.parent.GetComponent<Animator>().SetTrigger("Grab Release");
            anim.SetTrigger("Grab Escape");
            p.rb.simulated = true;
            this.transform.parent.parent = null;
            Debug.Log("Player Escaped!");
        }
    }
}
