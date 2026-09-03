using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TJJBaseController : IEnemyController
{
    // Runtime cache to avoid linear search on every resolve call.
    private readonly Dictionary<TJJActionType, IEnemyAction> actionLookup = new Dictionary<TJJActionType, IEnemyAction>();

    [SerializeField] private List<TJJActionBinding> actionBindings = new List<TJJActionBinding>();

    public override void Start()
    {
        base.Start();
        BuildActionLookup();
    }

    public virtual List<TJJActionPhase> SelectAction()
    { return null; }

    /// <summary>
    /// Resolves an action by enum key from cached bindings.
    /// </summary>
    public IEnemyAction ResolveAction(TJJActionType type)
    {
        IEnemyAction action;
        return actionLookup.TryGetValue(type, out action) ? action : null;
    }

    /// <summary>
    /// Converts design-time phase definitions into runtime action groups.
    /// </summary>
    public void ApplyActionList(List<TJJActionPhase> phases)
    {
        actionList.Clear();

        if (phases == null || phases.Count == 0)
        {
            return;
        }

        foreach (TJJActionPhase phase in phases)
        {
            if (phase == null || phase.actionCombo == null || phase.actionCombo.Count == 0)
            {
                continue;
            }

            List<ActionCaller> group = BuildActionGroup(phase);
            if (group.Count > 0)
            {
                actionList.Add(group);
            }
        }
    }

    public override bool TryBuildNextActionList()
    {
        for (int attempt = 0; attempt < MaxActionBuildAttempts; attempt++)
        {
            List<TJJActionPhase> selectedPhases = SelectAction();

            if (selectedPhases == null || selectedPhases.Count == 0)
            {
                Debug.LogWarning($"{nameof(TJJPhaseOneController)}: No phase set configured.");
                continue;
            }

            ApplyActionList(selectedPhases);

            if (actionList.Count == 0)
            {
                Debug.LogWarning($"{nameof(TJJPhaseOneController)}: Selected phase set produced no executable actions.");
                continue;
            }

            // Seed lastAction for transition logic in downstream actions.
            lastAction = initialAction != null ? initialAction : actionList[0][0].action;
            return true;
        }

        Debug.LogError($"{nameof(TJJPhaseOneController)}: Failed to build a valid action list after {MaxActionBuildAttempts} attempts.");
        return false;
    }

    /// <summary>
    /// Builds one executable group from one phase.
    /// For each shared index group, one entry is chosen randomly.
    /// Non-indexed entries are always included.
    /// </summary>
    private List<ActionCaller> BuildActionGroup(TJJActionPhase phase)
    {
        List<ActionCaller> group = new List<ActionCaller>();

        HashSet<TJJActionEntry> selectedIndexedEntries = SelectIndexedEntries(phase.actionCombo);

        foreach (TJJActionEntry entry in phase.actionCombo)
        {
            if (entry == null)
            {
                continue;
            }

            if (entry.UseIndex && !selectedIndexedEntries.Contains(entry))
            {
                continue;
            }

            TJJActionCaller caller = entry.ToActionCaller(this);
            if (caller == null || caller.action == null)
            {
                continue;
            }

            group.Add(caller);
        }

        return group;
    }

    /// <summary>
    /// Selects one random entry per index bucket.
    /// </summary>
    private static HashSet<TJJActionEntry> SelectIndexedEntries(List<TJJActionEntry> entries)
    {
        var selected = new HashSet<TJJActionEntry>();

        var indexedGroups = entries
            .Where(e => e != null && e.UseIndex)
            .GroupBy(e => e.Index);

        foreach (IGrouping<int, TJJActionEntry> group in indexedGroups)
        {
            List<TJJActionEntry> candidates = group.ToList();
            TJJActionEntry chosen = candidates[Random.Range(0, candidates.Count)];
            selected.Add(chosen);
        }

        return selected;
    }

    /// <summary>
    /// Builds/refreshes the type->action map from serialized bindings.
    /// Last duplicate key wins by design.
    /// </summary>
    private void BuildActionLookup()
    {
        actionLookup.Clear();

        foreach (TJJActionBinding binding in actionBindings)
        {
            if (binding == null || binding.action == null)
            {
                continue;
            }

            actionLookup[binding.actionType] = binding.action;
        }
    }
}