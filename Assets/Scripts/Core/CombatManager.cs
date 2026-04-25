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
        private PlayerData currentAttacker;
        private PlayerData currentDefender;

        public bool IsCombatActive => combatActive;

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
            Debug.Log($"Combat started: {attacker.playerName} vs {defender.playerName} with {allCombatants.Count} pieces.");
        }

        void Update()
        {
            if (!combatActive) return;

            float dt = Time.deltaTime;
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

            bool timeout = combatTimer >= gameConfig.combatMaxDuration;

            if (!attackerAlive || !defenderAlive || timeout)
            {
                combatActive = false;
                bool attackerWon;
                if (timeout)
                {
                    int attackerHp = allCombatants.Where(p => p.owner == currentAttacker && p.IsAlive).Sum(p => p.currentHealth);
                    int defenderHp = allCombatants.Where(p => p.owner == currentDefender && p.IsAlive).Sum(p => p.currentHealth);
                    attackerWon = attackerHp >= defenderHp;
                    Debug.Log($"Combat timeout! Attacker HP={attackerHp}, Defender HP={defenderHp}");
                }
                else
                {
                    attackerWon = attackerAlive;
                }
                GameLoopManager.Instance?.EndCombat(attackerWon);
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
