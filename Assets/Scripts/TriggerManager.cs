using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class TriggerManager : MonoBehaviour
{
    public TriggerAction[] trigger;
    public Death Player;
    public Tilemap Triggers;
    public FluidPhysics fluidManager;
    public SoundManager sfxManager;
    private void Start()
    {
        Triggers.GetComponent<TilemapRenderer>().enabled = false;
    }
    public void launch(Tile tile)
    {
        foreach (var item in trigger)
        {
            if (item.triggerTile == tile)
            {
                doAction(item);
            }
            else
            {
                continue;
            }
        }
    }
    public void doAction(TriggerAction action)
    {
        switch (action.action)
        {
            case TriggerAction.Action.Replace:
                ReplaceTiles(action);
                break;
            case TriggerAction.Action.Kill:
                Player.Die();
                break;
            case TriggerAction.Action.LightUp:
                LightUp(action);
                break;
            case TriggerAction.Action.SetFluid:
                SetFluid(action);
                break;
            case TriggerAction.Action.MakeSound:
                sfxManager.PlaySound(action.sound);
                break;
        }
    }
    private void ReplaceTiles(TriggerAction action)
    {
        Vector3Int offset = action.targetBase.origin;
        for (int x = 0; x < action.targetBase.size.x; x++)
        {
            for (int y = 0; y < action.targetBase.size.y; y++)
            {
                Vector3Int currentTile = new Vector3Int(x, y, 0) + offset;
                if ((Tile)Triggers.GetTile(currentTile) == action.TargetTile)
                {
                    action.targetBase.SetTile(currentTile, action.ReplaceTo);
                    if (action.ReplaceTo.colliderType == Tile.ColliderType.None)
                    {
                        fluidManager.fluidField[x, y].isObstacle = false;
                    }
                    else
                    {
                        fluidManager.fluidField[x, y].isObstacle = true;
                    }
                }
                else continue;
            }
        }
    }
    private void LightUp(TriggerAction action)
    {
        Vector3Int offset = action.targetBase.origin;
        for (int x = 0; x < action.targetBase.size.x; x++)
        {
            for (int y = 0; y < action.targetBase.size.y; y++)
            {
                Vector3Int currentTile = new Vector3Int(x, y, 0) + offset;
                if ((Tile)Triggers.GetTile(currentTile) == action.TargetTile)
                {
                    action.targetBase.SetTile(currentTile, action.ReplaceTo);
                }
                else continue;
            }
        }
    }
    private void SetFluid(TriggerAction action)
    {
        Vector3Int offset = action.targetBase.origin;
        for (int x = 0; x < action.targetBase.size.x; x++)
        {
            for (int y = 0; y < action.targetBase.size.y; y++)
            {
                Vector3Int currentTile = new Vector3Int(x, y, 0) + offset;
                if ((Tile)Triggers.GetTile(currentTile) == action.TargetTile)
                {
                    fluidManager.fluidField[x, y] = action.fluid;
                }
                else continue;
            }
        }
    }
}
[System.Serializable]
public class TriggerAction
{
    public enum Action
    {
        Replace,
        Kill,
        LightUp,
        SetFluid,
        MakeSound
    }
    public Action action;
    public SoundEffect.SoundEvent sound;
    public Tile triggerTile;
    public Tile TargetTile;
    public Light light;
    public Fluid fluid;
    public Tile ReplaceTo;
    public Tilemap targetBase;

}