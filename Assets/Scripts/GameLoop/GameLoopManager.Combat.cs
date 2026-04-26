using UnityEngine;
using System.Collections.Generic;
using AutoChess.Data;

namespace AutoChess.Core
{
    public partial class GameLoopManager
    {
        void StartCombatPhase()
        {
            CurrentPhase = GamePhase.Combat;
            uiManager?.HidePieceDetail();
            uiManager?.ShowPhase("战斗阶段", gameConfig.combatMaxDuration);

            var human = HumanPlayer;
            if (human != null && human.IsAlive)
                AutoFillBoard(human);

            foreach (var player in allPlayers)
            {
                if (!player.IsAlive) continue;
                var pb = new PlayerBoard();
                pb.SaveFrom(boardManager, player);
                playerBoards[player] = pb;
                Debug.Log($"[Combat SaveFrom] {player.playerName}: board={pb.boardInfos.Count}, bench={pb.benchInfos.Count}");
            }

            isCreepRound = creepManager != null && creepManager.IsCreepRound(CurrentRound);
            currentCreepData = isCreepRound ? creepManager.GetCreepData(CurrentRound) : null;

            if (augmentManager != null)
            {
                foreach (var player in allPlayers)
                {
                    if (!player.IsAlive) continue;
                    augmentManager.ApplyAugmentCombatBuffs(player.boardPieces, player);
                }
            }

            if (human != null && human.IsAlive)
            {
                bool hasBoardPieces = human.boardPieces.Count > 0;
                if (!hasBoardPieces)
                {
                    Debug.Log("[Combat] Human has no pieces on board — instant loss.");
                    SimulateAIVersusAI();
                    EndCombat(false);
                    return;
                }

                if (isCreepRound && currentCreepData != null)
                {
                    uiManager?.ShowMatchup(human.playerName, currentCreepData.roundName);
                    creepManager.SpawnCreeps(currentCreepData);
                    combatManager.StartCombat(human, creepManager.GetCreepPlayer());
                }
                else
                {
                    var opponent = SelectOpponent(human);
                    if (opponent != null && opponent.IsAlive)
                    {
                        uiManager?.ShowMatchup(human.playerName, opponent.playerName);
                        uiManager?.UpdateOpponentHealth(opponent);
                        LoadOpponentMirror(opponent);
                        combatManager.StartCombat(human, opponent);
                    }
                    else
                    {
                        EndCombat(true);
                        return;
                    }
                }
            }
            else
            {
                if (isCreepRound)
                    SimulateAICreepRounds();
                else
                    SimulateAIVersusAI();
                CurrentPhase = GamePhase.Result;
                if (phaseCoroutine != null) StopCoroutine(phaseCoroutine);
                phaseCoroutine = StartCoroutine(ResultThenNextRound());
                return;
            }

            if (!isCreepRound)
                SimulateAIVersusAI();
            else
                SimulateAICreepRounds();
        }

        PlayerData SelectOpponent(PlayerData player)
        {
            var candidates = new List<PlayerData>();
            foreach (var p in allPlayers)
            {
                if (p != player && p.IsAlive)
                    candidates.Add(p);
            }

            if (candidates.Count == 0) return null;
            if (candidates.Count == 1) return candidates[0];

            if (player.lastOpponent != null && candidates.Contains(player.lastOpponent))
            {
                candidates.Remove(player.lastOpponent);
            }

            var picked = candidates[Random.Range(0, candidates.Count)];
            player.lastOpponent = picked;
            picked.lastOpponent = player;
            return picked;
        }

        void LoadOpponentMirror(PlayerData opponent)
        {
            int playerRows = gameConfig.boardRows;
            for (int r = playerRows; r < playerRows * 2; r++)
            {
                for (int c = 0; c < gameConfig.boardCols; c++)
                {
                    var slot = boardManager.GetSlot(r, c);
                    if (slot != null && slot.piece != null)
                    {
                        var piece = slot.piece;
                        if (piece.owner != null)
                            piece.owner.boardPieces.Remove(piece);
                        boardManager.RemoveFromTracking(piece);
                        Destroy(piece.gameObject);
                        slot.piece = null;
                    }
                }
            }

            if (!playerBoards.ContainsKey(opponent))
            {
                Debug.LogWarning($"[LoadOpponentMirror] No saved board for {opponent.playerName}");
                return;
            }

            var pb = playerBoards[opponent];
            Debug.Log($"[LoadOpponentMirror] {opponent.playerName}: {pb.boardInfos.Count} board pieces, {pb.benchInfos.Count} bench pieces");

            foreach (var info in pb.boardInfos)
            {
                if (info.heroData == null) continue;
                int mirroredRow = (playerRows * 2 - 1) - info.boardPos.x;
                int col = info.boardPos.y;
                var slot = boardManager.GetSlot(mirroredRow, col);
                if (slot == null)
                {
                    Debug.LogWarning($"[LoadOpponentMirror] Slot({mirroredRow},{col}) is null for {info.heroData.heroName}");
                    continue;
                }
                if (slot.IsOccupied)
                {
                    Debug.LogWarning($"[LoadOpponentMirror] Slot({mirroredRow},{col}) occupied by {slot.piece.heroData.heroName} for {info.heroData.heroName}");
                    continue;
                }

                var piece = boardManager.SpawnPiece(info.heroData, opponent, info.starLevel);
                bool placed = boardManager.PlacePiece(piece, mirroredRow, col);
                if (!placed)
                    Debug.LogWarning($"[LoadOpponentMirror] PlacePiece failed for {info.heroData.heroName} at ({mirroredRow},{col})");
                piece.transform.rotation = Quaternion.Euler(0, 180, 0);
            }
        }

        void ClearEnemySide()
        {
            int playerRows = gameConfig.boardRows;
            for (int r = playerRows; r < playerRows * 2; r++)
            {
                for (int c = 0; c < gameConfig.boardCols; c++)
                {
                    var slot = boardManager.GetSlot(r, c);
                    if (slot != null && slot.piece != null)
                    {
                        var piece = slot.piece;
                        if (piece.owner != null)
                            piece.owner.boardPieces.Remove(piece);
                        boardManager.RemoveFromTracking(piece);
                        Destroy(piece.gameObject);
                        slot.piece = null;
                    }
                }
            }
        }
    }
}
