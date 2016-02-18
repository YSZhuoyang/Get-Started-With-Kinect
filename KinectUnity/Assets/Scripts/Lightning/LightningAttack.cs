using UnityEngine;
using System.Collections;

public class LightningAttack : MonoBehaviour
{
    private static ushort NUM_SEGMENTS = 12;
    private static float OFFSET_RANGE = 0.12f;
    private static float END_POINT_OFFSET = 0.5f;
    private static float INTERVAL = 0.05f;
    private static float LIFETIME = 1.3f;

    private GameObject lightningController;
    private LightningController lightningControllerScript;

    private float startTime;
    private float lastFrameTime;

    private Vector3 startPos;
    private Vector3 endPos;
    private Vector3 lightningVec;

    private LineRenderer lineRenderer;
    //private SpriteRenderer
    private GameObject[] lightnings;
    private Vector3[] points;

	// Use this for initialization
	void Start()
    {
        startTime = Time.time;
        lastFrameTime = 0f;
        
        points = new Vector3[NUM_SEGMENTS];
        startPos = new Vector3();
        endPos = new Vector3();
	}

    private void GenerateLightnings()
    {
        points[0] = startPos;
        points[NUM_SEGMENTS - 1] = endPos + new Vector3(
            Random.Range(-END_POINT_OFFSET, END_POINT_OFFSET),
            Random.Range(-END_POINT_OFFSET, END_POINT_OFFSET),
            Random.Range(-END_POINT_OFFSET, END_POINT_OFFSET));
        
        lightningVec = points[NUM_SEGMENTS - 1] - points[0];

        for (ushort i = 1; i < NUM_SEGMENTS - 1; i++)
        {
            points[i] = points[0] + i * lightningVec / 11f + new Vector3(
                Random.Range(-OFFSET_RANGE, OFFSET_RANGE),
                Random.Range(-OFFSET_RANGE, OFFSET_RANGE),
                Random.Range(-OFFSET_RANGE, OFFSET_RANGE));
        }

        GetComponent<LineRenderer>().SetPositions(points);
    }
	
	// Update is called once per frame
	void Update()
    {
        if (Time.time - startTime > LIFETIME)
        {
            Destroy(gameObject);

            lightningController = GameObject.Find("LightningController");

            if (lightningController == null)
            {
                print("Error: FireBallController game object not found");

                return;
            }

            lightningControllerScript = lightningController.GetComponent<LightningController>();

            if (lightningControllerScript == null)
            {
                print("Error: FireBallController script not found");

                return;
            }

            lightningControllerScript.SetLightningState(LightningController.LightningState.extinguished);
        }
        else if (Time.time - lastFrameTime > INTERVAL)
        {
            GenerateLightnings();
            lastFrameTime = Time.time;
        }
    }

    void OnCollisionEnter(Collision col)
    {
        /*fireBallController = GameObject.Find("FireBallController");

        if (fireBallController == null)
        {
            print("Error: FireBallController game object not found");

            return;
        }

        fireBallControllerScript = fireBallController.GetComponent<FireBallController>();

        if (fireBallControllerScript == null)
        {
            print("Error: FireBallController script not found");

            return;
        }

        fireBallControllerScript.SetFireBallState(FireBallController.FireBallState.extinguished);

        // Stop audio playing
        fireBallControllerScript.StopFlyingAudioPlaying();*/

        if (col.gameObject.tag == "Enemy")
        {
            Destroy(col.gameObject);

            // Trigger explosion
            //Instantiate(Resources.Load<GameObject>("Explosion"), col.transform.position, Quaternion.identity);
            Instantiate(Resources.Load<GameObject>("Debris"), col.transform.position, Quaternion.identity);
        }
        else
        {
            // Trigger explosion
            //Instantiate(Resources.Load<GameObject>("Explosion"), transform.position, Quaternion.identity);
        }
    }

    public void SetStartPosAndEndPos(Vector3 startPosIn, Vector3 endPosIn)
    {
        startPos = startPosIn;
        endPos = endPosIn;
    }
}