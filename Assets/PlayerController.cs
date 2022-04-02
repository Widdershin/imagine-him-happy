using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Vector3 playerVelocity = Vector3.zero;
    public Vector3 cameraFollowPoint = Vector3.zero;
    public CharacterController controller;
    public Transform ballTransform;

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
    public bool grabbing = false;

    public Rigidbody ballRigidBody;
    public float defaultBallAngularDrag;
    public float grabbedBallAngularDrag;

    private LayerMask sphereMask;

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

            ballTransform.position = spawnPoint.position + Vector3.up + Vector3.forward;
        }

        cameraFollowPoint = ballTransform.position;
        originalLeftHandPosition = leftHand.localPosition;
        originalRightHandPosition = rightHand.localPosition;
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
            if (SidewaysInput() < 0.01f && ForwardInput() < 0.01f)
            {
                stamina += Time.deltaTime / 2f;
            }
        }

        stamina = Mathf.Clamp(stamina, 0, staminaInSeconds);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        onGround = controller.isGrounded;

        if (onGround && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        Vector3 move = new Vector3(SidewaysInput() * 0.8f, 0, ForwardInput());

        if (SprintInput() && stamina > 0.1f)
        {
            move *= sprintMoveMultiplier;
        }

        controller.Move(transform.rotation * (move * Time.deltaTime * speed));

        // Changes the height position of the player..
        if (false && JumpInput() && onGround)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        cameraFollowPoint = Vector3.Lerp(cameraFollowPoint, ballTransform.position, 0.1f);
        controller.transform.LookAt(new Vector3(cameraFollowPoint.x, transform.position.y, cameraFollowPoint.z));

        grabbing = false;
        var origin = transform.position - transform.right;

        RaycastHit hitInfo;
        Ray ray = new Ray(origin, ballTransform.position - origin);

        if (Physics.Raycast(ray, out hitInfo, 1.3f, sphereMask))
        {
            desiredLeftHandPosition = transform.InverseTransformPoint(hitInfo.point);
            grabbing = true;
        }
        else
        {
            desiredLeftHandPosition = originalLeftHandPosition;
        }

        origin = transform.position + transform.right;

        ray = new Ray(origin, ballTransform.position - origin);

        if (Physics.Raycast(ray, out hitInfo, 1.3f, sphereMask))
        {
            desiredRightHandPosition = transform.InverseTransformPoint(hitInfo.point);
            grabbing = true;
        }
        else
        {
            desiredRightHandPosition = originalRightHandPosition;
        }

        if (grabbing)
        {
            ballRigidBody.angularDrag = grabbedBallAngularDrag;
        } else
        {
            ballRigidBody.angularDrag = defaultBallAngularDrag;
        }

        leftHand.localPosition = Vector3.Lerp(leftHand.localPosition, desiredLeftHandPosition, 0.1f);
        rightHand.localPosition = Vector3.Lerp(rightHand.localPosition, desiredRightHandPosition, 0.1f);
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
}
