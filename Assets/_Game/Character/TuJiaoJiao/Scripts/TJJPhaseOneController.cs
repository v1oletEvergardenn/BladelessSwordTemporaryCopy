using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

/// <summary>
/// TuJiaoJiao phase-one controller:
/// - selects an action phase list
/// - resolves entries to executable actions
/// - runs action coroutine flow from base controller
/// </summary>
public class TJJPhaseOneController : TJJBaseController
{
    [Header("ActionTest")]
    [SerializeField] public bool enableTest = false;

    [SerializeField] private List<TJJActionPhase> TESTActionCombo = new List<TJJActionPhase>();
    [SerializeField] private List<TJJActionPhase> actionCombo1 = new List<TJJActionPhase>();
    [SerializeField] private List<TJJActionPhase> actionCombo2 = new List<TJJActionPhase>();
    [SerializeField] private List<TJJActionPhase> actionCombo3 = new List<TJJActionPhase>();
    [SerializeField] private List<TJJActionPhase> actionCombo4 = new List<TJJActionPhase>();
    [SerializeField] private List<TJJActionPhase> actionCombo5 = new List<TJJActionPhase>();
    [SerializeField] private List<TJJActionPhase> actionCombo6 = new List<TJJActionPhase>();

    /// <summary>
    /// Chooses which phase set to run.
    /// Test set has priority when enabled.
    /// Otherwise first non-empty production set is used.
    /// </summary>
    public override List<TJJActionPhase> SelectAction()
    {
        if (enableTest && TESTActionCombo.Count > 0)
        {
            return TESTActionCombo;
        }

        if (actionCombo1.Count > 0) return actionCombo1;
        if (actionCombo2.Count > 0) return actionCombo2;
        if (actionCombo3.Count > 0) return actionCombo3;
        if (actionCombo4.Count > 0) return actionCombo4;
        if (actionCombo5.Count > 0) return actionCombo5;
        if (actionCombo6.Count > 0) return actionCombo6;

        return null;
    }

    public override void DoBreak(float amount)
    {
        return; // Phase one does not react to break damage
    }
}