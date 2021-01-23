using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class PlayerMovement : MonoBehaviour
{
    public bool hasTorch;
    public float SwimmingLevel;
    public GameObject JumpSmoke;
    public FluidPhysics fluidManager;
    public Light DefaultLight;
    public SoundManager sfxManager;
    private Rigidbody2D rb;
    public Vector2 moveDirection;
    private Animator animator;
    public float Speed;
    public float jumpforce;
    public bool isGrounded;
    public bool isSwimming;
    public bool isClimbing;
    public bool FreeFalling;
    private float initialGravityScale;
    private LightEmitting lEmit;
    private Vector3Int offset;
    private Vector3Int currentPos;
    private Vector2 Flow;
    private SpriteRenderer sp;
    public TriggerManager tManager;
    public Tilemap Triggers;
    private float jumpTime;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        initialGravityScale = rb.gravityScale;
        offset = fluidManager.ObstacleField.origin;
        animator = GetComponentInChildren<Animator>();
        sp = GetComponentInChildren<SpriteRenderer>();
    }
    private void Update()
    {
        currentPos = Vector3Int.FloorToInt(transform.position);
        currentPos.z = 0;
        moveDirection.x = Input.GetAxisRaw("Horizontal");
        if((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space)) && (isGrounded && !isSwimming))
        {
            moveDirection.y = 2;
        }
        else if (!(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space)))
        {
            moveDirection.y = 0;

        }
        if((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space)) && isClimbing)
        {
            moveDirection.y = 1;

        }
        if ((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space)) && isSwimming)
        {
            moveDirection.y = 1;
        }
        else if (!(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space)) && isSwimming)
        {
            moveDirection.y = 0;

        }
        if ((Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.LeftShift)) && isSwimming)
        {
            moveDirection.y = -1;

        }
    }
    private void FixedUpdate()
    {
        if(!isGrounded)
        {
            jumpTime += Time.fixedDeltaTime;
        }
        if (moveDirection.y != 0 && (isGrounded || isSwimming) && !isClimbing)
        {
            if (!isSwimming)
            {
                rb.AddForce(moveDirection * jumpforce, ForceMode2D.Impulse);
                sfxManager.PlaySound(SoundEffect.SoundEvent.onJump);
            }
            else
            {
                Vector3Int el = currentPos - offset;
                if ((float)fluidManager.fluidField[el.x, el.y].Level / fluidManager.MaximumLevel >= SwimmingLevel)
                {
                    float jumpWater = Mathf.Abs(jumpforce * rb.gravityScale);
                    Vector2 jumpingVector = new Vector2(Flow.x, moveDirection.y * jumpWater).normalized;
                    rb.AddForce(jumpingVector, ForceMode2D.Impulse);
                }
            }
        }
        else if (moveDirection.y != 0 && isClimbing)
        {
            isClimbing = false;
            var joint = GetComponent<DistanceJoint2D>();
            rb.AddForce(joint.connectedBody.velocity/2, ForceMode2D.Impulse);
            Debug.Log("Destroyed component");
            FreeFalling = true;
            sfxManager.PlaySound(SoundEffect.SoundEvent.onJump);
            joint.enabled = false;
        }
        if (moveDirection.x > 0)
        {
            sp.flipX = false;
        }
        else if (moveDirection.x < 0)
        {
            sp.flipX = true;
        }
        if(hasTorch)
        {
            if(lEmit == null)
            {
                lEmit = gameObject.AddComponent<LightEmitting>();
                lEmit.light = DefaultLight;
                lEmit.Start();
            }
        }
        else
        {
            if(lEmit != null)
            {
                Destroy(lEmit);
            }
            else
            {
                tag = "Untagged";
            }
        }
        if(!isSwimming && !isClimbing && !FreeFalling)
        {
            rb.velocity = new Vector2(moveDirection.x*Speed, rb.velocity.y);
        }
        else if (!isClimbing && !FreeFalling && isSwimming)
        {
            Flow = (moveDirection * Speed / 2);
            rb.velocity = new Vector2(Flow.x, moveDirection.y * jumpforce);
        }
        else if (isClimbing)
        {
            rb.AddForce(moveDirection * Speed, ForceMode2D.Force);
            gameObject.GetComponent<DistanceJoint2D>().attachedRigidbody.AddForce(moveDirection * Speed, ForceMode2D.Force);
        }
        else if (FreeFalling && moveDirection.x != 0)
        {
            rb.AddForce(new Vector2(moveDirection.x,0), ForceMode2D.Impulse);
        }
        if (moveDirection == Vector2.zero && !FreeFalling)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        if (isSwimming)
        {
            Vector3Int el = currentPos - offset;
            if ((float)fluidManager.fluidField[el.x, el.y].Level / fluidManager.MaximumLevel >= SwimmingLevel)
            {
                float levels = (float)fluidManager.fluidField[el.x, el.y].Level / fluidManager.MaximumLevel;
                float resistance = initialGravityScale / Mathf.Abs((levels));
                rb.gravityScale = resistance;
                rb.drag = 5 * resistance;
            }
            if (hasTorch)
            {
                hasTorch = false;
            }
        }
        else
        {
            rb.gravityScale = initialGravityScale;
            rb.drag = 0;
        }
        animator.SetFloat("Speed", Mathf.Abs(moveDirection.x * Speed));
        animator.SetBool("isJumping", !isGrounded);
        animator.SetBool("hasTorch", hasTorch);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent(out PointEffector2D pe))
        {
            FreeFalling = true;
        }
        if (collision.gameObject.CompareTag("Rope"))
        {
            DistanceJoint2D joint;
            if(TryGetComponent(out DistanceJoint2D newJoint))
            {
                joint = newJoint;
                joint.enabled = true;
            }
            else
            {
                joint = gameObject.AddComponent<DistanceJoint2D>();
            }
            if (!isClimbing)
            {
                isClimbing = true;
                sfxManager.PlaySound(SoundEffect.SoundEvent.onBodyImpact);
                joint.connectedBody = collision.gameObject.GetComponent<Rigidbody2D>();

            } 
            else
            {
                joint.connectedBody = collision.gameObject.GetComponent<Rigidbody2D>();
            }
        }
        if (collision.gameObject.CompareTag("Trigger"))
        {
            Vector3Int NormalizedVelocity = new Vector3Int(Mathf.FloorToInt(rb.velocity.normalized.x), Mathf.FloorToInt(rb.velocity.normalized.y), 0);
            if(isGrounded)
            {
                NormalizedVelocity.y = 0;
            }
            Vector3Int tilePos = currentPos + NormalizedVelocity;
            Tile tile = (Tile)Triggers.GetTile(tilePos);
            if (tile != null)
            {
                tManager.launch(tile);
            }
        }
        if (collision.CompareTag("Exit"))
        {
            Exit exit = collision.GetComponent<Exit>();
            exit.onExitSuccessfull();
        }
        if (!hasTorch)
        {
            if (collision.TryGetComponent<Torch>(out Torch torch))
            {
                sfxManager.PlaySound(SoundEffect.SoundEvent.onPickup);
                hasTorch = true;
                DefaultLight = torch.light;
            }
        }
        if (collision.CompareTag("Deadly"))
        {
            GetComponent<Death>().Die();
        }
        if (collision.gameObject.name == "Fluid")
        {
            Vector3Int el = currentPos - offset;
            if ((float)fluidManager.fluidField[el.x, el.y].Level / fluidManager.MaximumLevel >= SwimmingLevel)
                isSwimming = true;
            else
            {
                isSwimming = false;
            }
            if (fluidManager.fluidField[el.x, el.y].isDeadly)
            {
                GetComponent<Death>().Die();
            }
            else
            {
                sfxManager.PlaySound(SoundEffect.SoundEvent.onWaterEnter);
            }
        }
        else
        {
            if (!collision.isTrigger && !collision.gameObject.CompareTag("Rope"))
            {
                if(jumpTime >= 1.2f && !isGrounded)
                {
                    sfxManager.PlaySound(SoundEffect.SoundEvent.onJumpImpact);
                    Instantiate(JumpSmoke, transform.position, Quaternion.identity);
                }
                jumpTime = 0;
                isGrounded = true;
                FreeFalling = false;
            }
        }

    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (isGrounded)
        {
            if (moveDirection.y > 0)
            {
                sfxManager.PlaySound(SoundEffect.SoundEvent.onJump);
            }
        }
        if (!collision.isTrigger)
            isGrounded = false;
        if (collision.gameObject.name == "Fluid")
        {
            sfxManager.PlaySound(SoundEffect.SoundEvent.onWaterExit);
            Vector3Int el = currentPos - offset;
            if ((float)fluidManager.fluidField[el.x, el.y].Level / fluidManager.MaximumLevel >= SwimmingLevel)
                isSwimming = true;
            else
            {
                isSwimming = false;
            }
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Fluid")
        {
            Vector3Int el = currentPos - offset;
            if (!(el.x > 0 && el.x < fluidManager.fluidField.GetLength(0) - 1) && !(el.y > 0 && el.y < fluidManager.fluidField.GetLength(1) - 1))
            {
                isSwimming = false;
            }
            if ((float)fluidManager.fluidField[el.x, el.y].Level / fluidManager.MaximumLevel >= SwimmingLevel)
                isSwimming = true;
            else
            {
                isSwimming = false;
            }
            if (fluidManager.fluidField[el.x, el.y].isDeadly)
            {
                GetComponent<Death>().Die();
            }
        }
        if(collision.name == "Level")
        {
            isGrounded = true;
        }
    }
}
