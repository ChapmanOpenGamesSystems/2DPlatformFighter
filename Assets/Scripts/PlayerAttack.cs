using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour {
    
    [System.Serializable] public struct Attack
    {
        public GameObject hitbox; //For melee: where attacks will hit.  For projectile: where projectiles will spawn
        public int hitFrame; //Frame that attack actually 
        public float attackTime;  //How long each attack takes(aka how long until you can attack again)
        public int attackDamage;
        public float knockbackAmount;
        public Vector2 knockbackDirection;
        public string name;
    }

    public Attack Bair;
    public Attack Fair;
    public Attack Uair;
    public Attack Dair;
    public Attack Nair;

    public Attack Ftilt;
    public Attack Utilt;
    public Attack Dtilt;
    public Attack Jab;

    [SerializeField] private Attack FSpecial;
    [SerializeField] private Attack USpecial;
    [SerializeField] private Attack DSpecial;
    [SerializeField] private Attack FASpecial;
    [SerializeField] private Attack UASpecial;
    [SerializeField] private Attack DASpecial;

    [SerializeField] private GameObject Grab;
    [SerializeField] private Attack ForwardThrow;
    [SerializeField] private Attack UpThrow;
    [SerializeField] private Attack DownThrow;
    [SerializeField] private Attack BackThrow;

    [SerializeField] private GameObject projectile;

    private Player p;
    private PlayerMovement pmove;
    private BoxCollider2D hitbox;

    private int maxColliders = 4; //Can only hit 4 players (including yourself)

    [SerializeField] private float projectileSpeed = 10f;
    public float projectileKnockback = 500f;
    public int projectileDamage = 5;

    private float grabTime = 0.2f;

    private Vector2 FSpecialDirection = new Vector2(1, 0);
    private Vector2 USpecialDirection = new Vector2(0, 1);
    private Vector2 DSpecialDirection = new Vector2(0, -1);
    private Vector2 FASpecialDirection = new Vector2(1,-1);
    private Vector2 UASpecialDirection = new Vector2(0, 1);
    private Vector2 DASpecialDirection = new Vector2(0, -1);

    private Animator anim;


    // Use this for initialization
    void Start() {
        p = GetComponent<Player>();
        pmove = GetComponent<PlayerMovement>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update() {
        if(anim.GetCurrentAnimatorStateInfo(0).IsName("Grabbing"))
        {
            Transform[] transformList = this.GetComponentsInChildren<Transform>();
            GameObject grabbedPlayer = null;
            foreach(Transform t in transformList)
            {
                if(t.gameObject.tag == "Player")
                {
                    grabbedPlayer = t.gameObject;
                }
            }

            if(grabbedPlayer != null)
            {
                if (Input.GetButtonDown("Vertical"))
                {
                    if (Input.GetAxisRaw("Vertical") > 0)
                    {
                        Debug.Log("Up Throw");
                        anim.SetTrigger("UThrow");
                        grabbedPlayer.GetComponent<Player>().PlayerStagger(UpThrow);
                        StartCoroutine(ReleasePlayer(grabbedPlayer));
                    }
                    else if (Input.GetAxisRaw("Vertical") < 0)
                    {
                        Debug.Log("Down Throw");
                        anim.SetTrigger("DThrow");
                        grabbedPlayer.GetComponent<Player>().PlayerStagger(DownThrow);
                        StartCoroutine(ReleasePlayer(grabbedPlayer));
                    }
                }
                if (Input.GetButtonDown("Horizontal"))
                {
                    Attack tempThrow = new Attack(); //Using tempThrow as deep copy to change knockback direction without affecting original
                    if (Input.GetAxisRaw("Horizontal") > 0 && p.facingRight || Input.GetAxisRaw("Horizontal") < 0 && !p.facingRight)
                    {
                        Debug.Log("Forward Throw");
                        anim.SetTrigger("FThrow");
                        tempThrow = ForwardThrow;
                        if (!GetComponent<Player>().facingRight)
                        {
                            tempThrow.knockbackDirection = new Vector2(tempThrow.knockbackDirection.x * -1, tempThrow.knockbackDirection.y);
                        }
                        grabbedPlayer.GetComponent<Player>().PlayerStagger(tempThrow);
                        StartCoroutine(ReleasePlayer(grabbedPlayer));
                    }
                    else if (Input.GetAxisRaw("Horizontal") < 0 && p.facingRight || Input.GetAxisRaw("Horizontal") > 0 && !p.facingRight)
                    {
                        Debug.Log("Back Throw");
                        anim.SetTrigger("BThrow");
                        tempThrow = BackThrow;
                        if (!GetComponent<Player>().facingRight)
                        {
                            tempThrow.knockbackDirection = new Vector2(tempThrow.knockbackDirection.x * -1, tempThrow.knockbackDirection.y);
                        }
                        GetComponent<PlayerMovement>().PlayerFlip();
                        grabbedPlayer.GetComponent<Player>().PlayerStagger(tempThrow);
                        StartCoroutine(ReleasePlayer(grabbedPlayer));
                    }
                }

            }
            
        }

        if (Input.GetButtonDown("Attack")) //As long as they aren't in an animation, pressing attack will launch an attack
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Movement") && p.isGrounded)
            {
                GroundedAttack();
            }

            else if ((anim.GetCurrentAnimatorStateInfo(0).IsName("Movement") || anim.GetCurrentAnimatorStateInfo(0).IsName("Jump")) && !p.isGrounded)
            {
                AerialAttack();
            }
        }
        //You can't make multiple inputs at once, so else-if suite is optimal
        else if (Input.GetButtonDown("Special") && (anim.GetCurrentAnimatorStateInfo(0).IsName("Movement") || anim.GetCurrentAnimatorStateInfo(0).IsName("Jump"))) //As long as they aren't in an animation other than jump, pressing attack will launch an attack
        {
            SpecialAttack();
        }

        else if (Input.GetButtonDown("Grab") && anim.GetCurrentAnimatorStateInfo(0).IsName("Movement") && p.isGrounded)
        {
            StartCoroutine(GrabPlayer());            
        }

        
    }


    /*~~~~~~~~~~~~~~ ALL THINGS BELOW NEED TO BE WORKED ON ~~~~~~~~~~~~~~~~~~~~*/
    //Use Start() and Update() as needed
    //For attacks that are reliant on movement, make the necessary calls to PlayerMovement

    //Function for managing all attacks, should make calls to the attack functions below

    //Nic's Edit: All attack functions call this as a coroutine with hitboxes and delay as parameters
    IEnumerator AttackCalled(Attack attack)
    {
        CircleCollider2D hitbox = attack.hitbox.GetComponent<CircleCollider2D>();
        for (int frameCount = attack.hitFrame;  frameCount > 1; --frameCount)
        {
            --frameCount;
            yield return new WaitForEndOfFrame();   
        }
        hitbox.enabled = true;
        yield return new WaitForSeconds(attack.attackTime); //TODO: Have this wait until the attack's respective animation is over
        hitbox.enabled = false;
    }

    //Function for ground attacks
    void GroundedAttack()
    {
        //Btilt isn't a thing, can only be executed by turning around and ftilting.
        if (Input.GetAxisRaw("Horizontal") < 0 && !p.facingRight || Input.GetAxisRaw("Horizontal") > 0 && p.facingRight) //Ftilt
        {
            anim.SetTrigger("Ftilt");
            StartCoroutine(AttackCalled(Ftilt));
        }
        else if (Input.GetAxisRaw("Vertical") > 0) //Utilt
        {
            anim.SetTrigger("Utilt");
            StartCoroutine(AttackCalled(Utilt));
        }
        else if (Input.GetAxisRaw("Vertical") < 0) //Dtilt
        {
            anim.SetTrigger("Dtilt");
            StartCoroutine(AttackCalled(Dtilt));
        }
        else //Jab if attack is called and no directional attack is selected
        {
            anim.SetTrigger("Jab");
            StartCoroutine(AttackCalled(Jab));
        }

    }

    //Function for aerial attack
    void AerialAttack()
    {
        if (Input.GetAxisRaw("Horizontal") < 0 && p.facingRight || Input.GetAxisRaw("Horizontal") > 0 && !p.facingRight) //Backair
        {
            anim.SetTrigger("Bair");
            StartCoroutine(AttackCalled(Bair));
        }
        else if (Input.GetAxisRaw("Horizontal") < 0 && !p.facingRight || Input.GetAxisRaw("Horizontal") > 0 && p.facingRight) //Fair
        {
            anim.SetTrigger("Fair");
            StartCoroutine(AttackCalled(Fair));
        }
        else if (Input.GetAxisRaw("Vertical") > 0) //Up air
        {
            anim.SetTrigger("Uair");
            StartCoroutine(AttackCalled(Uair));
        }
        else if (Input.GetAxisRaw("Vertical") < 0) //Dair
        {
            anim.SetTrigger("Dair");
            StartCoroutine(AttackCalled(Dair));
        }
        else //Nair if attack is called and no directional attack is selected
        {
            anim.SetTrigger("Nair");
            StartCoroutine(AttackCalled(Nair));
        }
    }

    //Function for special attack
    void SpecialAttack()
    {
        //Need backwards side special for when you're moving in the air and using a side special (It needs to flip you)
        if (Input.GetAxisRaw("Horizontal") < 0 && p.facingRight || Input.GetAxisRaw("Horizontal") > 0 && !p.facingRight) //Side Special (Backwards)
        {
            pmove.PlayerFlip(); //Need to flip them if they're facing backwards
            //Aerial Special
            anim.SetTrigger("FASpecial");
            StartCoroutine(SpawnProjectile(FASpecial, FASpecialDirection)); //Flipped FASpecial direction
            //Can't possibly be grounded
        }
        else if (Input.GetAxisRaw("Horizontal") < 0 && !p.facingRight || Input.GetAxisRaw("Horizontal") > 0 && p.facingRight) //Side Special (Forwards)
        {

            if (!p.isGrounded)
            {
                //Aerial Special
                anim.SetTrigger("FASpecial");
                StartCoroutine(SpawnProjectile(FASpecial, FASpecialDirection));
            }
            else
            {
                //Grounded Special
                anim.SetTrigger("FSpecial");
                StartCoroutine(SpawnProjectile(FSpecial, FSpecialDirection));
            }

        }
        else if (Input.GetAxisRaw("Vertical") > 0) //Up Special
        {

            if (!p.isGrounded)
            {
                //Aerial Special
                anim.SetTrigger("UASpecial");
                StartCoroutine(SpawnProjectile(UASpecial, UASpecialDirection));
            }
            else
            {
                //Grounded Special
                anim.SetTrigger("USpecial");
                StartCoroutine(SpawnProjectile(USpecial, USpecialDirection));
            }

        }
        else if (Input.GetAxisRaw("Vertical") < 0) //Down Special
        {

            if (!p.isGrounded)
            {
                //Aerial Special
                anim.SetTrigger("DASpecial");
                StartCoroutine(SpawnProjectile(DASpecial, DASpecialDirection));
            }
            else
            {
                //Grounded Special
                anim.SetTrigger("DSpecial");
                StartCoroutine(SpawnProjectile(DSpecial, DSpecialDirection));
            }

        }
        else //Neutral Special if attack is called and no directional attack is selected
        {
            Debug.Log("Neutral Special");
            //TODO: Implement Neutral special

            if (!p.isGrounded)
            {
                //Aerial Special
            }
            else
            {
                //Grounded Special
            }

        }

    }

    //Function for grapping opponent
    IEnumerator GrabPlayer()
    {
        anim.SetTrigger("Grab");
        //Push and Pop of Grabbing are handled in HitDetection
        CircleCollider2D grabBox = Grab.GetComponent<CircleCollider2D>();
        grabBox.enabled = true;
        yield return new WaitForSeconds(grabTime);
        grabBox.enabled = false;
    }

    //Function for releasing opponent after throw
    IEnumerator ReleasePlayer(GameObject grabbedPlayer)
    {
        yield return new WaitForEndOfFrame(); //Ensures throw direction
        grabbedPlayer.transform.parent = null;
        grabbedPlayer.GetComponent<Rigidbody2D>().simulated = true;
        anim.SetTrigger("Grab Release");
        //No need to allow grabbed player to escape since they'll go directly to stagger
    }

    //Spawns a Projectile at the given Attacks location
    IEnumerator SpawnProjectile(Attack specialAttack, Vector2 direction)
    {
        for (int frameCount = specialAttack.hitFrame; frameCount > 1; --frameCount)
        {
            --frameCount;
            yield return new WaitForEndOfFrame();
        }
        GameObject projectileClone = Instantiate(projectile);
        projectileClone.GetComponent<ProjectileScript>().shooter = this.gameObject;
        projectileClone.transform.position = specialAttack.hitbox.transform.position;

        Rigidbody2D rb = projectileClone.GetComponent<Rigidbody2D>();

        if (p.facingRight) //If they're not facing right, everything is flipped!
        {
            rb.velocity = direction * projectileSpeed;
        }
        else
        {
            rb.velocity = new Vector2(direction.x * -1, direction.y) * projectileSpeed;
        }

        yield return new WaitForSeconds(specialAttack.attackTime);

    }
}
