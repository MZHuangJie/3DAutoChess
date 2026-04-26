using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using AutoChess.Data;

namespace AutoChess.Core
{
    public class CombatManager : MonoBehaviour
    {
        public static CombatManager Instance { get; private set; }

        [SerializeField] private GameConfig gameConfig;

        private List<ChessPiece> allCombatants = new List<ChessPiece>();
        private bool combatActive = false;
        private float combatTimer = 0f;
        private bool isOvertime = false;
        private float overtimeTimer = 0f;
        private bool isDraw = false;
        private PlayerData currentAttacker;
        private PlayerData currentDefender;

        public bool IsCombatActive => combatActive;
        public bool IsOvertime => isOvertime;
        public bool IsDraw => isDraw;

        public float RemainingTime
        {
            get
            {
                if (!combatActive) return 0f;
                if (isOvertime)
                    return Mathf.Max(0, gameConfig.combatOvertimeDuration - overtimeTimer);
                return Mathf.Max(0, gameConfig.combatMaxDuration - combatTimer);
            }
        }

        void Awake()
        {
            Instance = this;
        }

        public void StartCombat(PlayerData attacker, PlayerData defender)
        {
            currentAttacker = attacker;
            currentDefender = defender;
            allCombatants.Clear();

            var boardPieces = BoardManager.Instance.GetAllPiecesOnBoard();
            foreach (var p in boardPieces)
            {
                if (p.owner == attacker || p.owner == defender)
                    allCombatants.Add(p);
            }

            foreach (var piece in allCombatants)
            {
                piece.OnCombatStart();
            }

            // Apply faction bonuses
            FactionManager.Instance?.ApplyFactionsToPieces(allCombatants);

            combatActive = true;
            combatTimer = 0f;
            isOvertime = false;
            overtimeTimer = 0f;
            isDraw = false;
            Debug.Log($"Combat started: {attacker.playerName} vs {defender.playerName} with {allCombatants.Count} pieces.");
        }

        void Update()
        {
            if (!combatActive) return;

            float dt = Time.deltaTime;
            if (isOvertime)
                overtimeTimer += dt;
            else
                combatTimer += dt;

            for (int i = 0; i < allCombatants.Count; i++)
            {
                var piece = allCombatants[i];
                if (piece == null || !piece.IsAlive) continue;
                piece.TickCombat(dt, gameConfig);
            }

            CheckCombatEnd();
        }

        void CheckCombatEnd()
        {
            if (currentAttacker == null || currentDefender == null) return;

            bool attackerAlive = allCombatants.Any(p => p.owner == currentAttacker && p.IsAlive);
            bool defenderAlive = allCombatants.Any(p => p.owner == currentDefender && p.IsAlive);

            // One side wiped out
            if (!attackerAlive || !defenderAlive)
            {
                combatActive = false;
                isDraw = false;
                bool attackerWon = attackerAlive;
                GameLoopManager.Instance?.EndCombat(attackerWon);
                return;
            }

            if (!isOvertime)
            {
                // Regular timer expired, both sides alive → enter overtime
                if (combatTimer >= gameConfig.combatMaxDuration)
                {
                    isOvertime = true;
                    overtimeTimer = 0f;
                    Debug.Log("Combat entering overtime!");
                }
                return;
            }

            // Overtime expired, both sides still alive → draw
            if (overtimeTimer >= gameConfig.combatOvertimeDuration)
            {
                combatActive = false;
                isDraw = true;
                Debug.Log("Overtime expired! Draw — both sides take damage.");
                GameLoopManager.Instance?.EndCombat(true);
            }
        }

        public ChessPiece FindNearestEnemy(ChessPiece self)
        {
            ChessPiece nearest = null;
            float nearestDist = float.MaxValue;

            foreach (var other in allCombatants)
            {
                if (!other.IsAlive) continue;
                if (other.owner == self.owner) continue; // same team

                float dist = Vector3.Distance(self.transform.position, other.transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = other;
                }
            }

            return nearest;
        }

        public int GetAliveCount(PlayerData player)
        {
            return allCombatants.Count(p => p.owner == player && p.IsAlive);
        }

        public int GetAliveStarCount(PlayerData player)
        {
            int sum = 0;
            foreach (var p in allCombatants)
                if (p.owner == player && p.IsAlive) sum += p.starLevel;
            return sum;
        }

        public List<ChessPiece> GetEnemiesInRange(ChessPiece caster, float range)
        {
            var result = new List<ChessPiece>();
            foreach (var other in allCombatants)
            {
                if (!other.IsAlive || other.owner == caster.owner) continue;
                if (Vector3.Distance(caster.transform.position, other.transform.position) <= range * 1.2f)
                    result.Add(other);
            }
            return result;
        }

        public ChessPiece GetLowestHpAlly(ChessPiece caster)
        {
            ChessPiece lowest = null;
            int lowestHp = int.MaxValue;
            foreach (var other in allCombatants)
            {
                if (!other.IsAlive || other.owner != caster.owner) continue;
                if (other.currentHealth < lowestHp)
                {
                    lowestHp = other.currentHealth;
                    lowest = other;
                }
            }
            return lowest;
        }
    }
}
