using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    [System.Serializable] public struct PlayerUI {
        public GameObject life1;
        public GameObject life2;
        public GameObject life3;
        public GameObject life4;
        public Text playerName;
        public Text playerDamage;
        public Text playerNameShadow;
        public Text playerDamageShadow;
    };

    public PlayerUI player1UI;
    public PlayerUI player2UI;
    
	// Use this for initialization
	void Start () {
        GameManager.Instance.player1UI = player1UI;
        GameManager.Instance.player2UI = player2UI;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
