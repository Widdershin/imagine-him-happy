using UnityEngine;
using UnityEngine.InputSystem;

public class Spring3
{
    private Spring x;
    private Spring y;
    private Spring z;

    public Spring3(Vector3 value, float acceleration = 0.25f, float dampening = 0.98f)
    {
        x = new Spring(value.x, acceleration, dampening);  
        y = new Spring(value.y, acceleration, dampening);  
        z = new Spring(value.z, acceleration, dampening);
    }

    public void SetTarget(Vector3 target)
    {
        x.SetTarget(target.x);
        y.SetTarget(target.y);
        z.SetTarget(target.z);
    }

    public Vector3 Update(float deltaTime)
    {
        return new Vector3(x.Update(deltaTime), y.Update(deltaTime), z.Update(deltaTime));
    }
}
public class Spring
{
    public float value;
    public float acceleration;
    public float dampening;

    private float velocity;
    private float target;

    public Spring(float value, float acceleration = 0.25f, float dampening = 0.98f)
    {
        this.value = value;
        this.target = value;
        this.acceleration = acceleration;
        this.dampening = dampening;
        this.velocity = 0f;
    }

    public void SetTarget(float newTarget)
    {
        this.target = newTarget;
    }

    public float Update(float deltaTime)
    {
        var distance = this.target - this.value;

        this.velocity += this.acceleration * distance;
        this.velocity *= this.dampening;

        this.value += this.velocity * deltaTime;

        return this.value;
    }
}

public class PlayerController : MonoBehaviour
{
    public Vector3 playerVelocity = Vector3.zero;
    public Vector3 cameraFollowPoint = Vector3.zero;
    public CharacterController controller;
    public Transform ballTransform;
    public Transform bodyTransform;

    public Transform leftHand;
    public Transform rightHand;

    public Transform spawnPoint;

    public PlayerInput input;
    public float speed = 2.0f;
    public float jumpHeight = 1.0f;
    public float gravityValue = -9.81f;
    public float pushForce = 1f;
    public float sprintMoveMultiplier = 1.3f;
    public float sprintPushMultiplier = 1.5f;
    public float staminaInSeconds = 1f;
    public float stamina = 1f;
    public bool onGround;
    public bool rockAbove;
    public bool grabbing = false;
    public float squished = 0f;
    public float squishedDelay = 0f;

    public Spring squishSpring = new Spring(0f);

    public Rigidbody ballRigidBody;
    public float defaultBallAngularDrag;
    public float grabbedBallAngularDrag;

    public float handAcceleration = 0.25f;
    public float handDampening = 0.98f;

    private LayerMask sphereMask;
    private Spring3 leftHandSpring;
    private Spring3 rightHandSpring;
    private Vector3 originalLeftHandPosition;
    private Vector3 desiredLeftHandPosition;
    private Vector3 originalRightHandPosition;
    private Vector3 desiredRightHandPosition;

    // Start is called before the first frame update
    void Start()
    {
        if (spawnPoint != null)
        {
            controller.enabled = false;
            transform.position = spawnPoint.position + Vector3.up;
            controller.enabled = true;

            ballTransform.position = spawnPoint.position + Vector3.up * 4f;
        }

        cameraFollowPoint = ballTransform.position;
        originalLeftHandPosition = leftHand.localPosition;
        originalRightHandPosition = rightHand.localPosition;

        leftHandSpring = new Spring3(originalLeftHandPosition, handAcceleration, handDampening);
        rightHandSpring = new Spring3(originalRightHandPosition, handAcceleration, handDampening);

        sphereMask = ~LayerMask.NameToLayer("Sphere");
    }

    void OnForward()
    {
        playerVelocity.y = 1;
    }

    float ForwardInput()
    {
        return input.actions["Forward"].ReadValue<float>() - input.actions["Backward"].ReadValue<float>();
    }

    float SidewaysInput()
    {
        return input.actions["Right"].ReadValue<float>() - input.actions["Left"].ReadValue<float>();
    }

    bool JumpInput()
    {
        return input.actions["Jump"].ReadValue<float>() > 0.01f;
    }

    bool SprintInput()
    {
        return input.actions["Sprint"].ReadValue<float>() > 0.01f;
    }

    private void Update()
    {
        if (SprintInput())
        {
            stamina -= Time.deltaTime;
        }
        else
        {
            stamina += Time.deltaTime / 2f;
        }

        var squished = squishSpring.Update(Time.deltaTime);

        stamina = Mathf.Clamp(stamina, 0, staminaInSeconds);
        bodyTransform.localScale = new Vector3(1, 0.6159f - (0.6159f * squished / 1.5f), 1);
        controller.height = 1.25f - (1.25f * squished) / 1.5f;
        controller.radius = 0.36f - (0.36f * squished) / 1.5f;

        if (!rockAbove && squished > 0.99f)
        {
            squishSpring.SetTarget(0f);
        }


        leftHand.localPosition = leftHandSpring.Update(Time.deltaTime);
        rightHand.localPosition = rightHandSpring.Update(Time.deltaTime);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        onGround = controller.isGrounded;

        if (onGround && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        Vector3 move = ((transform.forward * ForwardInput()) + (transform.right * SidewaysInput() * 0.8f));

        if (SprintInput() && stamina > 0.1f)
        {
            move *= sprintMoveMultiplier;
        }

        playerVelocity += move * 0.3f;
        playerVelocity.x *= 0.95f;
        playerVelocity.z *= 0.95f;

        // Changes the height position of the player..
        if (false && JumpInput() && onGround)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        /*        cameraFollowPoint = Vector3.Lerp(cameraFollowPoint, ballTransform.position, 0.1f);*/
        cameraFollowPoint = ballTransform.position;
        controller.transform.LookAt(new Vector3(cameraFollowPoint.x, transform.position.y, cameraFollowPoint.z));

        grabbing = false;
        var origin = transform.position - transform.right;

        RaycastHit hitInfo;
        Ray ray = new Ray(origin, ballTransform.position - origin);

        if (Physics.Raycast(ray, out hitInfo, 1.3f, sphereMask))
        {
            leftHandSpring.SetTarget(transform.InverseTransformPoint(hitInfo.point));
            grabbing = true;
        }
        else
        {
            leftHandSpring.SetTarget(originalLeftHandPosition);
        }

        origin = transform.position + transform.right;

        ray = new Ray(origin, ballTransform.position - origin);

        if (Physics.Raycast(ray, out hitInfo, 1.3f, sphereMask))
        {
            rightHandSpring.SetTarget(transform.InverseTransformPoint(hitInfo.point));
            grabbing = true;
        }
        else
        {
            rightHandSpring.SetTarget(originalRightHandPosition);
        }

        if (grabbing)
        {
            ballRigidBody.angularDrag = grabbedBallAngularDrag;
        } else
        {
            ballRigidBody.angularDrag = defaultBallAngularDrag;
        }
    }


    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody rb = hit.collider.attachedRigidbody;

        if (rb != null && !rb.isKinematic)
        {
            var force = hit.moveDirection * pushForce * Time.deltaTime;

            if (SprintInput() && stamina > 0.1f)
            {
                force *= sprintPushMultiplier;
            }

            rb.AddForce(force, ForceMode.Force);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Boulder")
        {
            squishSpring.SetTarget(1f);
            rockAbove = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Boulder")
        {
            rockAbove = false;
        }
    }
}
