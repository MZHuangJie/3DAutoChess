using UnityEngine;
using System.Collections.Generic;
using AutoChess.Data;
using AutoChess.Core;

namespace AutoChess.AI
{
    public class AIManager : MonoBehaviour
    {
        [SerializeField] private GameConfig gameConfig;
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private AIController aiController;

        public void Setup(GameConfig config, BoardManager board, AIController controller)
        {
            gameConfig = config;
            boardManager = board;
            aiController = controller;
        }

        public void MakeDecisions(PlayerData aiPlayer, PlayerBoard playerBoard)
        {
            aiController?.MakeDecisions(aiPlayer, playerBoard);
        }

        public void PlaceAIPieces(PlayerData aiPlayer, PlayerBoard playerBoard)
        {
            if (aiPlayer == null || playerBoard == null) return;
            // AI pieces are placed on a virtual board, not visible to player
            // The PlayerBoard saves their positions for mirror loading
            playerBoard.SaveFrom(boardManager, aiPlayer);
        }

        // Legacy MVP method - kept for compatibility but not used in Milestone 2
        public void RandomizeAIPieces(PlayerData aiPlayer)
        {
            // Clear old
            foreach (var oldPiece in aiPlayer.boardPieces)
            {
                if (oldPiece != null) Destroy(oldPiece.gameObject);
            }
            aiPlayer.boardPieces.Clear();

            int playerRows = gameConfig.boardRows;
            int cols = gameConfig.boardCols;

            int pieceCount = Random.Range(2, 5);
            for (int i = 0; i < pieceCount; i++)
            {
                var hero = CreateRandomHeroData(i);
                var piece = boardManager.SpawnPiece(hero, aiPlayer);
                aiPlayer.boardPieces.Add(piece);
            }

            foreach (var piece in aiPlayer.boardPieces)
            {
                if (piece == null) continue;
                int attempts = 0;
                bool placed = false;
                while (!placed && attempts < 50)
                {
                    int r = Random.Range(playerRows, playerRows * 2);
                    int c = Random.Range(0, cols);
                    if (boardManager.GetSlot(r, c) != null && !boardManager.GetSlot(r, c).IsOccupied)
                    {
                        boardManager.PlacePiece(piece, r, c);
                        piece.transform.rotation = Quaternion.Euler(0, 180, 0);
                        placed = true;
                    }
                    attempts++;
                }
            }
        }

        HeroData CreateRandomHeroData(int seed)
        {
            var hero = ScriptableObject.CreateInstance<HeroData>();
            hero.heroName = $"AI_Hero_{seed}";
            hero.cost = Random.Range(1, 5);
            hero.displayColor = Random.ColorHSV();
            hero.maxHealth = Random.Range(400, 800);
            hero.attackDamage = Random.Range(40, 80);
            hero.attackSpeed = Random.Range(0.5f, 1.2f);
            hero.armor = Random.Range(10, 40);
            hero.magicResist = Random.Range(10, 40);
            hero.attackRange = Random.Range(1f, 4f);
            hero.maxMana = 100;
            hero.startingMana = 0;
            hero.attackType = hero.attackRange > 2f ? AttackType.Ranged : AttackType.Melee;
            return hero;
        }
    }
}
