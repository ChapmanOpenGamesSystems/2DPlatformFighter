﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    [SerializeField] private float walkSpeed = 7.5f; //The speed at which the player walks
    [SerializeField] private float runSpeed = 15f; //The speed at which the player runs
    [SerializeField] private float jumpForce = 500f; //The force applied on player jump
    [SerializeField] private float airSpeed = 10f; //The speed at which the player moves through the air
    public short maxJumps = 2; //The max number of jumps players have

    public int playerNum = 0;

    private Player p;
    private Animator anim;

    private float smoothTime = 0.2f; //Internal smoothing value used for SmoothDamp
    private float horizontalMove; //The current velocity of the player
    public short jumpsLeft { get; set; } //How many jumps the player has left
    private Rigidbody2D rb; //The Rigidbody2D on the player character
    private Vector2 smoothingVelocity = Vector2.zero; //Internal Vector2 to be used as ref parameter for SmoothDamp

    private bool hasAirDodged = false;
    private float dodgeRollLength = 0.5f; //How long the player is in the dodge roll (How long their hitbox will remain off)
    private float rollForce = 500f;
    private float airDodgeLength = 0.5f; //How long the player is in a neutral air dodge (How long their hitbox will remain off)
    private float airDodgeForce = 500f;
    private float spotDodgeLength = 0.25f; //How long the player is in a spot Dodge
    private bool isFastFalling = false;

    private bool downPressed;
    private bool dodgePressed;
    private bool dodgeDown;
    private bool downDown;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody2D>();
        p = GetComponent<Player>();
        anim = GetComponent<Animator>();
        jumpsLeft = maxJumps;
	}
	
	// Update is called once per frame
	void Update () {
        if(Input.GetAxis("Vertical" + playerNum) < 0 && !downDown) {
            downPressed = true;
            downDown = true;
        } else {
            downPressed = false;
        }

        if(Mathf.Abs(Input.GetAxis("Dodge" + playerNum)) > 0.1 && !dodgeDown) {
            dodgePressed = true;
            dodgeDown = true;
        } else {
            dodgePressed = false;
        }

        if(Input.GetAxis("Vertical" + playerNum) == 0) {
            downDown = false;
        }

        if(Mathf.Abs(Input.GetAxis("Dodge" + playerNum)) < 0.1) {
            dodgeDown = false;
        }


        RaycastHit2D beamToFloor = Physics2D.Raycast(transform.position, -Vector2.up, 1.91f); //1.905 should be distance to ground, but 1.91 allows leniency to avoid bugs (outliers occasionally popped up and prevented flipping)
        //Jump if player has jumps left
        if (Input.GetButtonDown("Jump" + playerNum) && jumpsLeft > 0)
        {
            if(anim.GetCurrentAnimatorStateInfo(0).IsName("Movement") || anim.GetCurrentAnimatorStateInfo(0).IsName("Jump"))
            {
                PlayerJump();
                p.isGrounded = false;
            }
            else if(anim.GetCurrentAnimatorStateInfo(0).IsName("Grabbing Ledge"))
            {
                //Undo code for ledge grab
                anim.SetBool("CanLandCancel", true);
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;

                PlayerJump();
                p.isGrounded = false;
            }
        }
        else if (beamToFloor.collider != null) //If you're close to the floor and haven't jumped, you must be grounded.
        {
            if(!p.isGrounded)
            {
                anim.SetTrigger("Land-Cancel");
            }
            p.isGrounded = true;
            isFastFalling = false;
            hasAirDodged = false;
            jumpsLeft = maxJumps; //When you're grounded, you regain your max jumps
            GetComponent<Player>().lastHit = 0; //When Grounded, lastHit resets
        }
        else
        {
            p.isGrounded = false;
        }

        //Change player movement based on whether or not player is sprinting/in the air
        if(anim.GetCurrentAnimatorStateInfo(0).IsName("Movement") && p.isGrounded)
        {
            if(Mathf.Abs(Input.GetAxisRaw("Horizontal" + playerNum)) > 0.1)
            {
                if (Input.GetButton("Sprint" + playerNum))
                {
                    horizontalMove = Input.GetAxisRaw("Horizontal" + playerNum) * runSpeed;
                    anim.SetFloat("Move", 1.0f);
                }
                else
                {
                    horizontalMove = Input.GetAxisRaw("Horizontal" + playerNum) * walkSpeed;
                    anim.SetFloat("Move", 0.5f);
                }
            }
            else
            {
                horizontalMove = 0f;
                anim.SetFloat("Move", 0f);
            }
        }
        else if(anim.GetCurrentAnimatorStateInfo(0).IsName("Grabbed") || anim.GetCurrentAnimatorStateInfo(0).IsName("Grabbing") || anim.GetCurrentAnimatorStateInfo(0).IsName("Grab") || anim.GetCurrentAnimatorStateInfo(0).IsName("Grabbing Ledge")) //If you are being grabbed/grabbing/on ledge, you can't move 
        {
            horizontalMove = 0;
        }
        else //If you're not being grabbed or grabbing, you can move
        {
            //TODO: Implement a falling animation
            horizontalMove = Input.GetAxisRaw("Horizontal" + playerNum) * airSpeed;
            //If you're in the air and press down after/at the peak of your jump, you fast fall
            if ((anim.GetCurrentAnimatorStateInfo(0).IsName("Movement") || anim.GetCurrentAnimatorStateInfo(0).IsName("Jump")) && !p.isGrounded)
            {
                if (downPressed && Input.GetAxisRaw("Vertical" + playerNum) < 0 && rb.velocity.y <= 0 && !isFastFalling)
                {
                    rb.AddForce(new Vector2(0f, -1 * jumpForce));
                    isFastFalling = true;
                    downPressed = false;
                }
            }
        }


        //Move player based on their current speed
        PlayerMove(horizontalMove);
        //If the player is on the ground (GROUNDED or IDLE), they can dodge roll/spot dodge
        if(dodgePressed && anim.GetCurrentAnimatorStateInfo(0).IsName("Movement") && p.isGrounded)
        {
            if(Input.GetAxisRaw("Vertical" + playerNum) < 0)
            {
                StartCoroutine(PlayerSpotDodge());
            }
            else
            {
                StartCoroutine(PlayerDodgeRoll());
            }
        }
        //If they're in the air and try to air dodge, they do unless they already have
        else if (dodgePressed && (anim.GetCurrentAnimatorStateInfo(0).IsName("Movement") || anim.GetCurrentAnimatorStateInfo(0).IsName("Jump")) && !p.isGrounded && !hasAirDodged)
        {
            StartCoroutine(PlayerAirDodge());
        }



    }


    /*~~~~~~~~~~~~~~ ALL THINGS BELOW NEED TO BE WORKED ON ~~~~~~~~~~~~~~~~~~~~*/
    //Use Start() and Update() as needed


    //Function for moving to the left or right
    void PlayerMove(float horizontalVelocity)
    {
        //Flip the character model if it changes horizontal direction on the ground
        if(p.facingRight && horizontalVelocity < 0 && anim.GetCurrentAnimatorStateInfo(0).IsName("Movement") && p.isGrounded)
        {
            PlayerFlip();
        }
        else if(!p.facingRight && horizontalVelocity > 0 && anim.GetCurrentAnimatorStateInfo(0).IsName("Movement") && p.isGrounded)
        {
            PlayerFlip();
        }

        Vector2 targetVelocity;

        if (isFastFalling && rb.velocity.y < 0)  //Precondition: The player is 'fastfalling' and moving downwards
        {
            targetVelocity = new Vector2(horizontalVelocity, 2 * rb.velocity.y);
        }
        else
        {
            targetVelocity = new Vector2(horizontalVelocity, rb.velocity.y);
        }

        rb.velocity = Vector2.SmoothDamp(rb.velocity, targetVelocity, ref smoothingVelocity, smoothTime);
    }

    //Function for player jumping and double jump
    void PlayerJump()
    {
        anim.SetTrigger("Jump");
        //Reset the vertical velocity to 0 before adding jump force(to keep jumps consistent)
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(new Vector2(0f, jumpForce));
        --jumpsLeft;
    }

    //Function for the player dodge rolling
    IEnumerator PlayerDodgeRoll() //IEnumerator for waitForSeconds()
    {
        anim.SetTrigger("Dodge Roll");
        p.hitbox.enabled = false; //Disabling boxCollider hitbox
        if(p.facingRight)
        {
            rb.AddForce(new Vector2(rollForce, 0f));
        }
        else
        {
            rb.AddForce(new Vector2(rollForce * -1, 0f));
        }
        yield return new WaitForSeconds(dodgeRollLength); //TODO: Replace dodgeRollLength with a wait until no longer in dodge roll state
        p.hitbox.enabled = true; //Re-enabling boxCollider hitbox
    }

    //Function for the player dodge rolling
    IEnumerator PlayerSpotDodge() //IEnumerator for waitForSeconds()
    {
        anim.SetTrigger("Spot Dodge");
        p.hitbox.enabled = false; //Disabling boxCollider hitbox
        yield return new WaitForSeconds(spotDodgeLength); //TODO: Replace spotDodgeLength with a wait until no longer in spot dodge state
        p.hitbox.enabled = true; //Re-enabling boxCollider hitbox
    }

    //Function for player air dodging
    IEnumerator PlayerAirDodge()
    {
        //Air Dodge
        hasAirDodged = true;
        p.hitbox.enabled = false; //Disabling boxCollider hitbox

        //Apply force based on vertical/Horizontal input after zeroing out velocity
        if (Input.GetAxisRaw("Horizontal" + playerNum) == 0 && Input.GetAxisRaw("Vertical" + playerNum) == 0)
        {
            //Neutral Dodge keeps initial velocity
            anim.SetTrigger("Air Dodge");
        }
        else
        {
            anim.SetTrigger("Air Slide");
            rb.velocity = new Vector2(0f, 0f);
        }
        rb.AddForce(new Vector2(Input.GetAxisRaw("Horizontal" + playerNum) * airDodgeForce, Input.GetAxisRaw("Vertical" + playerNum) * airDodgeForce));

        yield return new WaitForSeconds(airDodgeLength);
        p.hitbox.enabled = true; //Re-enabling boxCollider hitbox

    }

    //Function to flip player character horizontally
    public void PlayerFlip()
    {
        p.facingRight = !p.facingRight; //Flip the variable storing the direction character's facing
        transform.localScale = new Vector2(transform.localScale.x * -1, transform.localScale.y); //Use negative scale to 'flip' player character
    }
}
