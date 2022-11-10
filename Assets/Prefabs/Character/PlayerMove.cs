using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour {

    [SerializeField] private float walkSpeed = 10f;
    // Start is called before the first frame update
    private Vector2 moveInput;
    private Rigidbody myRigidBody;
    void Start() {
        myRigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update() {
        Vector3 playerVelocity = new Vector3(moveInput.x * walkSpeed, myRigidBody.velocity.y, moveInput.y * walkSpeed);
        myRigidBody.velocity = transform.TransformDirection(playerVelocity);
    }

    void OnMove(InputValue value){
        moveInput = value.Get<Vector2>();
    }
}
