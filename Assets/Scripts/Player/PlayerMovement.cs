using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 moveDirection;
    public float Speed;
    public float jumpforce;
    private bool isGrounded;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        moveDirection.x = (int)Input.GetAxisRaw("Horizontal");
        if(Input.GetKey(KeyCode.W) && isGrounded)
        {
            moveDirection.y = 2;
        }
        else
        {
            moveDirection.y = 0;

        }

    }
    private void FixedUpdate()
    {
        rb.velocity = new Vector2(moveDirection.x*Speed, rb.velocity.y);
        if(moveDirection == Vector2.zero)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        if(moveDirection.y != 0 && isGrounded)
        {
            rb.AddForce(moveDirection * jumpforce, ForceMode2D.Impulse);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        isGrounded = true;

    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        isGrounded = false;
    }
}
