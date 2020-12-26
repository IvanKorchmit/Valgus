using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class AffectOnLighting : MonoBehaviour
{
    public LightBehavior lightManager; // Reference to light manager object
    private Vector3Int IntPosition;
    private new SpriteRenderer renderer;
    private Vector3Int offset;
    public Tilemap litTilemap;
    private void Update()
    {
        IntPosition = Vector3Int.RoundToInt(transform.position-offset);
        float brightness = lightManager.lightLevel[IntPosition.x, IntPosition.y].Power / lightManager.maximumLevel;
        Color color = new Color(brightness, brightness, brightness);
        Color coloredLight = lightManager.lightLevel[IntPosition.x, IntPosition.y].color;
        color *= coloredLight;
        renderer.color = color;
    }
    private void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
        offset = litTilemap.origin;
    }
}
