using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

//TODO: ADD RESPAWN LOGIC FOR PLAYER SCORE BUT NOT DAMAGE

public class GameManager : MonoBehaviour {

    public enum Gamemode {
        STOCK,
        STAMINA,
        TIME
    }

    [System.Serializable] public enum Stage {
        PIAZZA,
        LAWN,
        LIBRARY,
        NONE
    }

    public Transform spawnpoint;
    //Separate prefabs because different control bindings/player specific details
    public GameObject player1Prefab;
    public GameObject player2Prefab;
    public GameObject[] players; //Array of all player GameObjects in Scene
    public static GameManager Instance;
    public Gamemode gamemode;
    public Stage stage;
    public GameObject stageCanvas;
    public GameObject gamemodeCanvas;

    // Use this for initialization
    void Start() {
        Instance = this;
        DontDestroyOnLoad(this);
    }

    public void SetStage(string stage) {
        if(stage.Equals("PIAZZA")) {
            this.stage = Stage.PIAZZA;
        } else if(stage.Equals("LAWN")) {
            this.stage = Stage.LAWN;
        } else if(stage.Equals("LIBRARY")) {
            this.stage = Stage.LIBRARY;
        } else {
            this.stage = Stage.NONE;
        }
        stageCanvas.SetActive(false);
        gamemodeCanvas.SetActive(true);
    }

    public void SetGamemode(string gamemode) {
        //Using string parameter instead of Gamemode parameter because Unity doesn't seem to support enum parameters on button clicks
        switch (stage) {
            case Stage.PIAZZA:
                SceneManager.LoadScene("Piazza");
                break;
            case Stage.LAWN:
                SceneManager.LoadScene("Lawn");
                break;
            case Stage.LIBRARY:
                SceneManager.LoadScene("Library");
                break;
            case Stage.NONE:
                SceneManager.LoadScene("Sandbox");
                break;
        }
        StartCoroutine(InitializeGame(gamemode));
    }

    public void PlayerKill(int playerNum)
    {
        Debug.Log("Player " + playerNum + " killed");
        GameObject playerToKill = Array.Find(players, player => player.GetComponent<Player>().playerNum == playerNum);
        switch (gamemode)
        {
            case Gamemode.STOCK:
                playerToKill.GetComponent<Player>().playerScore -= 1;
                if (playerToKill.GetComponent<Player>().playerScore <= 0)
                {
                    Debug.Log("Player " + playerNum + " eliminated");
                }
                else
                {
                    PlayerRespawn(playerNum, playerToKill.GetComponent<Player>().playerScore);
                }
                break;
            case Gamemode.TIME:
                //Going to need some sort of logic to determine whether it was an SD or a kill; possibly attaching "lastHit" as a player number and -1 when not hit, refreshing when on the ground
                //TODO: Add Score logic for time
                int lastHit = playerToKill.GetComponent<Player>().lastHit;
                Debug.Log("Last hit: Player " + lastHit); //NOTE: This will always be 0, since last hit is reset when the player is on the ground when PlayerMove is attached until knockback is added.
                if(lastHit == 0)
                {
                    playerToKill.GetComponent<Player>().playerScore -= 2; //SD is -2
                }
                else
                {
                    playerToKill.GetComponent<Player>().playerScore -= 1;  //-1 from dying
                    GameObject playerKiller = Array.Find(players, player => player.GetComponent<Player>().playerNum == lastHit);
                    playerKiller.GetComponent<Player>().playerScore += 1; //+1 from killing
                }
                PlayerRespawn(playerNum, playerToKill.GetComponent<Player>().playerScore);
                break;
            case Gamemode.STAMINA:
                //Stamina score represents stock, so there can be more than one death if the rules are set to have more than one stock
                playerToKill.GetComponent<Player>().playerScore -= 1;
                if (playerToKill.GetComponent<Player>().playerScore <= 0)
                {
                    Debug.Log("Player " + playerNum + " eliminated");
                }
                else
                {
                    PlayerRespawn(playerNum, playerToKill.GetComponent<Player>().playerScore);
                }
                break;
            default:
                //Empty default case just in case
                Debug.Log("Default case reached in GameManager.PlayerKill()");
                break;
        }
        Destroy(playerToKill);
        StartCoroutine(RefreshPlayers());
    }

    public void PlayerRespawn(int playerNum, int playerScore)
    {
        GameObject newPlayer;
        //TODO: Replace this once controls are figured out using a single prefab
        if(playerNum == 1 )
        {
            newPlayer = Instantiate(player1Prefab);
        }
        else
        {
            newPlayer = Instantiate(player2Prefab);
        }
        if (gamemode == Gamemode.STAMINA)
        {
            newPlayer.GetComponent<Player>().playerDamage = 100;
        }
        else
        {
            newPlayer.GetComponent<Player>().playerDamage = 0;
        }
        newPlayer.transform.position = spawnpoint.position;
        newPlayer.GetComponent<Player>().playerNum = playerNum;
        newPlayer.GetComponent<Player>().playerScore = playerScore;
        newPlayer.GetComponent<Player>().lastHit = 0;

    }

    public void PlayerDamage(int playerNum, int damageValue)
    {
        Debug.Log(playerNum);
        GameObject playerToDamage = Array.Find(players, player => player.GetComponent<Player>().playerNum == playerNum);
        if(gamemode == Gamemode.STAMINA)
        {
            playerToDamage.GetComponent<Player>().playerDamage -= damageValue;
            if (playerToDamage.GetComponent<Player>().playerDamage <= 0)
            {
                PlayerKill(playerNum);
            }
        }
        else
        {
            playerToDamage.GetComponent<Player>().playerDamage += damageValue;
        }
    }

    public IEnumerator InitializeGame(string gamemode)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame(); //Wait for LoadScene to be complete before beginning Initialization, which completes DURING the next frame, so we have to skip 2
        players = GameObject.FindGameObjectsWithTag("Player"); //Updates players when intializing game
        if (gamemode.Equals("STOCK"))
        {
            this.gamemode = Gamemode.STOCK;
            //In STOCK, playerScore is stock count and playerDamage is percent
            foreach (GameObject player in players)
            {
                player.GetComponent<Player>().playerScore = 3; //3 Stock
                player.GetComponent<Player>().playerDamage = 0; //0% initial
            }
        }
        else if (gamemode.Equals("STAMINA"))
        {
            this.gamemode = Gamemode.STAMINA;
            //In STAMINA, playerScore is stock and playerDamage is health
            foreach (GameObject player in players)
            {
                player.GetComponent<Player>().playerScore = 3; //3 Stock
                player.GetComponent<Player>().playerDamage = 100; //100 HP initial
            }
        }
        else if (gamemode.Equals("TIME"))
        {
            this.gamemode = Gamemode.TIME;
            //In TIME, playerScore is score while playerDamage is percent
            foreach (GameObject player in players)
            {
                player.GetComponent<Player>().playerScore = 0; //0 Score
                player.GetComponent<Player>().playerDamage = 0; //0% initial
            }
        }
    }

    public IEnumerator RefreshPlayers()
    {
        yield return new WaitForEndOfFrame(); //Destroy only occurs at the end of the update loop, so we have to wait for the next frame before updating the players array
        players = GameObject.FindGameObjectsWithTag("Player"); //Updates GameObject[] to account for players respawned
    }
}
