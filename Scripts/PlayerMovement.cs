using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public float defaultSpeed = 12f;
    public float moveSpeed;
    public float sprintMult = 1.3f;
    public float gravity = -9.81f;

    public Transform groundCheck; //position to check ground at
    public float groundRadius = 0.4f; //radius
    public float groundDistance = 1;
    public LayerMask groundMask;

    public bool isGrounded;

    Vector3 velocity;

    public float jumpHeight = 3f;
    public static PlayerMovement Instance { get; private set; }
    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself. 
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    void Update()
    {
        RaycastHit hit;
        //isGrounded = Physics.CheckSphere(groundCheck.position, groundRadius, groundMask);
        isGrounded = Physics.SphereCast(groundCheck.position, groundRadius, Vector3.down, out hit, groundDistance, groundMask);
        if (isGrounded)
        {
            Rigidbody body = hit.collider.GetComponentInParent<Rigidbody>();
            body.AddForceAtPosition(Vector3.down * 10, transform.position, ForceMode.Impulse);
        }
        if (isGrounded && velocity.y < 0)   
        {
            velocity.y = -2f; //negative to force onto ground
        } 

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;

        if (Input.GetKey(KeyCode.LeftShift)) {
            moveSpeed = defaultSpeed * sprintMult;
        }
        else
        {
            moveSpeed = defaultSpeed;
        }

        controller.Move(move * moveSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); //some physics equation for velocity needed to jump a height
        }

        //gravity
        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
}
