using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AutoChess.Data;

namespace AutoChess.Core
{
    public partial class GameLoopManager
    {
        private bool carouselHumanPicked = false;

        void StartCarouselPhase()
        {
            CurrentPhase = GamePhase.Carousel;

            var heroes = availableHeroes.ToArray();
            var baseEquips = availableEquipment.FindAll(e => e.equipmentType == EquipmentType.Base).ToArray();
            currentCarouselItems = carouselManager?.GenerateItems(heroes, baseEquips);
            carouselHumanPicked = false;

            if (phaseCoroutine != null) StopCoroutine(phaseCoroutine);
            phaseCoroutine = StartCoroutine(CarouselWaveRoutine());
        }

        IEnumerator CarouselWaveRoutine()
        {
            var pickOrder = carouselManager?.GetPickOrder(allPlayers);
            if (pickOrder == null || currentCarouselItems == null)
            {
                StartPreparationPhase();
                yield break;
            }

            uiManager?.ShowCarouselSelection(currentCarouselItems, false);

            int waveSize = 2;
            int index = 0;

            while (index < pickOrder.Count)
            {
                var wave = new List<PlayerData>();
                for (int i = 0; i < waveSize && index < pickOrder.Count; i++, index++)
                    wave.Add(pickOrder[index]);

                bool humanInWave = wave.Exists(p => p.isHuman);
                if (humanInWave)
                    carouselHumanPicked = false;

                var aiPickDelays = new Dictionary<PlayerData, float>();
                foreach (var player in wave)
                {
                    if (!player.isHuman)
                        aiPickDelays[player] = Random.Range(1f, 8f);
                }

                PhaseTimer = 10f;
                string waveNames = string.Join(", ", wave.ConvertAll(p => p.playerName));
                uiManager?.ShowPhase($"选秀轮 - {waveNames}", PhaseTimer);
                uiManager?.ShowCarouselSelection(currentCarouselItems, humanInWave && !carouselHumanPicked);

                float elapsed = 0f;
                bool allDone = false;

                while (PhaseTimer > 0 && !allDone)
                {
                    float dt = Time.deltaTime;
                    PhaseTimer -= dt;
                    elapsed += dt;
                    uiManager?.UpdateTimer(PhaseTimer);

                    foreach (var kvp in new Dictionary<PlayerData, float>(aiPickDelays))
                    {
                        if (elapsed >= kvp.Value)
                        {
                            int idx = carouselManager.AIChoose(currentCarouselItems);
                            if (idx >= 0) carouselManager.PickItem(kvp.Key, idx);
                            aiPickDelays.Remove(kvp.Key);
                            uiManager?.ShowCarouselSelection(currentCarouselItems, humanInWave && !carouselHumanPicked);
                        }
                    }

                    bool aisDone = aiPickDelays.Count == 0;
                    bool humanDone = !humanInWave || carouselHumanPicked;
                    allDone = aisDone && humanDone;

                    yield return null;
                }

                foreach (var player in wave)
                {
                    if (!player.isHuman && aiPickDelays.ContainsKey(player))
                    {
                        int idx = carouselManager.AIChoose(currentCarouselItems);
                        if (idx >= 0) carouselManager.PickItem(player, idx);
                    }
                }
                if (humanInWave && !carouselHumanPicked)
                {
                    var human = HumanPlayer;
                    for (int i = 0; i < currentCarouselItems.Count; i++)
                    {
                        if (!currentCarouselItems[i].picked)
                        {
                            if (human != null && human.IsAlive)
                                carouselManager.PickItem(human, i);
                            carouselHumanPicked = true;
                            break;
                        }
                    }
                }

                uiManager?.ShowCarouselSelection(currentCarouselItems, false);
            }

            yield return new WaitForSeconds(1.5f);
            uiManager?.HideCarouselSelection();
            carouselHumanPicked = false;
            StartPreparationPhase();
        }

        public void OnCarouselSelected(int index)
        {
            if (CurrentPhase != GamePhase.Carousel) return;
            if (carouselHumanPicked) return;

            var human = HumanPlayer;
            if (human != null && human.IsAlive)
                carouselManager?.PickItem(human, index);

            carouselHumanPicked = true;
            uiManager?.ShowCarouselSelection(currentCarouselItems, false);
        }
    }
}
