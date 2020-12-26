using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utilities.ColorManipulation;
using Utilities.Math;
using System.Diagnostics;
public class FluidPhysics : MonoBehaviour
{
    public int MaximumLevel;
    public Tilemap ObstacleField;
    public Tilemap fluidTilemap;
    public Fluid[,] fluidField;
    public LightBehavior lightManager;
    private Vector3Int offsetObstacle;
    public Fluid[] fluidPresets;
    public TileBase fluidTile;
    
    private void Start()
    {
        fluidField = new Fluid[ObstacleField.size.x, ObstacleField.size.y];
        offsetObstacle = ObstacleField.origin;
        fluidField = initializeFluid(fluidField);
        InvokeRepeating("Step", 0f, 0.1f);
        UnityEngine.Debug.Log(fluidField.Length + " " + lightManager.lightLevel.Length);
    }
    private void Step()
    {
        
        fluidField = calculatePhysics(fluidField);
        UpdateVisuals();
    }
    public Fluid[,] initializeFluid(Fluid[,] self)
    {
        Fluid[,] initialized = self;
        for (int x = 0; x < initialized.GetLength(0); x++)
        {
            for (int y = 0; y < initialized.GetLength(1); y++)
            {
                Vector3Int tilepos = new Vector3Int(x, y, 0) + offsetObstacle;
                Vector3Int tileposObst = new Vector3Int(x, y, 0) + offsetObstacle;
                if (fluidTilemap.GetTile(tilepos) != null)
                {
                    initialized[x, y] = FindFluidPreset(fluidTilemap.GetColor(tilepos));
                    initialized[x, y].Level = MaximumLevel;
                }
                if(ObstacleField.GetColliderType(tileposObst) != Tile.ColliderType.None)
                {
                    initialized[x, y].isObstacle = true;
                }
            }
        }
        return initialized;
    }
    public Fluid[,] calculatePhysics(Fluid[,] self)
    {
        // Fluid[,] newField = (Fluid[,])self.Clone();
        Fluid[,] newField = self;
        Vector2 direction = new Vector2(1, 1);
        for (int x = 0; x < newField.GetLength(0); x++)
        {
            for (int y = 0; y < newField.GetLength(1); y++)
            {
                newField = ApplyRule(x, y, newField, direction);
            }
        }
        direction = new Vector2(-1, 1);
        for (int x = newField.GetLength(0)-1; x > 0; x--)
        {
            for (int y = 0; y < newField.GetLength(1); y++)
            {
                newField = ApplyRule(x, y, newField, direction);
            }
        }
        return newField;
    }
    private void UpdateVisuals()
    {
        for (int x = 0; x < fluidField.GetLength(0); x++)
        {
            for (int y = 0; y < fluidField.GetLength(1); y++)
            {
                Vector3Int tilepos = new Vector3Int(x, y, 0) + offsetObstacle;
                Vector3Int left = new Vector3Int(x-1, y, 0) + offsetObstacle;
                Vector3Int right = new Vector3Int(x+1, y, 0) + offsetObstacle;
                Vector3Int top = new Vector3Int(x, y+1, 0) + offsetObstacle;
                Vector3Int bottom = new Vector3Int(x, y-1, 0) + offsetObstacle;
                fluidTilemap.SetTileFlags(tilepos, TileFlags.None);
                fluidTilemap.SetTileFlags(left, TileFlags.None);
                fluidTilemap.SetTileFlags(right, TileFlags.None);
                fluidTilemap.SetTileFlags(top, TileFlags.None);
                fluidTilemap.SetTileFlags(bottom, TileFlags.None);

                fluidTilemap.SetColor(tilepos, adaptColor(x, y, fluidField[x, y].color));
                if (x > 0)
                    fluidTilemap.SetColor(left, adaptColor(x-1,y,fluidField[x-1,y].color));
                if (x < fluidField.GetLength(0)-1)
                    fluidTilemap.SetColor(right, adaptColor(x + 1, y, fluidField[x + 1, y].color));
                if (y < fluidField.GetLength(1)-1)
                    fluidTilemap.SetColor(top, adaptColor(x, y + 1, fluidField[x, y + 1].color));
                if (y > 0)
                    fluidTilemap.SetColor(bottom, adaptColor(x, y - 1, fluidField[x, y - 1].color));

                if (fluidField[x, y].Level > 0)
                {
                    fluidField[x, y].color.a = 1;
                    fluidTilemap.SetTile(tilepos, fluidTile);
                }
                else if (fluidField[x, y].Level == 0 && !fluidField[x,y].isObstacle)
                {
                    fluidTilemap.SetTile(tilepos, null);
                    fluidTilemap.SetColor(tilepos, fluidField[x, y].color);
                    fluidField[x, y].isGlowing = false;
                }
            }
        }
    }
    public Fluid[,] ApplyRule(int x, int y, Fluid[,] self, Vector2 loopDirection)
    {
        Fluid[,] newFluid = self;
        if (newFluid[x, y].Level >= 1 && !newFluid[x,y].isObstacle)
        {
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

                        }
                        if (newFluid[x, y - 1].Level > 0 && newFluid[x, y - 1].Level < MaximumLevel)
                        {
                            newFluid[x, y - 1].Level++;
                            newFluid[x, y].Level--;
                            if (!newFluid[x, y - 1].isObstacle && newFluid[x, y].Level > 0 && newFluid[x, y - 1].Level > 0 && newFluid[x, y].color != newFluid[x, y - 1].color)
                            {
                                Color temp = ColorManipulation.mixColors(newFluid[x, y - 1].color, newFluid[x, y].color);
                                newFluid[x, y - 1].color = temp;
                                newFluid[x, y].color = temp;
                            }
                            if (newFluid[x, y].isGlowing)
                            {
                                newFluid[x, y - 1].light.Power = splitPower(newFluid[x, y], newFluid[x, y - 1]);
                                newFluid[x, y - 1].isGlowing = true;
                            }

                        }
                    }
                }
            }
            if (loopDirection.x == 1)
            {
                if (x > 0)
                {
                    if (!newFluid[x - 1, y].isObstacle)
                    {
                        if (newFluid[x - 1, y].Level == 0 && newFluid[x, y].Level > 1)
                        {

                            newFluid[x - 1, y].color = newFluid[x + 1, y].color;
                            newFluid[x - 1, y].Level = newFluid[x, y].Level / 2;
                            newFluid[x, y].Level /= 2;
                            if (newFluid[x, y].isGlowing)
                            {
                                newFluid[x - 1, y].light.Power = splitPower(newFluid[x, y], newFluid[x - 1, y]);
                                newFluid[x - 1, y].isGlowing = true;
                            }
                        }
                        if (newFluid[x, y].Level > newFluid[x - 1, y].Level && newFluid[x, y].Level > 0)
                        {
                            newFluid[x - 1, y].Level++;
                            newFluid[x, y].Level--;
                            if (newFluid[x, y].isGlowing)
                            {
                                newFluid[x - 1, y].light.Power = splitPower(newFluid[x, y], newFluid[x - 1, y]);
                                newFluid[x - 1, y].isGlowing = true;
                            }
                        }
                        
                    }
                }
            }
            if(loopDirection.x == -1)
            {
                if(x < newFluid.GetLength(0)-1)
                {
                    if (!newFluid[x + 1, y].isObstacle)
                    {
                        if (newFluid[x + 1, y].Level == 0 && newFluid[x, y].Level > 1)
                        {
                            newFluid[x + 1, y].color = newFluid[x - 1, y].color;
                            newFluid[x + 1, y].Level = newFluid[x, y].Level / 2;
                            newFluid[x, y].Level /= 2;
                            if (newFluid[x, y].isGlowing)
                            {
                                newFluid[x + 1, y].light.Power = splitPower(newFluid[x, y], newFluid[x + 1, y]);
                                newFluid[x + 1, y].isGlowing = true;
                            }
                        }
                        if (newFluid[x, y].Level > newFluid[x + 1, y].Level && newFluid[x, y].Level > 0)
                        {
                            newFluid[x + 1, y].Level++;
                            newFluid[x, y].Level--;
                            if (newFluid[x, y].isGlowing)
                            {
                                newFluid[x + 1, y].light.Power = splitPower(newFluid[x,y],newFluid[x+1,y]);
                                newFluid[x + 1, y].isGlowing = true;
                            }
                        }
                    }
                }
            }
            if (!newFluid[x + 1, y].isObstacle && newFluid[x, y].Level > 0 && newFluid[x + 1, y].Level > 0 && newFluid[x, y].color != newFluid[x + 1, y].color)
            {
                Color temp = ColorManipulation.mixColors(newFluid[x + 1, y].color, newFluid[x, y].color);
                newFluid[x + 1, y].color = temp;
                newFluid[x, y].color = temp;
            }
            if (!newFluid[x - 1, y].isObstacle && newFluid[x, y].Level > 0 && newFluid[x - 1, y].Level > 0 && newFluid[x, y].color != newFluid[x - 1, y].color)
            {
                Color temp = ColorManipulation.mixColors(newFluid[x - 1, y].color, newFluid[x, y].color);
                newFluid[x - 1, y].color = temp;
                newFluid[x, y].color = temp;
            }
        }
        else if (newFluid[x, y].Level <= 0 && !newFluid[x, y].isObstacle)
        {
            newFluid[x, y].Erase();
        }

        return newFluid;
    }
    public Light[,] LightFluid(Light[,] self)
    {
        Light[,] lit = self;
        for (int x = 0; x < lit.GetLength(0); x++)
        {
            for (int y = 0; y < lit.GetLength(1); y++)
            {
                if(fluidField[x,y].isGlowing)
                {
                    lit[x, y].color = fluidField[x, y].color;
                    lit[x, y].color.a = 1;
                    lit[x, y].Power = fluidField[x, y].light.Power;
                }
            }
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
        if (next.isGlowing)
        {
            result = Math.Average(current.light.Power, next.light.Power);
        }
        else
        {
            result = current.light.Power;
        }
        return current.light.Power;
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
    public Fluid(bool clear)
    {
        if (clear)
        {
            color = Color.clear;
            Level = 0;
            Speed = 0;
            light = Light.empty;
            isGlowing = false;
            isObstacle = false;
        }
        else
        {
            color = Color.white;
            Level = 10;
            Speed = 1;
            light = Light.white;
            isGlowing = false;
            isObstacle = false;
        }

    }
    public Color color;
    public int Level;
    public int Speed;
    public bool isGlowing;
    public Light light;
    public bool isObstacle;
    public void Erase() // Erases itself
    {
        color = Color.clear;
        Level = 0;
        Speed = 0;
        isGlowing = false;
        light = Light.empty;
        isObstacle = false;
    }
}
