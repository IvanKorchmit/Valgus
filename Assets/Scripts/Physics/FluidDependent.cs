using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FluidDependent : MonoBehaviour
{
    private Rigidbody2D rb;
    public Tilemap obstacle;
    private Vector3Int offset;
    private Vector3Int currentPos;
    public FluidPhysics fluidManager;
    private float initGravity;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        offset = obstacle.origin;
        initGravity = rb.gravityScale;
    }
    private void FixedUpdate()
    {
        currentPos = Vector3Int.FloorToInt(transform.position)-offset;
        Fluid currentFluid = fluidManager.fluidField[currentPos.x, currentPos.y];
        float fluidLevel = (float)currentFluid.Level / fluidManager.MaximumLevel;
        if(currentFluid.Level == 0)
        {
            rb.gravityScale = initGravity;
        }
        else if (currentFluid.Level > 0 && currentFluid.Level < 5)
        {
            rb.gravityScale = initGravity * fluidLevel;
            rb.drag = rb.gravityScale / fluidLevel;
        }
        else if(currentFluid.Level >= 5)
        {
            rb.gravityScale = -initGravity * fluidLevel;
            rb.drag = rb.gravityScale / fluidLevel;
        }
    }
}
