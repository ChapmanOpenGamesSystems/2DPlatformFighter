using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawnpoint : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GameManager.Instance.spawnpoint = this.transform;
	}
	
}
