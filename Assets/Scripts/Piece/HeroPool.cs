using System.Collections.Generic;
using UnityEngine;
using AutoChess.Data;

namespace AutoChess.Core
{
    public class HeroPool : MonoBehaviour
    {
        public static HeroPool Instance { get; private set; }

        [SerializeField] private GameConfig gameConfig;
        [SerializeField] private List<HeroData> allHeroes;
        public List<HeroData> AllHeroes => allHeroes;

        // heroData -> remaining count in pool
        private Dictionary<HeroData, int> pool = new Dictionary<HeroData, int>();
        private System.Random rng = new System.Random();

        void Awake()
        {
            Instance = this;
        }

        public void Initialize(List<HeroData> heroes, GameConfig config)
        {
            allHeroes = new List<HeroData>(heroes);
            gameConfig = config;
            pool.Clear();

            foreach (var hero in allHeroes)
            {
                int cost = Mathf.Clamp(hero.cost, 1, 5);
                int count = config.poolCountByCost[cost];
                pool[hero] = count;
            }
        }

        public HeroData DrawHero(int playerLevel)
        {
            if (allHeroes == null || allHeroes.Count == 0) return null;

            // 1. Determine cost tier based on level probability
            int[] prob = gameConfig.GetRollProbability(Mathf.Clamp(playerLevel, 1, 10));
            int roll = Random.Range(0, 100);
            int cumulative = 0;
            int selectedCost = 1;
            for (int i = 0; i < prob.Length; i++)
            {
                cumulative += prob[i];
                if (roll < cumulative)
                {
                    selectedCost = i + 1;
                    break;
                }
            }

            // 2. Find all heroes of that cost with remaining pool
            var candidates = new List<HeroData>();
            foreach (var hero in allHeroes)
            {
                if (hero.cost == selectedCost && pool.ContainsKey(hero) && pool[hero] > 0)
                {
                    candidates.Add(hero);
                }
            }

            // 3. If no candidates at selected cost, try other costs (fallback)
            if (candidates.Count == 0)
            {
                for (int cost = 1; cost <= 5; cost++)
                {
                    foreach (var hero in allHeroes)
                    {
                        if (hero.cost == cost && pool.ContainsKey(hero) && pool[hero] > 0)
                            candidates.Add(hero);
                    }
                    if (candidates.Count > 0) break;
                }
            }

            if (candidates.Count == 0) return null;

            // 4. Random pick from candidates
            var picked = candidates[Random.Range(0, candidates.Count)];
            pool[picked]--;
            return picked;
        }

        public HeroData DrawHeroByCost(int cost)
        {
            var candidates = new List<HeroData>();
            foreach (var hero in allHeroes)
            {
                if (hero.cost == cost && pool.ContainsKey(hero) && pool[hero] > 0)
                    candidates.Add(hero);
            }
            if (candidates.Count == 0) return null;
            var picked = candidates[Random.Range(0, candidates.Count)];
            pool[picked]--;
            return picked;
        }

        public void ReturnHero(HeroData hero)
        {
            if (hero == null) return;
            if (pool.ContainsKey(hero))
                pool[hero]++;
        }

        public int GetRemainingCount(HeroData hero)
        {
            if (hero == null) return 0;
            return pool.ContainsKey(hero) ? pool[hero] : 0;
        }
    }
}
