using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utilities.ColorManipulation;
public class AffectOnLighting : MonoBehaviour
{
    public LightBehavior lightManager; // Reference to light manager object
    private Vector3Int IntPosition;
    private new SpriteRenderer renderer;
    private ParticleSystem ps;
    private Vector3Int offset;
    public Tilemap litTilemap;
    public Color InitialColor;
    private void Update()
    {
        IntPosition = Vector3Int.RoundToInt(transform.position-offset);
        if(!(IntPosition.x >= 0 && IntPosition.x < lightManager.lightLevel.GetLength(0)-1 && IntPosition.y >= 0 && IntPosition.y < lightManager.lightLevel.GetLength(1)))
        {
            return;
        }
        float brightness = lightManager.lightLevel[IntPosition.x, IntPosition.y].Power / lightManager.maximumLevel;
        Color color = new Color(brightness, brightness, brightness);
        if (renderer != null)
        {
            Color coloredLight = ColorManipulation.mixColors(InitialColor, lightManager.lightLevel[IntPosition.x, IntPosition.y].color);
            color *= coloredLight;
            renderer.color = color;
        }
        else if (ps != null)
        {
            Color coloredLight = ColorManipulation.mixColors(ps.main.startColor.color, lightManager.lightLevel[IntPosition.x, IntPosition.y].color);
            color *= coloredLight;
            var startColor = ps.main;
            startColor.startColor = color;
        }
    }
    private void Start()
    {
        if(lightManager == null)
        {
            lightManager = GameObject.Find("LightManager").GetComponent<LightBehavior>();
        }
        if(litTilemap == null)
        {
            litTilemap = GameObject.Find("Grid").transform.Find("Level").GetComponent<Tilemap>();
        }
        if(TryGetComponent(out SpriteRenderer sp))
        {
            renderer = sp;
        }
        if (renderer != null)
        {
            InitialColor = renderer.color;
        }
        if (TryGetComponent(out ParticleSystem newPs))
        {
            ps = newPs;
        }
        offset = litTilemap.origin;
    }
}
