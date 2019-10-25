using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CameraMovement : MonoBehaviour {


    // VARIABLES //
    private GameObject gameManager;
    private GameManager gm;

    private new Transform transform;
    private List<Transform> playerTransforms;
    private Vector3 desiredPos;
    //Used for clamping the transform of the camera
    public Vector2 MaxXAndY;
    public Vector2 MinXAndY;

    private Camera cam;
    public float camSpeed;


    // METHODS //

    private void Awake()
    {
        transform = GetComponent<Transform>();
        cam = GetComponent<Camera>();  
    }


    private void Start ()
    {
        gameManager = GameObject.Find("GameManager");
        gm = gameManager.GetComponent<GameManager>();
        SetPlayerList();
    }
	

	private void Update ()
    {
        SetPlayerList();
        CalculateTransform();
        CalculateSize();
        ClampPosition(MinXAndY, MaxXAndY);
    }


    private void LateUpdate()
    {
        //Updates the camera's position
        transform.position = Vector3.MoveTowards(transform.position, desiredPos, camSpeed);

        //Keeps the camera's size within the constraints
        if (cam.orthographicSize < 9)
        {
            cam.orthographicSize = 9;
        }
        else if (cam.orthographicSize > 15)
        {
            cam.orthographicSize = 15;
        }
    }


    //Method to find largest and lowest x and y values
    private void CalculateTransform()
    {
        desiredPos = Vector3.zero;
        float distance = 0f;

        //Obtains the largest X and Y coordinates of the players
        var camPosY = GetMaxY();
        var camPosX = GetMaxX();

        var distanceY = -(camPosY + 5f) * 0.5f / Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        var distanceX = -(camPosX / cam.aspect + 5f) * 0.5f / Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);

        distance = distanceY < distanceX ? distanceY : distanceX;

        for (int i = 0; i < playerTransforms.Count; i++)
        {
            desiredPos += playerTransforms[i].position;
        }

        if (distance > -10f)
        {
            distance = -10f;
        }

        desiredPos /= playerTransforms.Count;
        desiredPos.z = distance;
    }

    //Calculates what size the camera should be in relation to the player's transforms
    private void CalculateSize()
    {
        float maxDifX = GetMaxX();
        float maxDifY = GetMaxY();


        if (maxDifX >= maxDifY)
        {
            cam.orthographicSize = (maxDifX / 2) + 3;
        }
        else if (maxDifY > maxDifX)
        {
            cam.orthographicSize = (maxDifY / 2) + 3;
        }
        else
        {
            Debug.Log("Something went wrong while attempting to change the cameras size.");
        }
    }

    //Obtains the largest X value from a list of player's transforms
    private float GetMaxX()
    {
        var xSort = playerTransforms.OrderByDescending(p => p.position.x);
        var maxX = xSort.First().position.x - xSort.Last().position.x;

        return maxX;
    }

    //Obtains the largest Y value from a list of player's transforms
    private float GetMaxY()
    {
        var ySort = playerTransforms.OrderByDescending(p => p.position.y);
        var maxY = ySort.First().position.y - ySort.Last().position.y;

        return maxY;
    }

    //Prevents the camera from moving beyond the given parameters
    private void ClampPosition(Vector2 MinXAndY, Vector2 MaxXAndY)
    {
        var clampedValues = transform.position;

        clampedValues.x = Mathf.Clamp(transform.position.x, MinXAndY.x, MaxXAndY.x);
        clampedValues.y = Mathf.Clamp(transform.position.y, MinXAndY.y, MaxXAndY.y);

        transform.position = clampedValues;
    }

    //Grabs the transforms of the player's and puts it in a list
    private void SetPlayerList()
    {
        playerTransforms = new List<Transform>();
        for (int i = 0; i < gm.players.Length; i++)
        {
            playerTransforms.Add(gm.players[i].GetComponent<Transform>());
        }
    }
}
