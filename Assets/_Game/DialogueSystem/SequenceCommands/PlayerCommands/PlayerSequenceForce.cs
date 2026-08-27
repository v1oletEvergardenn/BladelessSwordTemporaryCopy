using System.Collections;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    public static class PlayerSequenceForce
    {
        public static bool ForceAttack(bool attackLeft)
        {
            var player = PlayerControl.instance;
            var attack = PlayerAttack.instance;
            var energy = Energy.instance;
            if (player == null || attack == null) return false;

            ActionLock.ClearAll();
            if (Health.instance != null) Health.instance.stunned = false;
            if (energy != null) energy.currentEnergy = energy.maxEnergy;

            attack.attackTimer = attack.attackGap + 1f;

            if (player.FacingRight == attackLeft) player.ForceFlip();

            attack.InitializeAttack(attackLeft);
            attack.anim.SetBool("isCombat", true);
            attack.anim.Play(player.GetAttackAnimName());
            return true;
        }

        public static bool ForceAttackHS(bool attackLeft)
        {
            var hs = HeartSwordAbilities.instance;
            if (hs == null || hs.hs_CounterAttack == null) return false;
            return ForceHSAbility(hs.hs_CounterAttack, attackLeft);
        }

        public static bool ForceHS(string abilityName, bool attackLeft)
        {
            var hs = HeartSwordAbilities.instance;
            if (hs == null) return false;

            IHeartSwordAbility ability = ResolveAbility(hs, abilityName);
            if (ability == null) return false;

            return ForceHSAbility(ability, attackLeft);
        }

        public static bool ForceTeleportForward()
        {
            var player = PlayerControl.instance;
            if (player == null) return false;

            ActionLock.ClearAll();
            if (Energy.instance != null) Energy.instance.currentEnergy = Energy.instance.maxEnergy;

            float dir = player.FacingRight ? 1f : -1f;
            Vector3 target = player.transform.position + new Vector3(player.TeleportDistance * dir, 1.2f, 0f);
            player.DesignatedPositionTeleport(target);
            return true;
        }

        public static bool ForceTeleportTo(Vector3 worldPosition)
        {
            var player = PlayerControl.instance;
            if (player == null) return false;

            ActionLock.ClearAll();
            if (Energy.instance != null) Energy.instance.currentEnergy = Energy.instance.maxEnergy;

            player.DesignatedPositionTeleport(worldPosition);
            return true;
        }

        public static bool ForceJump()
        {
            var player = PlayerControl.instance;
            if (player == null) return false;

            ActionLock.ClearAll();
            player.coyoteTimer = Mathf.Max(player.coyoteTimer, 0.25f);
            player.Jump();
            return true;
        }

        public static bool ForceDoubleJump(float holdTime)
        {
            var player = PlayerControl.instance;
            var energy = Energy.instance;
            if (player == null) return false;

            ActionLock.ClearAll();
            if (energy != null) energy.currentEnergy = energy.maxEnergy;
            player.isGrounded = false;
            player.DoubleJump(Mathf.Max(0f, holdTime));
            return true;
        }

        public static IEnumerator WaitTeleportDone(float fallbackSeconds = 1f)
        {
            var player = PlayerControl.instance;
            if (player == null) yield break;

            float timeout = Mathf.Max(0.1f, fallbackSeconds);
            float elapsed = 0f;

            while (elapsed < timeout)
            {
                bool swordActive = player.TeleportSword != null && player.TeleportSword.activeSelf;
                if (!swordActive) break;

                elapsed += Mathf.Max(DialogueTime.deltaTime, 0f);
                yield return null;
            }
        }

        private static bool ForceHSAbility(IHeartSwordAbility ability, bool attackLeft)
        {
            if (ability == null) return false;

            ActionLock.ClearAll();

            if (Health.instance != null) Health.instance.stunned = false;
            if (Energy.instance != null) Energy.instance.currentEnergy = Energy.instance.maxEnergy;

            if (ability.hSAbilityManager != null)
            {
                float cost = ability.GetCurrentAttribute() != null ? ability.GetCurrentAttribute().HS_Cost : 0f;
                float deficit = cost - ability.hSAbilityManager.currentHS_point;
                if (deficit > 0f) ability.hSAbilityManager.ModifyHSPoint(deficit);
                ability.hSAbilityManager.CancelAllAbilities();
            }

            if (PlayerAttack.instance != null)
            {
                PlayerAttack.instance.attackTimer = PlayerAttack.instance.attackGap + 1f;
            }

            if (ability.controller != null && ability.controller.FacingRight == attackLeft)
            {
                ability.controller.ForceFlip();
            }

            bool ok = ability.CheckPerformAbility(attackLeft);
            if (ok) return true;

            ability.CancelAction();
            ability.isActive = true;
            ability.isPerforming = true;
            ability.hsHitEffectPlayed = false;
            ability.hsHitTargets.Clear();

            IEnumerator act =
                ability.GetBranchIndex() == 1 ? ability.FirstBranchAct() :
                ability.GetBranchIndex() == 2 ? ability.SecondBranchAct() :
                ability.OriginalAct();

            ability.co_ability = ability.StartCoroutine(act);
            return true;
        }

        private static IHeartSwordAbility ResolveAbility(HeartSwordAbilities hs, string abilityName)
        {
            if (string.IsNullOrWhiteSpace(abilityName)) return null;
            string n = abilityName.Trim().ToLowerInvariant();

            if (n == "counterattack" || n == "counter" || n == "hs_counterattack") return hs.hs_CounterAttack;
            if (n == "criticalslash" || n == "critical" || n == "hs_criticalslash") return hs.hs_CriticalSlash;
            if (n == "slashwave" || n == "wave" || n == "hs_slashwave") return hs.hs_SlashWave;

            return null;
        }
    }
}