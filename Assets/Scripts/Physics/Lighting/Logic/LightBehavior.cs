using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Diagnostics;
using Utilities.ColorManipulation;
public class LightBehavior : MonoBehaviour
{
    public Light[,] lightLevel;
    public LightSource[] lightBulbs;
    public Tilemap lightSources;
    public Tilemap LitTilemap;
    private Vector3Int offset;
    public SpriteRenderer DebugSprite;
    public FluidPhysics fluidManager;
    Stopwatch sw = new Stopwatch();
    public float maximumLevel;
    public float absorptionLevel;
    public float fade;
    private void Start()
    {
        lightLevel = new Light[LitTilemap.size.x, LitTilemap.size.y];
        offset = LitTilemap.origin;
        sw.Start();
        defineLightSources();
        updateVisuals();
        sw.Stop();
        UnityEngine.Debug.Log($"The definition, finding light sources and calculating light \n" +
            $" including color interpolation in between took {sw.ElapsedMilliseconds}ms in total to finish the task");

    }
    private void FixedUpdate()
    {
        defineLightSources();
        updateVisuals();
        Texture2D texture = DrawLightMap();
        texture.filterMode = FilterMode.Point;
        DebugSprite.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }
    public void defineLightSources()
    {
        lightLevel = new Light[LitTilemap.size.x, LitTilemap.size.y];
        for (int x = 0; x < lightLevel.GetLength(0); x++)
        {
            for (int y = 0; y < lightLevel.GetLength(1); y++)
            {
                lightLevel[x, y] = checkLightbulb(x, y);
            }
        }
        if(fluidManager.fluidField != null)
        {   
            lightLevel = fluidManager.LightFluid(lightLevel);
        }
        lightLevel = calculateLighting(lightLevel);
    }
    private Light[,] calculateLighting(Light[,] self)
    {
        //Light[,] newLight = (Light[,])self.Clone();
        Light[,] newLight = self;
        for (int x = 0; x < newLight.GetLength(0); x++)
        {
            for (int y = 0; y < newLight.GetLength(1); y++)
            {
                newLight = applyRule(newLight, x, y);
            }
        }
        // Calculating light in the opposite direction
        for (int x = newLight.GetLength(0) - 1; x > 0; x--)
        {
            for (int y = newLight.GetLength(1) - 1; y > 0; y--)
            {
                newLight = applyRule(newLight, x, y);
            }
        }
        return newLight;
    }
    private Light[,] applyRule(Light[,] self, int x, int y)
    {
        Light[,] newLight = self;
        Vector3Int tilePos = new Vector3Int(x, y, 0) + offset;
        Tile.ColliderType collider = LitTilemap.GetColliderType(tilePos);
        if (collider != Tile.ColliderType.None)
        {
            if (newLight[x, y].Power > absorptionLevel)
            {
                newLight[x, y].Power -= absorptionLevel;
            }
            else
            {
                newLight[x, y].Power = 0;
            }

        }
        if (x > 0)
        {
            if (newLight[x - 1, y].Power < newLight[x, y].Power)
            {
                newLight[x - 1, y].Power = newLight[x, y].Power - fade;
                if (newLight[x, y].color != Color.black && newLight[x - 1, y].color != Color.black && newLight[x, y].color != newLight[x - 1, y].color)
                {
                    newLight[x - 1, y].color = ColorManipulation.mixColors(newLight[x, y].color, newLight[x - 1, y].color);
                }
                else
                {
                    newLight[x - 1, y].color = newLight[x, y].color;
                }
            }
        }
        if (x < newLight.GetLength(0) - 1)
        {
            if (newLight[x + 1, y].Power < newLight[x, y].Power)
            {
                newLight[x + 1, y].Power = newLight[x, y].Power - fade;
                if (newLight[x, y].color != Color.black && newLight[x + 1, y].color != Color.black && newLight[x, y].color != newLight[x + 1, y].color)
                {
                    newLight[x + 1, y].color = ColorManipulation.mixColors(newLight[x, y].color, newLight[x + 1, y].color);
                }
                else
                {
                    newLight[x + 1, y].color = newLight[x, y].color;
                }
            }
        }
        if (y > 0)
        {
            if (newLight[x, y - 1].Power < newLight[x, y].Power)
            {
                newLight[x, y - 1].Power = newLight[x, y].Power - fade;
                if (newLight[x, y].color != Color.black && newLight[x, y - 1].color != Color.black && newLight[x,y].color != newLight[x,y - 1].color)
                {
                    newLight[x, y - 1].color = ColorManipulation.mixColors(newLight[x, y].color, newLight[x, y - 1].color);
                }
                else
                {
                    newLight[x, y - 1].color = newLight[x, y].color;
                }
            }
        }
        if (y < newLight.GetLength(1) - 1)
        {
            if (newLight[x, y + 1].Power < newLight[x, y].Power)
            {
                newLight[x, y + 1].Power = newLight[x, y].Power - fade;
                if (newLight[x, y].color != Color.black && newLight[x, y + 1].color != Color.black && newLight[x, y].color != newLight[x, y + 1].color)
                {
                    newLight[x, y + 1].color = ColorManipulation.mixColors(newLight[x, y].color, newLight[x, y + 1].color);
                }
                else
                {
                    newLight[x, y + 1].color = newLight[x, y].color;
                }
            }
        }
        return newLight;
    }
    private Light checkLightbulb(int x, int y)
    {
        Vector3Int currentTilePos = new Vector3Int(x, y, 0) + offset;
        TileBase currentTile = lightSources.GetTile(currentTilePos);
        Light newLight = new Light(0);
        for (int i = 0; i < lightBulbs.Length; i++)
        {
            if (lightBulbs[i].LightTile == currentTile)
            {
                newLight = lightBulbs[i].light;
            }
        }
        return newLight;
    }
    private void updateVisuals()
    {
        for (int x = 0; x < lightLevel.GetLength(0); x++)
        {
            for (int y = 0; y < lightLevel.GetLength(1); y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0) + offset;
                float brightness = lightLevel[x, y].Power / maximumLevel;
                Color color = new Color(brightness, brightness, brightness);
                Color coloredLight = lightLevel[x, y].color;
                color *= coloredLight;
                LitTilemap.SetTileFlags(tilePos, TileFlags.None);
                LitTilemap.SetColor(tilePos, color);
                lightSources.SetTileFlags(tilePos, TileFlags.None);
                lightSources.SetColor(tilePos, color);
            }
        }
    }
    private Texture2D DrawLightMap()
    {
        Texture2D newText = new Texture2D(lightLevel.GetLength(0), lightLevel.GetLength(1));
        for (int x = 0; x < lightLevel.GetLength(0); x++)
        {
            for (int y = 0; y < lightLevel.GetLength(1); y++)
            {
                float brightness = lightLevel[x, y].Power / maximumLevel;
                // Color color = new Color(brightness, brightness, brightness);
                Color color = fluidManager.fluidField[x, y].color;
                newText.SetPixel(x, y, color);
            }
        }
        newText.Apply();
        return newText;
    }
}
[System.Serializable]
public class LightSource
{
    public Light light;
    public TileBase LightTile;
}
[System.Serializable]
public struct Light
{
    public float Power;
    public Color color;
    public Light(float Power)
    {
        this.Power = Power;
        color = Color.black;
    }
    public static Light empty
    {
        get
        {
            Light l = new Light();
            l.color = Color.black;
            l.Power = 0;
            return l;
        }
    }
    public static Light white
    {
        get
        {
            Light l = new Light();
            l.color = Color.white;
            l.Power = 10;
            return l;
        }
    }
}