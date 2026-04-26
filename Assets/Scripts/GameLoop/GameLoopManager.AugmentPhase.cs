using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AutoChess.Data;

namespace AutoChess.Core
{
    public partial class GameLoopManager
    {
        void StartAugmentPhase()
        {
            CurrentPhase = GamePhase.AugmentSelect;
            PhaseTimer = gameConfig.augmentSelectDuration;

            int tier = augmentManager != null ? augmentManager.GetTierForRound(CurrentRound, gameConfig.augmentRounds) : 1;
            currentAugmentChoices = augmentManager?.GenerateChoices(tier);

            foreach (var player in allPlayers)
            {
                if (player.isHuman || !player.IsAlive) continue;
                var aiChoices = augmentManager?.GenerateChoices(tier);
                if (aiChoices != null && aiChoices.Count > 0)
                {
                    var pick = augmentManager.AIChoose(aiChoices);
                    augmentManager.ApplyAugment(player, pick);
                }
            }

            uiManager?.ShowPhase("海克斯选择", PhaseTimer);
            uiManager?.ShowAugmentSelection(currentAugmentChoices);

            if (phaseCoroutine != null) StopCoroutine(phaseCoroutine);
            phaseCoroutine = StartCoroutine(AugmentTimer(currentAugmentChoices));
        }

        IEnumerator AugmentTimer(List<AugmentData> choices)
        {
            while (PhaseTimer > 0)
            {
                PhaseTimer -= Time.deltaTime;
                uiManager?.UpdateTimer(PhaseTimer);
                yield return null;
            }
            if (choices != null && choices.Count > 0)
                OnAugmentSelected(0);
        }

        public void OnAugmentSelected(int index)
        {
            if (CurrentPhase != GamePhase.AugmentSelect) return;

            var human = HumanPlayer;
            if (human != null && human.IsAlive && currentAugmentChoices != null && index >= 0 && index < currentAugmentChoices.Count)
            {
                augmentManager?.ApplyAugment(human, currentAugmentChoices[index]);
            }

            currentAugmentChoices = null;
            uiManager?.HideAugmentSelection();

            if (phaseCoroutine != null) StopCoroutine(phaseCoroutine);
            StartPreparationPhase();
        }
    }
}
