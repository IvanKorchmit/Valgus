using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactSound : MonoBehaviour
{
    private Rigidbody2D rb;
    private SoundManager sfxManager;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sfxManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name != "Fluid")
        {
            if (rb.velocity.normalized.magnitude == 1)
                sfxManager.PlaySound(SoundEffect.SoundEvent.onBodyImpact);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.name == "Fluid")
        {
            sfxManager.PlaySound(SoundEffect.SoundEvent.onWaterEnter);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Fluid")
        {
            sfxManager.PlaySound(SoundEffect.SoundEvent.onWaterExit);
        }
    }
}
