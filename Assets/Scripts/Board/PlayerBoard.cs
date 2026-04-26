using System.Collections.Generic;
using UnityEngine;
using AutoChess.Data;

namespace AutoChess.Core
{
    [System.Serializable]
    public class BoardPieceInfo
    {
        public HeroData heroData;
        public int starLevel;
        public Vector2Int boardPos; // valid if on board
        public int benchIndex;      // valid if on bench
        public bool isOnBench;
    }

    public class PlayerBoard
    {
        public List<BoardPieceInfo> boardInfos = new List<BoardPieceInfo>();
        public List<BoardPieceInfo> benchInfos = new List<BoardPieceInfo>();

        public void SaveFrom(BoardManager board, PlayerData player)
        {
            boardInfos.Clear();
            benchInfos.Clear();

            var allPieces = board.GetPiecesByOwner(player);
            foreach (var piece in allPieces)
            {
                if (piece == null) continue;
                var info = new BoardPieceInfo
                {
                    heroData = piece.heroData,
                    starLevel = piece.starLevel,
                    boardPos = piece.boardPosition,
                    benchIndex = piece.benchIndex,
                    isOnBench = piece.isOnBench
                };
                if (piece.isOnBench)
                    benchInfos.Add(info);
                else
                    boardInfos.Add(info);
            }
        }

        public void LoadTo(BoardManager board, PlayerData player, int enemyRowOffset = 0)
        {
            // Clear existing pieces for this player on board
            var existing = board.GetPiecesByOwner(player, false);
            foreach (var p in existing)
            {
                if (p != null && p.gameObject != null)
                {
                    board.RemovePieceFromAnywhere(p);
                    board.RemoveFromTracking(p);
                    Object.Destroy(p.gameObject);
                }
            }
            player.boardPieces.Clear();
            player.benchPieces.Clear();

            // Spawn board pieces
            foreach (var info in boardInfos)
            {
                if (info.heroData == null) continue;
                int row = info.boardPos.x + enemyRowOffset;
                int col = info.boardPos.y;
                var slot = board.GetSlot(row, col);
                if (slot == null || slot.IsOccupied) continue;

                var piece = board.SpawnPiece(info.heroData, player, info.starLevel);
                board.PlacePiece(piece, row, col);
                piece.transform.rotation = Quaternion.Euler(0, enemyRowOffset > 0 ? 180 : 0, 0);
            }

            // Spawn bench pieces (only if no offset, i.e. loading own board)
            if (enemyRowOffset == 0)
            {
                foreach (var info in benchInfos)
                {
                    if (info.heroData == null) continue;
                    var slot = board.GetBenchSlot(info.benchIndex);
                    if (slot == null || slot.IsOccupied) continue;

                    var piece = board.SpawnPiece(info.heroData, player, info.starLevel);
                    board.PlacePieceOnBench(piece, info.benchIndex);
                }
            }
        }
    }
}
