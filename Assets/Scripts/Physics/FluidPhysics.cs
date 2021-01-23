using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utilities.ColorManipulation;
using Utilities.Math;
using System.Diagnostics;
public class FluidPhysics : MonoBehaviour
{
    public bool Logging;
    public bool disableParticles;
    public int MaximumLevel;
    public Tilemap ObstacleField;
    public Tilemap fluidTilemap;
    public Fluid[,] fluidField;
    public LightBehavior lightManager;
    private Vector3Int offsetObstacle;
    public Fluid[] fluidPresets;
    public TileBase fluidTile;
    private Stopwatch sw = new Stopwatch();
    private SoundManager sfxManager;
    public GameObject deadlyParticle;
    private Camera main;
    public FluidTile fluidTiles;

    private void Start()
    {
        main = Camera.main;
        sfxManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        sw.Reset();
        sw.Start();
        fluidField = new Fluid[ObstacleField.size.x, ObstacleField.size.y];
        offsetObstacle = ObstacleField.origin;
        fluidField = initializeFluid(fluidField);
        InvokeRepeating("Step", 0f, 0.1f);
        sw.Stop();
        if (Logging)
            print($"Start() {sw.ElapsedMilliseconds}");
    }
    private void Step()
    {
        sw.Reset();
        sw.Start();
        fluidField = calculatePhysics(fluidField);
        UpdateVisuals();
        lightManager.Step();
        sw.Stop();
        if (Logging)
            print($"Step() {sw.ElapsedMilliseconds}");
    }
    public Fluid[,] initializeFluid(Fluid[,] self)
    {
        sw.Reset();
        sw.Start();
        Fluid[,] initialized = self;
        for (int x = 0; x < initialized.GetLength(0); x++)
        {
            for (int y = 0; y < initialized.GetLength(1); y++)
            {
                Vector3Int tilepos = new Vector3Int(x, y, 0) + offsetObstacle;
                Vector3Int tileposObst = new Vector3Int(x, y, 0) + offsetObstacle;
                if (fluidTilemap.GetTile(tilepos) == null && ObstacleField.GetColliderType(tileposObst) == Tile.ColliderType.None)
                {
                    continue;
                }
                else if (fluidTilemap.GetTile(tilepos) != null && ObstacleField.GetColliderType(tileposObst) == Tile.ColliderType.None)
                {
                    initialized[x, y] = FindFluidPreset(fluidTilemap.GetColor(tilepos));
                }
                else if (ObstacleField.GetColliderType(tileposObst) != Tile.ColliderType.None)
                {
                    initialized[x, y].isObstacle = true;
                }
            }
        }
        sw.Stop();
        if (Logging)
        {
            print($"initializeFluid() {sw.ElapsedMilliseconds}");
        }
        return initialized;
    }
    public Fluid[,] calculatePhysics(Fluid[,] self)
    {
        sw.Reset();
        sw.Start();
        Fluid[,] newField = self;
        int changes = 0;
        int count = 0;

        for (int y = 0; y < newField.GetLength(1); y++)
        {
            Vector2 direction = new Vector2(1, 1);
            for (int x = 0; x < newField.GetLength(0); x++)
            {
                if (newField[x, y].isObstacle || newField[x, y].Level <= 0)
                {
                    if (!newField[x, y].isObstacle)
                    {
                        newField[x, y].Erase();
                    }
                    continue;
                }
                newField = ApplyRule(x, y, newField, direction, ref changes, ref count);
            }
            direction = new Vector2(-1, 1);
            for (int x = newField.GetLength(0) - 1; x > 0; x--)
            {
                if (newField[x, y].isObstacle || newField[x, y].Level <= 0)
                {
                    if (!newField[x, y].isObstacle)
                    {
                        newField[x, y].Erase();
                    }
                    continue;
                }
                newField = ApplyRule(x, y, newField, direction, ref changes, ref count);
            }
        }
        if ((float)changes / count >= 0.2f)
        {
            sfxManager.PlaySound(SoundEffect.SoundEvent.onWaterMove);
        }
        sw.Stop();
        if (Logging)
            print($"calculatePhysics() {sw.ElapsedMilliseconds}");
        return newField;
    }
    private void UpdateVisuals()
    {
        sw.Reset();
        sw.Start();
        Vector3 offsetBounds = new Vector3(1, 1, 0);
        Vector3 cullingFloatStart = main.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3Int culling = Vector3Int.FloorToInt(cullingFloatStart) - offsetObstacle;
        culling.x -= 5;
        culling.y -= 5;
        Vector3 cullingFloatEnd = main.ViewportToWorldPoint(new Vector3(1, 1, 0)) + offsetBounds;
        Vector3Int cullingEnd = Vector3Int.FloorToInt(cullingFloatEnd) - offsetObstacle;
        cullingEnd.x += 5;
        cullingEnd.y += 5;
        for (int x = culling.x; x < cullingEnd.x; x++)
        {
            for (int y = culling.y; y < cullingEnd.y; y++)
            {
                if (!(x > 0 && x < fluidField.GetLength(0) && y > 0 && y < fluidField.GetLength(1)))
                {
                    continue;
                }
                Vector3Int tilepos = new Vector3Int(x, y, 0) + offsetObstacle;
                Vector3Int left = new Vector3Int(x - 1, y, 0) + offsetObstacle;
                Vector3Int right = new Vector3Int(x + 1, y, 0) + offsetObstacle;
                Vector3Int top = new Vector3Int(x, y + 1, 0) + offsetObstacle;
                Vector3Int bottom = new Vector3Int(x, y - 1, 0) + offsetObstacle;
                fluidTilemap.SetTileFlags(tilepos, TileFlags.None);
                fluidTilemap.SetTileFlags(left, TileFlags.None);
                fluidTilemap.SetTileFlags(right, TileFlags.None);
                fluidTilemap.SetTileFlags(top, TileFlags.None);
                fluidTilemap.SetTileFlags(bottom, TileFlags.None);
                if(lightManager.LitTilemap.GetColor(tilepos) == Color.black)
                {
                    fluidTilemap.SetColor(tilepos, Color.black);
                    continue;
                }
                if (!disableParticles)
                {
                    if (fluidTilemap.GetTile(top) == null && fluidTilemap.GetTile(tilepos) != null)
                    {
                        if (fluidField[x, y].isDeadly)
                        {
                            Color color = fluidTilemap.GetColor(tilepos);
                            GameObject particle = Instantiate(deadlyParticle, new Vector2(top.x + 0.5f, top.y), Quaternion.identity);
                            ParticleSystem ps = particle.GetComponent<ParticleSystem>();
                            var main = ps.main;
                            main.startColor = new ParticleSystem.MinMaxGradient(color, ColorManipulation.mixColors(color, new Color(1, 1, 1, color.a)));
                        }
                    }
                }
                fluidTilemap.SetColor(tilepos, adaptColor(x, y, fluidField[x, y].color));
                if (x > 0)
                    fluidTilemap.SetColor(left, adaptColor(x - 1, y, fluidField[x - 1, y].color));
                if (x < fluidField.GetLength(0) - 1)
                    fluidTilemap.SetColor(right, adaptColor(x + 1, y, fluidField[x + 1, y].color));
                if (y < fluidField.GetLength(1) - 1)
                    fluidTilemap.SetColor(top, adaptColor(x, y + 1, fluidField[x, y + 1].color));
                if (y > 0)
                    fluidTilemap.SetColor(bottom, adaptColor(x, y - 1, fluidField[x, y - 1].color));
                if (fluidField[x, y].isObstacle || (fluidField[x, y].Level <= 0 && fluidTilemap.GetTile(tilepos) == null))
                {
                    fluidTilemap.SetTile(tilepos, null);
                    fluidTilemap.SetColor(tilepos, fluidField[x, y].color);
                    fluidField[x, y].isGlowing = false;
                    continue;
                }
                if (fluidField[x, y].Level <= 0 && fluidTilemap.GetTile(tilepos) == null || fluidField[x, y].isObstacle)
                {
                    continue;
                }
                if (fluidField[x, y].Level > 0)
                {
                    fluidField[x, y].color.a = 1;
                    fluidTilemap.SetTile(tilepos, fluidTiles.center);
                    if(fluidTilemap.GetTile(left) == null && fluidTilemap.GetTile(right) != null && fluidTilemap.GetTile(bottom) != null && fluidTilemap.GetTile(top) == null && lightManager.LitTilemap.GetColliderType(left) == Tile.ColliderType.None)
                    {
                        fluidTilemap.SetTile(tilepos, fluidTiles.TopLeft);
                    }
                    else if (fluidTilemap.GetTile(left) != null && fluidTilemap.GetTile(right) == null && fluidTilemap.GetTile(bottom) != null && fluidTilemap.GetTile(top) == null && lightManager.LitTilemap.GetColliderType(right) == Tile.ColliderType.None)
                    {
                        fluidTilemap.SetTile(tilepos, fluidTiles.TopRight);

                    }
                }
                else if (fluidField[x, y].Level == 0 && !fluidField[x, y].isObstacle)
                {
                    fluidTilemap.SetTile(tilepos, null);
                    fluidTilemap.SetColor(tilepos, fluidField[x, y].color);
                    fluidField[x, y].isGlowing = false;
                }
            }
        }
        sw.Stop();
        if(Logging)
        print("UpdateVisuals() " + sw.ElapsedMilliseconds);

    }
    public Fluid[,] ApplyRule(int x, int y, Fluid[,] self, Vector2 loopDirection, ref int changes, ref int waterAmount)
    {
        Fluid[,] newFluid = self;


        bool isLeftObstacle = newFluid[x - 1, y].isObstacle;
        bool isRightObstacle = newFluid[x + 1, y].isObstacle;
        bool isBottomObstacle = newFluid[x, y - 1].isObstacle;
        if(newFluid[x,y].Level > 0)
        {
            waterAmount++;
        }
        if (newFluid[x, y].Level == 1)
        {
            if (loopDirection.x == 1)
            {
                if(x > 0)
                {
                    if(newFluid[x-1,y].Level <= 0 && !newFluid[x-1,y].isObstacle && newFluid[x+1,y].isObstacle)
                    {
                        newFluid[x - 1, y] = newFluid[x, y];
                        newFluid[x, y].Erase();
                    }
                }
            }
            if (loopDirection.x == -1)
            {
                if (x > 0)
                {
                    if (newFluid[x + 1, y].Level <= 0 && !newFluid[x + 1, y].isObstacle && newFluid[x - 1, y].isObstacle)
                    {

                        newFluid[x + 1, y] = newFluid[x, y];
                        newFluid[x, y].Erase();
                    }
                }
            }
        }
        if (newFluid[x, y].Level == newFluid[x + 1, y].Level && newFluid[x, y].Level == newFluid[x - 1, y].Level || (isLeftObstacle || isRightObstacle))
        {
            if (newFluid[x, y - 1].Level == newFluid[x, y - 1].Level || isBottomObstacle)
            {
                if (newFluid[x, y].color == newFluid[x + 1, y].color && newFluid[x, y].color == newFluid[x - 1, y].color && !isRightObstacle && !isLeftObstacle)
                {
                    return self;

                }
            }
        }
        if (newFluid[x, y].Level >= 1 && !newFluid[x, y].isObstacle)
        {
            #region Color Mixing
            if (y < newFluid.GetLength(1) - 1)
            {
                if (!newFluid[x, y + 1].isObstacle && newFluid[x, y].Level > 0 && newFluid[x, y + 1].Level > 0 && newFluid[x, y].color != newFluid[x, y + 1].color)
                {
                    Color temp = ColorManipulation.mixColors(newFluid[x, y + 1].color, newFluid[x, y].color);
                    newFluid[x, y + 1].color = temp;
                    newFluid[x, y].color = temp;
                }
            }
            #endregion
            if (loopDirection.y == 1)
            {
                if (y > 0)
                {

                    if (!newFluid[x, y - 1].isObstacle)
                    {
                        if (newFluid[x, y - 1].Level <= 0)
                        {
                            newFluid[x, y - 1] = newFluid[x, y];
                            newFluid[x, y].Erase();
                            changes++;
                        }
                        if (newFluid[x, y - 1].Level > 0 && newFluid[x, y - 1].Level < MaximumLevel)
                        {
                            newFluid[x, y - 1].Level++;
                            newFluid[x, y].Level--;
                            changes++;
                            if (!newFluid[x, y - 1].isObstacle && newFluid[x, y].Level > 0 && newFluid[x, y - 1].Level > 0 && newFluid[x, y].color != newFluid[x, y - 1].color)
                            {
                                Color temp = ColorManipulation.mixColors(newFluid[x, y - 1].color, newFluid[x, y].color);
                                newFluid[x, y - 1].color = temp;
                                newFluid[x, y].color = temp;
                            }
                            if (newFluid[x, y].isGlowing)
                            {
                                if (!newFluid[x, y - 1].isGlowing)
                                {
                                    newFluid[x, y - 1].light.Power = splitPower(newFluid[x, y], newFluid[x, y - 1]);
                                    newFluid[x, y].light.Power = splitPower(newFluid[x, y - 1], newFluid[x, y]);

                                    newFluid[x, y - 1].isGlowing = true;
                                }
                            }
                            if (newFluid[x, y].isDeadly)
                            {
                                newFluid[x, y - 1].isDeadly = true;
                            }

                        }
                    }
                }
            }
            if (loopDirection.x == 1)
            {
                if (!newFluid[x - 1, y].isObstacle && newFluid[x, y].Level > 0 && newFluid[x - 1, y].Level > 0 && newFluid[x, y].color != newFluid[x - 1, y].color)
                {
                    Color temp = ColorManipulation.mixColors(newFluid[x - 1, y].color, newFluid[x, y].color);
                    newFluid[x - 1, y].color = temp;
                    newFluid[x, y].color = temp;
                }
                if (x > 0)
                {
                    if (!newFluid[x - 1, y].isObstacle)
                    {
                        if (newFluid[x - 1, y].Level == 0 && newFluid[x, y].Level > 1)
                        {
                            newFluid[x - 1, y].color = newFluid[x, y].color;
                            newFluid[x - 1, y].Level = newFluid[x, y].Level / 2;
                            changes++;
                            newFluid[x, y].Level /= 2;
                            if (newFluid[x, y].isGlowing)
                            {
                                if (!newFluid[x - 1, y].isGlowing)
                                {
                                    newFluid[x - 1, y].light.Power = splitPower(newFluid[x, y], newFluid[x - 1, y]);
                                    newFluid[x, y].light.Power = splitPower(newFluid[x - 1, y], newFluid[x, y]);

                                    newFluid[x - 1, y].isGlowing = true;
                                }
                            }
                            if (newFluid[x, y].isDeadly)
                            {
                                newFluid[x - 1, y].isDeadly = true;
                            }
                        }
                        if (newFluid[x, y].Level > newFluid[x - 1, y].Level && newFluid[x, y].Level > 0)
                        {
                            changes++;
                            int difference = Mathf.Abs(newFluid[x, y].Level - newFluid[x - 1, y].Level);
                            if (difference >= 3 && newFluid[x, y].Level > difference)
                            {
                                newFluid[x - 1, y].Level++;
                                newFluid[x, y].Level--;
                            }
                            else
                            {
                                newFluid[x - 1, y].Level += 3;
                                newFluid[x, y].Level -= 3;
                            }
                            newFluid[x - 1, y].color = newFluid[x, y].color;
                            if (newFluid[x, y].isGlowing)
                            {
                                if (!newFluid[x - 1, y].isGlowing)
                                {
                                    newFluid[x - 1, y].light.Power = splitPower(newFluid[x, y], newFluid[x - 1, y]);
                                    newFluid[x, y].light.Power = splitPower(newFluid[x - 1, y], newFluid[x, y]);

                                    newFluid[x - 1, y].isGlowing = true;
                                }
                            }
                            if (newFluid[x, y].isDeadly)
                            {
                                newFluid[x - 1, y].isDeadly = true;
                            }


                        }
                    }
                }
            }
            else if (loopDirection.x == -1)
            {
                if (!newFluid[x + 1, y].isObstacle && newFluid[x, y].Level > 0 && newFluid[x + 1, y].Level > 0 && newFluid[x, y].color != newFluid[x + 1, y].color)
                {
                    Color temp = ColorManipulation.mixColors(newFluid[x + 1, y].color, newFluid[x, y].color);
                    newFluid[x + 1, y].color = temp;
                    newFluid[x, y].color = temp;
                }
                if (x < newFluid.GetLength(0) - 1)
                {
                    if (!newFluid[x + 1, y].isObstacle)
                    {
                        if (newFluid[x + 1, y].Level == 0 && newFluid[x, y].Level > 1)
                        {
                            changes++;
                            newFluid[x + 1, y].color = newFluid[x, y].color;
                            newFluid[x + 1, y].Level = newFluid[x, y].Level / 2;
                            newFluid[x, y].Level /= 2;
                            if (newFluid[x, y].isGlowing)
                            {
                                if (!newFluid[x + 1, y].isGlowing)
                                {
                                    newFluid[x + 1, y].light.Power = splitPower(newFluid[x, y], newFluid[x + 1, y]);
                                    newFluid[x, y].light.Power = splitPower(newFluid[x + 1, y], newFluid[x, y]);

                                    newFluid[x + 1, y].isGlowing = true;
                                }
                            }
                            if (newFluid[x, y].isDeadly)
                            {
                                newFluid[x + 1, y].isDeadly = true;
                            }
                        }
                        if (newFluid[x, y].Level > newFluid[x + 1, y].Level && newFluid[x, y].Level > 0)
                        {
                            int difference = Mathf.Abs(newFluid[x, y].Level - newFluid[x + 1, y].Level);
                            if (difference >= 3 && newFluid[x, y].Level > difference)
                            {
                                newFluid[x + 1, y].Level++;
                                newFluid[x, y].Level--;
                            }
                            else
                            {
                                newFluid[x + 1, y].Level += 3;
                                newFluid[x, y].Level -= 3;
                            }
                            newFluid[x + 1, y].color = newFluid[x, y].color;
                            if (newFluid[x, y].isGlowing)
                            {
                                if (!newFluid[x + 1, y].isGlowing)
                                {
                                    newFluid[x + 1, y].light.Power = splitPower(newFluid[x, y], newFluid[x + 1, y]);
                                    newFluid[x, y].light.Power = splitPower(newFluid[x + 1, y], newFluid[x, y]);

                                    newFluid[x + 1, y].isGlowing = true;
                                }
                            }
                            if (newFluid[x, y].isDeadly)
                            {
                                newFluid[x + 1, y].isDeadly = true;
                            }
                        }
                    }
                }
            }
        }
        return newFluid;
    }
    public Light[,] LightFluid(Light[,] self, int x, int y, ref Light cur)
    {
        Light[,] lit = self;
        Vector3Int tile = new Vector3Int(x, y, 0) + offsetObstacle;
        if (lightManager.lightSources.GetTile(tile) == null && (fluidField[x, y].isObstacle || !fluidField[x, y].isGlowing))
        {
            cur.Power = 0;
            cur.color = Color.black;
            lit[x, y] = cur;
            return lit;
        }
        if (fluidField[x, y].isGlowing)
        {
            lit[x, y].color = fluidField[x, y].color;
            lit[x, y].color.a = 1;
            lit[x, y].Power = fluidField[x, y].light.Power;
        }
        return lit;
    }
    private Fluid FindFluidPreset(Color color)
    {
        Fluid result = fluidPresets[Random.Range(0, fluidPresets.Length)];
        return result;
    }
    private float splitPower(Fluid current, Fluid next)
    {
        float result;
        // result = Math.Average(current.light.Power, next.light.Power);
        result = current.light.Power;
        return result;
    }
    private Color adaptColor(int x, int y, Color self)
    {
        Color adapted = self;
        Fluid currentFluid = fluidField[x, y];
        Light currentLight = lightManager.lightLevel[x, y];
        float brightness = currentLight.Power / lightManager.maximumLevel;
        Color adjustBrightness = new Color(brightness, brightness, brightness, 1);
        adapted = ColorManipulation.mixColors(currentLight.color, adapted);
        adapted *= adjustBrightness;
        adapted.a = (float)currentFluid.Level / MaximumLevel;

        return adapted;
    }
}
[System.Serializable]
public struct Fluid
{
    
    public Color color;
    public int Level;
    public bool isGlowing;
    public Light light;
    public bool isObstacle;
    public bool isDeadly;
    public void Erase() // Erases itself
    {
        color = Color.clear;
        Level = 0;
        isGlowing = false;
        isObstacle = false;
        isDeadly = false;


    }
}
[System.Serializable]
public class FluidTile
{
    public TileBase center;
    public TileBase TopLeft;
    public TileBase TopRight;

}