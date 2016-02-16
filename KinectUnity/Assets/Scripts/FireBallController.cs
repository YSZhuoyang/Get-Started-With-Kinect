using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class FireBallController : MonoBehaviour
{
    private static float FLYING_MAX_DURATION = 2f;
    private static float SHOOTING_GESTURE_MAX_DURATION = 0.4f;
    private static float SHOOTING_GESTURE_MAX_DISTANCE = 0.4f;

    private float flyingStartTime;
    private float shootingGestureStartTime;

    private GameObject fireBallEffect;
    private ParticleSystem.Particle[] trailerParticles;

    private Vector3 fireBallPreLoc;
    private Vector3 fireBallCurrLoc;
    private Vector3 currVelocity;

    private Vector3 shootingGestureStartPos;
    private Vector3 shootingGestureEndPos;

    private Vector3 rightHandPosition;
    private Vector3 rightElbowPosition;

    private HandState handRightState;
    private FireBallState fireBallState;

    private bool shootingFireBallGestureDetected;

    public enum FireBallState
    {
        flying, holding, extinguished
    }

    // Use this for initialization
    void Start()
    {
        rightHandPosition = new Vector3();
        rightElbowPosition = new Vector3();

        currVelocity = new Vector3();
        fireBallPreLoc = new Vector3();
        fireBallCurrLoc = new Vector3();
        
        fireBallState = FireBallState.extinguished;
        handRightState = HandState.Closed;

        shootingFireBallGestureDetected = false;
    }

    private void EnableFireBall()
    {
        if (!FireBallEnabled() &&
            fireBallState == FireBallState.extinguished)
        {
            fireBallEffect = Instantiate(Resources.Load<GameObject>("FireBallEffect"));
            fireBallState = FireBallState.holding;

            fireBallEffect.transform.position = rightHandPosition;
            fireBallCurrLoc = rightHandPosition;
            fireBallPreLoc = rightHandPosition;
            currVelocity = new Vector3(0f, 0f, 0f);
        }
    }

    private void DisableFireBall()
    {
        if (FireBallEnabled() &&
            fireBallState != FireBallState.extinguished)
        {
            Destroy(fireBallEffect);
            fireBallState = FireBallState.extinguished;
        }
    }

    private void ResponseToGesture()
    {
        if (rightHandPosition.y > rightElbowPosition.y && 
            handRightState == HandState.Open && 
            fireBallState == FireBallState.extinguished)
        {
            EnableFireBall();
        }

        if ((rightElbowPosition.y > rightHandPosition.y || handRightState == HandState.Closed) && 
            fireBallState == FireBallState.holding)
        {
            DisableFireBall();
        }
        
        if (FireBallEnabled() && 
            fireBallState == FireBallState.holding && 
            handRightState == HandState.Open)
        {
            float fireBallSpeed = Mathf.Sqrt(
                currVelocity.x * currVelocity.x +
                currVelocity.y * currVelocity.y +
                currVelocity.z * currVelocity.z
                );

            if (fireBallSpeed > 5)
            {
                if (!shootingFireBallGestureDetected)
                {
                    shootingFireBallGestureDetected = true;
                    shootingGestureStartTime = Time.time;
                    shootingGestureStartPos = rightHandPosition;
                }
                else if (Time.time - shootingGestureStartTime < SHOOTING_GESTURE_MAX_DURATION)
                {
                    shootingGestureEndPos = rightHandPosition;

                    Vector3 movingVec = shootingGestureEndPos - shootingGestureStartPos;
                    float movingLength = Mathf.Sqrt(
                        movingVec.x * movingVec.x +
                        movingVec.y * movingVec.y +
                        movingVec.z * movingVec.z);

                    if (movingLength > SHOOTING_GESTURE_MAX_DISTANCE)
                    {
                        fireBallState = FireBallState.flying;
                        flyingStartTime = Time.time;
                        shootingFireBallGestureDetected = false;
                    }
                }
                else if (Time.time - shootingGestureStartTime > SHOOTING_GESTURE_MAX_DURATION)
                {
                    shootingFireBallGestureDetected = false;
                }
            }
        }
    }
    
    private void ComputeVelocity()
    {
        // Compute fireBall moving velocity
        fireBallCurrLoc = rightHandPosition;
        currVelocity = (fireBallCurrLoc - fireBallPreLoc) / Time.deltaTime;
        fireBallPreLoc = fireBallCurrLoc;
    }

    private bool FireBallEnabled()
    {
        return GameObject.Find("FireBallEffect(Clone)") != null;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        ResponseToGesture();

        if (fireBallState == FireBallState.holding)
        {
            ComputeVelocity();

            // Update fire ball position based on right hand position
            fireBallEffect.transform.position = fireBallCurrLoc;
        }
        else if (fireBallState == FireBallState.flying)
        {
            if (Time.time - flyingStartTime > FLYING_MAX_DURATION)
            {
                // Trigger explosion
                Instantiate(Resources.Load<GameObject>("Explosion"), fireBallEffect.transform.position, Quaternion.identity);
                DisableFireBall();
            }
            else
            {
                // Fire ball flying
                fireBallEffect.transform.position += currVelocity * Time.deltaTime;
            }
        }
    }

    public void SetFireBallState(FireBallState fireBallStateIn)
    {
        fireBallState = fireBallStateIn;
    }

    public void SetGestures(
        Vector3 rightHandPositionIn,
        Vector3 rightElbowPositionIn,
        HandState handRightStateIn)
    {
        rightHandPosition = rightHandPositionIn;
        rightElbowPosition = rightElbowPositionIn;
        handRightState = handRightStateIn;
    }

    public Vector3 GetVelocity()
    {
        return currVelocity;
    }
}
