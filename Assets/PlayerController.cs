using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Vector3 playerVelocity = Vector3.zero;
    public CharacterController controller;
    public Transform ballTransform;

    public PlayerInput input;
    public float speed = 2.0f;
    public float jumpHeight = 1.0f;
    public float gravityValue = -9.81f;
    public float pushForce = 1f;
    public bool onGround;
    // Start is called before the first frame update
    void Start()
    {
        
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

    // Update is called once per frame
    void Update()
    {
        onGround = controller.isGrounded;

        if (onGround && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        Vector3 move = new Vector3(SidewaysInput(), 0, ForwardInput());
        controller.Move(transform.rotation * (move * Time.deltaTime * speed));

        controller.transform.LookAt(new Vector3(ballTransform.position.x, transform.position.y, ballTransform.position.z));

        // Changes the height position of the player..
        if (JumpInput() && onGround)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody rb = hit.collider.attachedRigidbody;

        if (rb != null && !rb.isKinematic)
        {
            rb.velocity = hit.moveDirection * pushForce;
        }
    }
}
