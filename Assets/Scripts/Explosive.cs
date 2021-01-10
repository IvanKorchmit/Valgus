using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosive : MonoBehaviour
{
    public FluidPhysics fluidManager;
    private SoundManager sfxManager;
    public GameObject explosion;
    private Vector3Int offset;
    private Vector3Int currentPos;
    private Rigidbody2D rb;
    private bool isGrounded;
    private float fallTime;
    public GameObject effector;
    // Start is called before the first frame update
    void Start()
    {
        offset = fluidManager.ObstacleField.origin;
        rb = GetComponent<Rigidbody2D>();
        sfxManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
    }
    private void FixedUpdate()
    {
        if(!isGrounded)
        {
            fallTime += rb.velocity.magnitude * rb.velocity.normalized.magnitude;
        }
        currentPos = new Vector3Int((int)rb.position.x, (int)rb.position.y, 0);
        Vector3Int el = currentPos - offset;
        if (fluidManager.fluidField[el.x, el.y].isDeadly)
        {
            Explode();
        }

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        isGrounded = false;

        if (collision.gameObject.name == "Level")
        {
            if(fallTime >= 800)
            {
                Explode();
            }
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {   
        isGrounded = false;
        fallTime = 0;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Deadly"))
        {
            Explode();
        }
    }
    public void Explode()
    {
        sfxManager.PlaySound(SoundEffect.SoundEvent.Explosion);
        Instantiate(explosion, gameObject.transform.position, Quaternion.identity);
        GameObject eff = Instantiate(effector, gameObject.transform.position, Quaternion.identity);
        Destroy(eff, 0.1f);
        Destroy(gameObject);
    }
}
