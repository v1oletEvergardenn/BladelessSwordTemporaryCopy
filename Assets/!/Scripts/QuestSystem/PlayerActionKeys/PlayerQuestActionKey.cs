using UnityEngine;

[CreateAssetMenu(fileName = "Player", menuName = "QuestSystem/Player Action Key")]
public class PlayerQuestActionKey : QuestActionKey
{
    public string Move => ActionID("player_move");
    public string Jump => ActionID("player_jump");
    public string defend => ActionID("player_defend");
    public string aim => ActionID("player_aim");
    public string leftCounterAttack => ActionID("left_counter_attack");
    public string rightCounterAttack => ActionID("right_counter_attack");
    public string swordTeleport => ActionID("sword_teleport");
    public string SwordJump => ActionID("sword_jump");

    public string CT_Proj => ActionID("CT_Proj");
    public string CT_norm_Proj => ActionID("CT_norm_Proj");
    public string CT_perf_Proj => ActionID("CT_perf_Proj");
    public string CT_Melee => ActionID("CT_Melee");

    public string HS_CounterAttack_released => ActionID("hs_attack_released");
    public string HS_CriticalSlash_released => ActionID("hs_CriticalSlash_released");
    public string HS_Waves_released => ActionID("hs_wave_released");
    public string HS_CounterAttack_hit => ActionID("hs_attack_hit");
    public string HS_CriticalSlash_hit => ActionID("hs_CriticalSlash_hit");
    public string HS_Waves_hit => ActionID("hs_wave_hit");
    //HeartSword actions
}