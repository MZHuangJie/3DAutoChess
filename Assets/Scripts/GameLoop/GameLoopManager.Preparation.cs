using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AutoChess.Data;

namespace AutoChess.Core
{
    public partial class GameLoopManager
    {
        void StartPreparationPhase()
        {
            CurrentPhase = GamePhase.Preparation;
            PhaseTimer = CurrentRound == 1 ? 5f : gameConfig.preparationDuration;
            uiManager?.ShowPhase("准备阶段", PhaseTimer);

            bool isFirstRound = CurrentRound == 1;

            foreach (var player in allPlayers)
            {
                if (!player.IsAlive) continue;

                if (!isFirstRound)
                    economyManager.GrantRoundIncome(player, CurrentRound);

                if (!isFirstRound)
                {
                    player.exp += 2;
                    player.CheckLevelUp(gameConfig);
                }

                if (!isFirstRound)
                {
                    player.freeRefreshRemaining = player.freeRefreshPerRound;
                    if (augmentManager != null)
                        augmentManager.ApplyExpPerRound(player, gameConfig);
                }

                if (!isFirstRound && !player.shopLocked)
                {
                    shopManager.RefreshShop(player, true);
                }
                else if (!isFirstRound && player.shopLocked)
                {
                    player.shopLocked = false;
                }

                if (!player.isHuman)
                {
                    aiManager?.MakeDecisions(player, playerBoards.ContainsKey(player) ? playerBoards[player] : null);
                    var pb = new PlayerBoard();
                    pb.SaveFrom(boardManager, player);
                    playerBoards[player] = pb;
                }
            }

            var human = HumanPlayer;
            if (human != null && human.IsAlive)
            {
                ResetHumanPiecesForPreparation(human);
            }

            uiManager?.UpdateUI();

            if (phaseCoroutine != null) StopCoroutine(phaseCoroutine);
            phaseCoroutine = StartCoroutine(PreparationTimer());
        }

        IEnumerator PreparationTimer()
        {
            while (PhaseTimer > 0)
            {
                PhaseTimer -= Time.deltaTime;
                uiManager?.UpdateTimer(PhaseTimer);
                yield return null;
            }
            StartCombatPhase();
        }

        void AutoFillBoard(PlayerData player)
        {
            int currentOnBoard = player.GetCurrentBoardUnitCount();
            int maxOnBoard = player.GetMaxUnitsOnBoard();
            int slotsToFill = maxOnBoard - currentOnBoard;
            if (slotsToFill <= 0 || player.benchPieces.Count == 0) return;

            var benchCopy = new List<ChessPiece>(player.benchPieces);
            for (int i = benchCopy.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (benchCopy[i], benchCopy[j]) = (benchCopy[j], benchCopy[i]);
            }

            int placed = 0;
            foreach (var piece in benchCopy)
            {
                if (placed >= slotsToFill) break;
                if (piece == null || !piece.IsAlive) continue;

                for (int r = 0; r < gameConfig.boardRows; r++)
                {
                    bool found = false;
                    for (int c = 0; c < gameConfig.boardCols; c++)
                    {
                        var slot = boardManager.GetSlot(r, c);
                        if (slot != null && !slot.IsOccupied)
                        {
                            boardManager.PlacePiece(piece, r, c);
                            placed++;
                            found = true;
                            break;
                        }
                    }
                    if (found) break;
                }
            }

            if (placed > 0)
                Debug.Log($"[AutoFill] {player.playerName}: placed {placed} pieces from bench to board");
        }

        void ResetHumanPiecesForPreparation(PlayerData human)
        {
            foreach (var piece in human.boardPieces)
            {
                if (piece == null) continue;
                var slot = boardManager.GetSlot(piece.boardPosition);
                if (slot != null)
                {
                    piece.ResetForPreparation(slot.worldPos);
                }
                else
                {
                    piece.ResetForPreparation(piece.transform.position);
                }
            }
            foreach (var piece in human.benchPieces)
            {
                if (piece == null) continue;
                piece.gameObject.SetActive(true);
                piece.state = PieceState.Idle;
            }
        }
    }
}
