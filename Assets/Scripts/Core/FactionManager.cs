using System.Collections.Generic;
using UnityEngine;
using AutoChess.Data;

namespace AutoChess.Core
{
    public class FactionManager : MonoBehaviour
    {
        public static FactionManager Instance { get; private set; }

        [SerializeField] private List<FactionData> allFactions;

        public List<FactionData> AllFactions => allFactions;

        void Awake()
        {
            Instance = this;
        }

        public void Setup(List<FactionData> factions)
        {
            allFactions = new List<FactionData>(factions);
        }

        public void ApplyFactionsToPieces(List<ChessPiece> pieces)
        {
            if (allFactions == null || allFactions.Count == 0) return;

            // Count faction occurrences among alive pieces
            var factionCounts = new Dictionary<string, int>();
            foreach (var piece in pieces)
            {
                if (piece == null || !piece.IsAlive) continue;
                if (piece.heroData == null || piece.heroData.factions == null) continue;

                foreach (var factionName in piece.heroData.factions)
                {
                    if (!factionCounts.ContainsKey(factionName))
                        factionCounts[factionName] = 0;
                    factionCounts[factionName]++;
                }
            }

            // Find active thresholds for each faction
            var activeBonuses = new Dictionary<string, FactionThreshold>();
            foreach (var faction in allFactions)
            {
                if (faction == null || faction.thresholds == null) continue;
                if (!factionCounts.ContainsKey(faction.factionName)) continue;

                int count = factionCounts[faction.factionName];
                FactionThreshold bestThreshold = null;
                foreach (var threshold in faction.thresholds)
                {
                    if (count >= threshold.count)
                        bestThreshold = threshold;
                }
                if (bestThreshold != null)
                    activeBonuses[faction.factionName] = bestThreshold;
            }

            // Apply bonuses to each piece
            foreach (var piece in pieces)
            {
                if (piece == null || !piece.IsAlive) continue;
                if (piece.heroData == null || piece.heroData.factions == null) continue;

                int totalHealthBonus = 0;
                int totalAttackBonus = 0;
                float totalAttackSpeedBonus = 0f;

                foreach (var factionName in piece.heroData.factions)
                {
                    if (activeBonuses.ContainsKey(factionName))
                    {
                        var threshold = activeBonuses[factionName];
                        totalHealthBonus += threshold.healthBonus;
                        totalAttackBonus += threshold.attackBonus;
                        totalAttackSpeedBonus += threshold.attackSpeedBonus;
                    }
                }

                if (totalHealthBonus != 0 || totalAttackBonus != 0 || totalAttackSpeedBonus != 0f)
                {
                    piece.ApplyFactionBonuses(totalHealthBonus, totalAttackBonus, totalAttackSpeedBonus);
                }
            }

            Debug.Log($"Applied factions. Active: {string.Join(", ", activeBonuses.Keys)}");
        }

        public void ClearFactionsFromPieces(List<ChessPiece> pieces)
        {
            foreach (var piece in pieces)
            {
                if (piece != null)
                    piece.ClearFactionBonuses();
            }
        }

        public Dictionary<string, int> GetActiveFactions(List<ChessPiece> pieces)
        {
            var result = new Dictionary<string, int>();
            if (allFactions == null) return result;

            var factionCounts = new Dictionary<string, int>();
            foreach (var piece in pieces)
            {
                if (piece == null || !piece.IsAlive) continue;
                if (piece.heroData?.factions == null) continue;
                foreach (var f in piece.heroData.factions)
                {
                    if (!factionCounts.ContainsKey(f)) factionCounts[f] = 0;
                    factionCounts[f]++;
                }
            }

            foreach (var faction in allFactions)
            {
                if (faction == null || !factionCounts.ContainsKey(faction.factionName)) continue;
                int count = factionCounts[faction.factionName];
                int activeLevel = 0;
                foreach (var t in faction.thresholds)
                {
                    if (count >= t.count) activeLevel = t.count;
                }
                if (activeLevel > 0)
                    result[faction.factionName] = activeLevel;
            }
            return result;
        }
    }
}
