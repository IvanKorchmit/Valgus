using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Diagnostics;
using Utilities.ColorManipulation;
public class LightBehavior : MonoBehaviour
{
    #region fields
    public enum DebuggerMode
    {
        OnlyBrightness,
        OnlyLightColors,
        OnlyFluidDensity,
        OnlyFluidColor,
        ShowOnlyGlowingLiquids,
    }
    public Light[,] lightLevel;
    private Camera main;
    private Vector3Int offset;
    [Header("Tilemaps")]
    public Tilemap lightSources;
    public Tilemap LitTilemap;
    [Header("Debugging & performance")]
    [Tooltip("Show elapsed methods in milliseconds?")]
    public bool logging;
    [Tooltip("Ignore light's intensity if it's less or equal, then in stage of visualizing lighting they will be ignored and turn tiles black. \n" +
        "It improves performance depending of how high it is, but if it's too high then it will spoil smooth gradient")]
    public float IgnorePower;
    [Tooltip("Set lighting enabled? The light map will be frozen until it's enabled")]
    public new bool enabled;
    [Tooltip("Reference to sprite which will render maps, but it will slow down the process")]
    public SpriteRenderer DebugSprite;
    public DebuggerMode DebugDrawMode;
    [Tooltip("Provide references to managers so they can talk to each other!")]
    [Header("Managers")]
    public FluidPhysics fluidManager;
    private Stopwatch sw = new Stopwatch();
    [Header("Light properties")]
    [Tooltip("The maximum brightness of light to be shown (Doesn't affect on logic)")]
    public float maximumLevel;
    [Tooltip("Light's intensity being absorped by solid tiles (At least if they have colliders)")]
    public float absorptionLevel;
    [Tooltip("Light's intensity being decreased each distance covered")]
    public float fade;
    [Tooltip("List of tiles which can be counted as light sources")]
    public LightSource[] lightBulbs;
    #endregion

