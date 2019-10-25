using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public int playerScore;
    public int playerDamage;
    public int playerNum; //playerNum starts at 1
    public bool isGrounded { get; set; } //Whether or not the player is grounded
    public bool facingRight { get; set; } //Whether or not the player is facing right
    public BoxCollider2D hitbox; //BoxCollider2D on the player character to be used as hitbox
    public bool isDodging { get; set; }//Whether or not the player is dodging
    public bool isAttacking { get; set; } //Keeping track of whether or not the player is attacking(To prevent multiple attacks at once)
    public int lastHit { get; set; } //int playerNum of last player to hit this one

    public Rigidbody2D rb { get; set; }
    private Animator anim;

    public enum PlayerState
    {
        IDLE,
        GROUNDED,
        DODGING,
        AERIAL,
        SLIDING, //SLIDING = Air Slide = Air Dodge
        ATTACKING,
        GRABBING,
        GRABBED,
        STAGGERED
    }

    public enum PlayerThrow
    {
        FORWARD_THROW,
        BACK_THROW,
        DOWN_THROW,
        UP_THROW
    }



    // Use this for initialization
    void Start () {
        facingRight = true;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        //Physics2D.IgnoreLayerCollision(2, 2, true); //Ignore layer collision between Ignore Raycast Layers (What I currently have the players on)
    }

    public void PlayerStagger(Collider2D col) //PlayerStagger for all cases beyond player attacks NOTE: sHOULD NOT BE USED
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Grabbing Ledge"))
        {
            //Undo code for ledge grab
            anim.SetBool("CanLandCancel", true);
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        //TODO: Replace Set Damage with Attack Specific Damages
        Debug.Log("5 Damage Dealt by " + col.name);
        GameManager.Instance.PlayerDamage(playerNum, 5);
        //TODO: Add Knockback
        //TODO: Add Knockback Specific to each attack
        Debug.Log("HIT! by " + col.name + " = Totally sent flying: Non-Player Attack");
    }

    public void PlayerStagger(PlayerAttack.Attack attack) //PlayerStagger for all player attacks
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Grabbing Ledge"))
        {
            //Undo code for ledge grab
            anim.SetBool("CanLandCancel", true);
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
            Debug.Log(attack.attackDamage + " Damage Dealt by " + attack.name);
        GameManager.Instance.PlayerDamage(playerNum, attack.attackDamage);
        rb.AddForceAtPosition(CalculateKnockback(attack.knockbackDirection, attack.knockbackAmount), attack.hitbox.transform.position);
        anim.SetTrigger("Stagger");
        if (attack.hitbox.tag.Equals("Projectile"))
        {
            lastHit = attack.hitbox.GetComponent<ProjectileScript>().shooter.GetComponent<Player>().playerNum;
        }
        else if(attack.hitbox.tag.Equals("Hazard"))
        {
            //Do nothing with the last hit variable
        }
        else
        {
            lastHit = attack.hitbox.transform.parent.gameObject.GetComponent<Player>().playerNum;
        }
    }

    public void PlayerThrown(PlayerThrow throwType)
    {
        Debug.Log(this.transform.name + " " + throwType + "N!");
    }

    public Vector2 CalculateKnockback(Vector2 knockbackDirection, float knockbackAmount)
    {
        if(GameManager.Instance.gamemode == GameManager.Gamemode.STAMINA) //In stamina, playerDamage starts at 100 and decreases to 0, so it's the same formula but modified
        {
            return (knockbackAmount * knockbackDirection) * (50 / ((float)playerDamage + 50)); ;
        }
        return (knockbackAmount * knockbackDirection) * (((float)playerDamage + 50)/50);
    }
}
