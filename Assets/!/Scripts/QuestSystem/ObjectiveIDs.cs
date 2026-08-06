using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObjectiveType
{
    Kill,
    Collect,
    Explore,
    Talk,
    Combat,
    PlayerInput,
    HeartSword,
    Custom
}

public abstract class ObjectiveIDs
{
    public abstract ObjectiveType ObjectiveType { get; }
}

// Kill objectives
public class KillObjectiveIDs : ObjectiveIDs
{
    public static readonly string KillMonster = "kill_monster";
    public static readonly string KillBoss = "kill_boss";
    public override ObjectiveType ObjectiveType => ObjectiveType.Kill;
}

public class PlayerInputObjectiveIDs : ObjectiveIDs
{
    public static readonly string Move = "player_move";
    public static readonly string Jump = "player_jump";
    public static readonly string defend = "player_defend";
    public static readonly string aim = "player_aim";
    public static readonly string leftCounterAttack = "left_counter_attack";
    public static readonly string rightCounterAttack = "right_counter_attack";
    public static readonly string swordTeleport = "sword_teleport";
    public static readonly string SwordJump = "sword_jump";

    public override ObjectiveType ObjectiveType => ObjectiveType.PlayerInput;
}

public class HeartSwordObjectiveIDs : ObjectiveIDs
{
    public static readonly string HS_CounterAttack_released = "hs_attack_released";
    public static readonly string HS_CriticalSlash_released = "hs_CriticalSlash_released";
    public static readonly string HS_Waves_released = "hs_wave_released";

    public static readonly string HS_CounterAttack_hit = "hs_attack_hit";
    public static readonly string HS_CriticalSlash_hit = "hs_CriticalSlash_hit";
    public static readonly string HS_Waves_hit = "hs_wave_hit";
    public override ObjectiveType ObjectiveType => ObjectiveType.HeartSword;
}

// Collect objectives
public class CollectObjectiveIDs : ObjectiveIDs
{
    public static readonly string CollectGem = "collect_gem";
    public static readonly string CollectCoin = "collect_coin";
    public override ObjectiveType ObjectiveType => ObjectiveType.Collect;
}

// Explore objectives
public class ExploreObjectiveIDs : ObjectiveIDs
{
    public static readonly string ExploreCave = "explore_cave";
    public static readonly string ExploreForest = "explore_forest";
    public override ObjectiveType ObjectiveType => ObjectiveType.Explore;
}

// Talk objectives
public class TalkObjectiveIDs : ObjectiveIDs
{
    public static readonly string TalkToNPC = "talk_to_npc";
    public static readonly string TalkToElder = "talk_to_elder";
    public override ObjectiveType ObjectiveType => ObjectiveType.Talk;
}

// Combat objectives
public class CombatObjectiveIDs : ObjectiveIDs
{
    public static readonly string CT_norm_Proj = "CT_norm_Proj";
    public static readonly string CT_perf_Proj = "CT_perf_Proj";
    public static readonly string CT_Melee = "CT_Melee";
    public override ObjectiveType ObjectiveType => ObjectiveType.Combat;
}

// Custom objectives
public class CustomObjectiveIDs : ObjectiveIDs
{
    public static readonly string Defend = "defend";
    public static readonly string SpecialAction = "special_action";
    public override ObjectiveType ObjectiveType => ObjectiveType.Custom;
}