    private void Start()
    {
        lightLevel = new Light[LitTilemap.size.x, LitTilemap.size.y];
        main = Camera.main;
        offset = LitTilemap.origin;
        sw.Start();
        defineLightSources();
        updateVisuals();
        sw.Stop();
        if(logging)
        UnityEngine.Debug.Log($"The definition, finding light sources and calculating light \n" +
            $" including color interpolation in between took {sw.ElapsedMilliseconds}ms in total to finish the task");
        enabled = true;

    }
    public void Step()
    {
        if (enabled)
        {
            defineLightSources();
            updateVisuals();
            Texture2D texture = DrawLightMap();
            texture.filterMode = FilterMode.Point;
            DebugSprite.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
    }
    public void defineLightSources()
    {
        sw.Reset();
        sw.Start();
        for (int x = 0; x < lightLevel.GetLength(0); x++)
        {
            for (int y = 0; y < lightLevel.GetLength(1); y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0) + offset;
                bool isEmitter = isLightEmitter(tilePos);
                if (fluidManager.fluidField != null)
                {
                    if (!isEmitter)
                    {
                        if (fluidManager.fluidField[x, y].isObstacle && lightSources.GetTile(tilePos) == null && !fluidManager.fluidField[x, y].isGlowing)
                        {
                            lightLevel[x, y] = new Light(0);
                            continue;
                        }
                        lightLevel[x, y] = checkLightbulb(x, y);
                        lightLevel = fluidManager.LightFluid(lightLevel, x, y);
                    }
                    else
                    {
                        lightLevel[x, y] = FindEmitters(tilePos);

                    }
                }
            }
        }
        sw.Stop();
        if(logging)
        print($"defineLightSources {sw.ElapsedMilliseconds}");
        lightLevel = calculateLighting(lightLevel);
    }
    public Light[,] calculateLighting(Light[,] self)
    {
        sw.Reset();
        sw.Start();
        Light[,] newLight = self;
        Vector2 direction = new Vector2(1, 1);

        for (int x = 0; x < newLight.GetLength(0); x++)
        {
            for (int y = 0; y < newLight.GetLength(1); y++)
            {
                newLight = applyRule(newLight, x, y, direction);
            }
        }
        // Calculating light in the opposite direction
        direction = new Vector2(-1, -1);
        for (int x = newLight.GetLength(0) - 1; x > 0; x--)
        {
            for (int y = newLight.GetLength(1) - 1; y > 0; y--)
            {
                newLight = applyRule(newLight, x, y, direction);
            }
        }
        sw.Stop();
        if(logging)
        print($"calculateLighting {sw.ElapsedMilliseconds}");
        return newLight;
    }
    // applies rules based on neighbors
    private Light[,] applyRule(Light[,] self, int x, int y, Vector2 loopDirection)
    {
        Light[,] newLight = self;
        Vector3Int tilePos = new Vector3Int(x, y, 0) + offset;
        Tile.ColliderType collider = LitTilemap.GetColliderType(tilePos);
        if (newLight[x, y].color == Color.black || newLight[x, y].Power <= 0) return newLight;
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
        if (loopDirection.x == -1 && loopDirection.y == -1)
        {
            if (x > 0)
            {
                if (newLight[x - 1, y].Power + fade < newLight[x, y].Power)
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
            if (y > 0)
            {
                if (newLight[x, y - 1].Power < newLight[x, y].Power)
                {
                    newLight[x, y - 1].Power = newLight[x, y].Power - fade;
                    if (newLight[x, y].color != Color.black && newLight[x, y - 1].color != Color.black && newLight[x, y].color != newLight[x, y - 1].color)
                    {
                        newLight[x, y - 1].color = ColorManipulation.mixColors(newLight[x, y].color, newLight[x, y - 1].color);
                    }
                    else
                    {
                        newLight[x, y - 1].color = newLight[x, y].color;
                    }
                }
            }
        }
        else if (loopDirection.x == 1 && loopDirection.y == 1)
        {
            if (x < newLight.GetLength(0) - 1)
            {
                if (newLight[x + 1, y].Power + fade < newLight[x, y].Power)
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
            if (y < newLight.GetLength(1) - 1)
            {
                if (newLight[x, y + 1].Power + fade< newLight[x, y].Power)
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
        }
        return newLight;
    }
    private Light checkLightbulb(int x, int y)
    {
        Vector3Int currentTilePos = new Vector3Int(x, y, 0) + offset;
        TileBase currentTile = lightSources.GetTile(currentTilePos);
        Light newLight = new Light(0);
        if (lightSources.GetTile(currentTilePos) == null)
        {
            return newLight;
        }
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
        sw.Reset();
        sw.Start();
        Vector3 offsetBounds = new Vector3(1, 1,0);
        Vector3 cullingFloatStart = main.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3Int culling = Vector3Int.FloorToInt(cullingFloatStart)-offset;

        Vector3 cullingFloatEnd = main.ViewportToWorldPoint(new Vector3(1, 1, 0)) + offsetBounds;
        Vector3Int cullingEnd = Vector3Int.FloorToInt(cullingFloatEnd) - offset;

        for (int x = culling.x; x < cullingEnd.x; x++)
        {
            for (int y = culling.y; y < cullingEnd.y; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0) + offset;
                if (lightLevel[x, y].Power <= IgnorePower || lightLevel[x, y].color == Color.black)
                {
                    LitTilemap.SetTileFlags(tilePos, TileFlags.None);
                    LitTilemap.SetColor(tilePos, Color.black);
                    lightSources.SetTileFlags(tilePos, TileFlags.None);
                    lightSources.SetColor(tilePos, Color.black);
                    continue;
                }
                float brightness = lightLevel[x, y].Power / maximumLevel;
                Color color = new Color(brightness, brightness, brightness, 1);
                Color coloredLight = lightLevel[x, y].color;
                color *= coloredLight;
                LitTilemap.SetTileFlags(tilePos, TileFlags.None);
                LitTilemap.SetColor(tilePos, color);
                lightSources.SetTileFlags(tilePos, TileFlags.None);
                lightSources.SetColor(tilePos, color);
            }
        }
        sw.Stop();
        if(logging)
        print($"updateVisuals {sw.ElapsedMilliseconds}");
    }
    private Texture2D DrawLightMap()
    {
        Texture2D newText = new Texture2D(lightLevel.GetLength(0), lightLevel.GetLength(1));
        if (!DebugSprite.gameObject.activeInHierarchy) return newText;
        for (int x = 0; x < lightLevel.GetLength(0); x++)
        {
            for (int y = 0; y < lightLevel.GetLength(1); y++)
            {
                Color color = lightLevel[x, y].color;
                switch (DebugDrawMode)
                {
                    case DebuggerMode.OnlyBrightness:
                        float brightness = lightLevel[x, y].Power / maximumLevel;
                        color = new Color(brightness, brightness, brightness);
                        break;
                    case DebuggerMode.OnlyLightColors:
                        color = lightLevel[x, y].color;
                        break;
                    case DebuggerMode.OnlyFluidDensity:
                        color = Color.white;
                        color.a = (float)fluidManager.fluidField[x, y].Level / fluidManager.MaximumLevel;
                        break;
                    case DebuggerMode.OnlyFluidColor:
                        color = fluidManager.fluidField[x, y].color;
                        break;
                    case DebuggerMode.ShowOnlyGlowingLiquids:
                        if (fluidManager.fluidField[x, y].isGlowing)
                        {
                            color = Color.white;
                        }
                        else
                        {
                            color = Color.black;
                        }
                        break;
                }
                newText.SetPixel(x, y, color);
            }
        }
        newText.Apply();
        return newText;
    }
    private Light FindEmitters(Vector3Int position)
    {
        position -= offset;

        if (!isLightEmitter(position + offset))
        {
            return lightLevel[position.x, position.y];
        }
        Light newLight = new Light(0);
        GameObject[] lightEmitters = GameObject.FindGameObjectsWithTag("Light Emitters");
        if (lightEmitters != null)
        {
            foreach (var light in lightEmitters)
            {
                if (position == light.GetComponent<LightEmitting>().IntPosition)
                {
                    newLight = light.GetComponent<LightEmitting>().light;
                    break;
                }
            }
        }
        return newLight;
    }
    private bool isLightEmitter(Vector3Int position)
    {
        position -= offset;
        GameObject[] lightEmitters = GameObject.FindGameObjectsWithTag("Light Emitters");
        if (lightEmitters != null)
        {
            foreach (var light in lightEmitters)
            {
                LightEmitting lEmit = light.GetComponent<LightEmitting>();
                if(lEmit == null)
                {
                    continue;
                }
                if (position == lEmit.IntPosition)
                {
                    return true;
                }
            }
        }
        return false;
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