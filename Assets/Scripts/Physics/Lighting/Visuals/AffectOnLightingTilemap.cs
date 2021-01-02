using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class AffectOnLightingTilemap : MonoBehaviour
{
    public Tilemap baseTilemap;
    public LightBehavior lightManager;
    private Tilemap self;
    private Vector3Int offset;
    private Camera main;
    private void Start()
    {
        self = GetComponent<Tilemap>();
        offset = baseTilemap.origin;
        main = Camera.main;
    }
    private void FixedUpdate()
    {
        UpdateVisuals();
    }
    private void UpdateVisuals()
    {
        Vector3 offsetBounds = new Vector3(1, 1, 0);
        Vector3 cullingFloatStart = main.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3Int culling = Vector3Int.FloorToInt(cullingFloatStart) - offset;

        Vector3 cullingFloatEnd = main.ViewportToWorldPoint(new Vector3(1, 1, 0)) + offsetBounds;
        Vector3Int cullingEnd = Vector3Int.FloorToInt(cullingFloatEnd) - offset;

        for (int x = culling.x; x < cullingEnd.x; x++)
        {
            for (int y = culling.y; y < cullingEnd.y; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0) + offset;
                if (lightManager.lightLevel[x, y].Power <= lightManager.IgnorePower || lightManager.lightLevel[x, y].color == Color.black)
                {
                    self.SetTileFlags(tilePos, TileFlags.None);
                    self.SetColor(tilePos, Color.black);
                    continue;
                }
                float brightness = lightManager.lightLevel[x, y].Power / lightManager.maximumLevel;
                Color color = new Color(brightness, brightness, brightness, 1);
                Color coloredLight = lightManager.lightLevel[x, y].color;
                color *= coloredLight;
                self.SetTileFlags(tilePos, TileFlags.None);
                self.SetColor(tilePos, color);
                self.SetTileFlags(tilePos, TileFlags.None);
                self.SetColor(tilePos, color);
            }
        }
    }
}
