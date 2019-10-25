using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileScript : MonoBehaviour {

    public GameObject shooter { get; set; }
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Invoke("DestroyProjectile", 0.5f); //Destroy Projectile after 1 second if it doesn't hit anything
	}

    //If the attack hits something, destroy it!
    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject == shooter)
        {
            return;
        }

        Destroy (this.gameObject);
    }

    void DestroyProjectile()
    {
        Destroy(this.gameObject);
    }

}
