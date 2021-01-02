using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LightEmitting : MonoBehaviour
{
    public new Light light;
    public Tilemap LitTM;
    private Vector3Int offset;
    public LightBehavior lightManager;
    public Vector3Int IntPosition;
    private Vector3Int oldPosition;
    private Rigidbody2D rb;
    public void Start()
    {
        if (lightManager == null)
        {
            lightManager = GameObject.Find("LightManager").GetComponent<LightBehavior>();
        }
        if (LitTM == null)
        {
            LitTM = GameObject.Find("Grid").transform.Find("Level").GetComponent<Tilemap>();
        }
        offset = LitTM.origin;
        tag = "Light Emitters";
        rb = GetComponent<Rigidbody2D>();
    }
    private void FixedUpdate()
    {
        
        if(rb != null)
        {
            IntPosition = Vector3Int.FloorToInt(rb.position)-offset;

        }
        else
        {
            IntPosition = Vector3Int.FloorToInt(transform.position)-offset;

        }
        if(oldPosition != IntPosition)
        {
            oldPosition = IntPosition;
        }
    }
}
