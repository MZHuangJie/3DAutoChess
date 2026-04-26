using System.Collections.Generic;
using UnityEngine;

namespace AutoChess.Core
{
    public struct CombatRecord
    {
        public int round;
        public string opponentName;
        public bool won;
        public int damage;
        public bool isPvE;
    }

    public class CombatStatsTracker : MonoBehaviour
    {
        public static CombatStatsTracker Instance { get; private set; }

        private List<CombatRecord> history = new List<CombatRecord>();

        void Awake()
        {
            Instance = this;
        }

        public void RecordResult(int round, string opponent, bool won, int damage, bool isPvE = false)
        {
            history.Add(new CombatRecord
            {
                round = round,
                opponentName = opponent,
                won = won,
                damage = damage,
                isPvE = isPvE
            });
        }

        public List<CombatRecord> GetRecentHistory(int count = 5)
        {
            int start = Mathf.Max(0, history.Count - count);
            return history.GetRange(start, history.Count - start);
        }

        public List<CombatRecord> GetFullHistory()
        {
            return history;
        }

        public void Clear()
        {
            history.Clear();
        }
    }
}
