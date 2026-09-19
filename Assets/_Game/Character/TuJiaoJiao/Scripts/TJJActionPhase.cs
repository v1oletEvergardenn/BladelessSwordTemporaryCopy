using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// All supported action slots for TJJ phase logic.
/// </summary>
public enum TJJActionType
{
    HoldHighEnough = 0,
    Charge = 1,
    FoxFire = 2,
    GoldShard = 3
}

/// <summary>
/// Maps a logical action type to a concrete enemy action instance.
/// </summary>
[Serializable]
public class TJJActionBinding
{
    [HorizontalGroup("Row", Width = 220)]
    [HideLabel]
    public TJJActionType actionType;

    [HorizontalGroup("Row")]
    [HideLabel]
    public IEnemyAction action;
}

/// <summary>
/// Specialized action caller wrapper for TJJ actions.
/// </summary>
public class TJJActionCaller : ActionCaller
{
    public TJJActionCaller(IEnemyAction action, int factor = 0, float delay = 0f) : base(action, factor, delay)
    {
    }
}

/// <summary>
/// One action entry in a phase combo.
/// UseIndex/Index allows mutually exclusive random picks by shared index group.
/// </summary>
[Serializable]
public class TJJActionEntry
{
    [HorizontalGroup("Row", Width = 15)]
    [HideLabel]
    public bool UseIndex = false;

    [HorizontalGroup("Row", Width = 30)]
    [ShowIf(nameof(UseIndex))]
    [HideLabel]
    public int Index;

    [HorizontalGroup("Row", Width = 230)]
    [HideLabel]
    public TJJActionType actionType;

    [HorizontalGroup("Row", Width = 100)]
    [HideLabel]
    public int factor = 0;

    [HorizontalGroup("Row")]
    [HideLabel]
    [PropertyRange(0, 10)]
    public float delay = 0f;

    /// <summary>
    /// Creates a runtime caller by resolving actionType from the controller.
    /// Returns null when no action is mapped.
    /// </summary>
    public TJJActionCaller ToActionCaller(TJJBaseController controller)
    {
        if (controller == null)
        {
            return null;
        }

        IEnemyAction action = controller.ResolveAction(actionType);
        if (action == null)
        {
            return null;
        }

        return new TJJActionCaller(action, factor, delay);
    }
}

/// <summary>
/// One phase = one combo definition (list of action entries).
/// </summary>
[Serializable]
public class TJJActionPhase
{
    public List<TJJActionEntry> actionCombo = new List<TJJActionEntry>();
}