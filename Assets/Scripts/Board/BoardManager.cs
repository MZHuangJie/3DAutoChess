using UnityEngine;
using System.Collections.Generic;
using AutoChess.Data;

namespace AutoChess.Core
{
    public class BoardSlot
    {
        public Vector2Int gridPos;
        public Vector3 worldPos;
        public ChessPiece piece;
        public bool isPlayerSide;

        public bool IsOccupied => piece != null;
    }

    public class BoardManager : MonoBehaviour
    {
        public static BoardManager Instance { get; private set; }

        [SerializeField] private GameConfig gameConfig;
        [SerializeField] private Transform boardOrigin;
        [SerializeField] private GameObject piecePrefab;
        [SerializeField] private Transform benchOrigin;

        private BoardSlot[,] grid;
        private List<BoardSlot> benchSlots = new List<BoardSlot>();
        private List<ChessPiece> allSpawnedPieces = new List<ChessPiece>();
        private int rows => gameConfig.boardRows * 2;
        private int cols => gameConfig.boardCols;
        private int playerRows => gameConfig.boardRows;

        public GameConfig Config => gameConfig;
        public int Rows => rows;
        public int Cols => cols;
        public int PlayerRows => playerRows;

        // Hex geometry
        public float HexColSpacing => gameConfig.cellSize;
        public float HexRowSpacing => gameConfig.cellSize * 0.866f;

        void Awake()
        {
            Instance = this;
            InitializeGrid();
        }

        float HexRowOffset(int row) => (row % 2 == 1) ? gameConfig.cellSize * 0.5f : 0f;

        Vector3 HexWorldPos(int row, int col)
        {
            return boardOrigin.position + new Vector3(
                col * HexColSpacing + HexRowOffset(row),
                0,
                row * HexRowSpacing
            );
        }

        void InitializeGrid()
        {
            grid = new BoardSlot[rows, cols];

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    grid[r, c] = new BoardSlot
                    {
                        gridPos = new Vector2Int(r, c),
                        worldPos = HexWorldPos(r, c),
                        isPlayerSide = r < playerRows
                    };
                }
            }

            float cellSize = gameConfig.cellSize;
            for (int i = 0; i < gameConfig.benchSlots; i++)
            {
                benchSlots.Add(new BoardSlot
                {
                    gridPos = new Vector2Int(-1, i),
                    worldPos = benchOrigin.position + new Vector3(i * cellSize, 0, 0),
                    isPlayerSide = true
                });
            }
        }

        public BoardSlot GetSlot(int row, int col)
        {
            if (grid == null || row < 0 || row >= rows || col < 0 || col >= cols) return null;
            return grid[row, col];
        }

        public BoardSlot GetSlot(Vector2Int pos) => GetSlot(pos.x, pos.y);

        public BoardSlot GetBenchSlot(int index)
        {
            if (index < 0 || index >= benchSlots.Count) return null;
            return benchSlots[index];
        }

        public Vector3 GetSlotWorldPos(int row, int col)
        {
            var slot = GetSlot(row, col);
            return slot != null ? slot.worldPos : Vector3.zero;
        }

        public bool IsPlayerSide(int row) => row < playerRows;

        public bool PlacePiece(ChessPiece piece, int row, int col)
        {
            var slot = GetSlot(row, col);
            if (slot == null || slot.IsOccupied) return false;

            RemovePieceFromAnywhere(piece);

            if (piece.owner != null)
            {
                piece.owner.benchPieces.Remove(piece);
                if (!piece.owner.boardPieces.Contains(piece))
                    piece.owner.boardPieces.Add(piece);
            }

            slot.piece = piece;
            piece.boardPosition = new Vector2Int(row, col);
            piece.isOnBench = false;
            piece.benchIndex = -1;
            piece.transform.position = slot.worldPos;
            piece.SetHealthBarVisible(true);
            RefreshFactionBonuses(piece.owner);
            return true;
        }

        public bool PlacePieceOnBench(ChessPiece piece, int benchIndex)
        {
            var slot = GetBenchSlot(benchIndex);
            if (slot == null || slot.IsOccupied) return false;

            RemovePieceFromAnywhere(piece);

            if (piece.owner != null)
            {
                piece.owner.boardPieces.Remove(piece);
                if (!piece.owner.benchPieces.Contains(piece))
                    piece.owner.benchPieces.Add(piece);
            }

            slot.piece = piece;
            piece.isOnBench = true;
            piece.benchIndex = benchIndex;
            piece.boardPosition = new Vector2Int(-1, benchIndex);
            piece.transform.position = slot.worldPos;
            piece.gameObject.SetActive(true);
            piece.SetHealthBarVisible(false);
            Debug.Log($"[PlacePieceOnBench] {piece.heroData.heroName} -> bench[{benchIndex}] pos={slot.worldPos} active={piece.gameObject.activeSelf}");
            RefreshFactionBonuses(piece.owner);
            return true;
        }

        public void RemovePieceFromAnywhere(ChessPiece piece)
        {
            if (piece.isOnBench && piece.benchIndex >= 0)
            {
                var bSlot = GetBenchSlot(piece.benchIndex);
                if (bSlot != null && bSlot.piece == piece) bSlot.piece = null;
            }
            else if (piece.boardPosition.x >= 0)
            {
                var slot = GetSlot(piece.boardPosition.x, piece.boardPosition.y);
                if (slot != null && slot.piece == piece) slot.piece = null;
            }
        }

        public void SwapPieces(ChessPiece a, ChessPiece b)
        {
            var posA = a.boardPosition;
            var posB = b.boardPosition;
            bool aOnBench = a.isOnBench;
            bool bOnBench = b.isOnBench;
            int benchA = a.benchIndex;
            int benchB = b.benchIndex;

            RemovePieceFromAnywhere(a);
            RemovePieceFromAnywhere(b);

            if (bOnBench)
                PlacePieceOnBenchForce(a, benchB);
            else
                PlacePiece(a, posB.x, posB.y);

            if (aOnBench)
                PlacePieceOnBenchForce(b, benchA);
            else
                PlacePiece(b, posA.x, posA.y);
        }

        private bool PlacePieceOnBenchForce(ChessPiece piece, int benchIndex)
        {
            var slot = GetBenchSlot(benchIndex);
            if (slot == null) return false;

            RemovePieceFromAnywhere(piece);

            if (piece.owner != null)
            {
                piece.owner.boardPieces.Remove(piece);
                if (!piece.owner.benchPieces.Contains(piece))
                    piece.owner.benchPieces.Add(piece);
            }

            slot.piece = piece;
            piece.boardPosition = new Vector2Int(-1, benchIndex);
            piece.isOnBench = true;
            piece.benchIndex = benchIndex;
            piece.transform.position = slot.worldPos;
            piece.gameObject.SetActive(true);
            piece.SetHealthBarVisible(false);
            RefreshFactionBonuses(piece.owner);
            return true;
        }

        public BoardSlot GetSlotAtWorldPosition(Vector3 worldPos)
        {
            BoardSlot nearest = null;
            float nearestDist = float.MaxValue;
            float maxSnap = gameConfig.cellSize * 0.6f;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var slot = grid[r, c];
                    float dx = worldPos.x - slot.worldPos.x;
                    float dz = worldPos.z - slot.worldPos.z;
                    float dist = dx * dx + dz * dz;
                    if (dist < nearestDist)
                    {
                        nearestDist = dist;
                        nearest = slot;
                    }
                }
            }

            if (nearest != null && Mathf.Sqrt(nearestDist) < maxSnap)
                return nearest;
            return null;
        }

        public BoardSlot GetBenchSlotAtWorldPosition(Vector3 worldPos)
        {
            float cellSize = gameConfig.cellSize;
            float maxSnap = cellSize * 0.6f;

            BoardSlot nearest = null;
            float nearestDist = float.MaxValue;

            foreach (var slot in benchSlots)
            {
                float dx = worldPos.x - slot.worldPos.x;
                float dz = worldPos.z - slot.worldPos.z;
                float dist = dx * dx + dz * dz;
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = slot;
                }
            }

            if (nearest != null && Mathf.Sqrt(nearestDist) < maxSnap)
                return nearest;
            return null;
        }

        public ChessPiece SpawnPiece(HeroData heroData, PlayerData owner, int starLevel = 1)
        {
            var go = Instantiate(piecePrefab);
            go.SetActive(true);
            go.name = $"Piece_{heroData.heroName}_{owner.playerName}";
            var piece = go.GetComponent<ChessPiece>();
            piece.Initialize(heroData, owner, starLevel);
            piece.boardPosition = new Vector2Int(-1, -1);
            piece.isOnBench = false;
            piece.benchIndex = -1;
            allSpawnedPieces.Add(piece);
            Debug.Log($"[SpawnPiece] {heroData.heroName} for {owner.playerName}, active={go.activeSelf}, renderer={go.GetComponentInChildren<Renderer>()?.enabled}");
            return piece;
        }

        public List<ChessPiece> GetAllPiecesOnBoard()
        {
            var result = new List<ChessPiece>();
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    if (grid[r, c].piece != null)
                        result.Add(grid[r, c].piece);
            return result;
        }

        public List<ChessPiece> GetPiecesOnBench()
        {
            var result = new List<ChessPiece>();
            foreach (var slot in benchSlots)
                if (slot.piece != null)
                    result.Add(slot.piece);
            return result;
        }

        public List<ChessPiece> GetPiecesByOwner(PlayerData owner, bool aliveOnly = true)
        {
            var result = new List<ChessPiece>();
            foreach (var piece in allSpawnedPieces)
            {
                if (piece == null) continue;
                if (piece.owner != owner) continue;
                if (aliveOnly && !piece.IsAlive) continue;
                result.Add(piece);
            }
            return result;
        }

        public void RemoveFromTracking(ChessPiece piece)
        {
            allSpawnedPieces.Remove(piece);
        }

        public void CleanupNullPieces()
        {
            allSpawnedPieces.RemoveAll(p => p == null || p.gameObject == null);
        }

        public void ClearAllTracking()
        {
            allSpawnedPieces.Clear();
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    if (grid[r, c] != null) grid[r, c].piece = null;
            foreach (var slot in benchSlots)
                slot.piece = null;
        }

        public void ClearBoardForCombat()
        {
        }

        public void RefreshFactionBonuses(PlayerData owner)
        {
            if (owner == null || FactionManager.Instance == null) return;
            var boardPieces = new List<ChessPiece>();
            foreach (var piece in owner.boardPieces)
            {
                if (piece != null && piece.IsAlive)
                    boardPieces.Add(piece);
            }
            FactionManager.Instance.ClearFactionsFromPieces(boardPieces);
            FactionManager.Instance.ApplyFactionsToPieces(boardPieces);
        }

        // Returns hex corner positions for rendering
        public Vector3[] GetHexCorners(Vector3 center, float size)
        {
            var corners = new Vector3[6];
            for (int i = 0; i < 6; i++)
            {
                float angleDeg = 60 * i + 30; // flat-top hex
                float angleRad = angleDeg * Mathf.Deg2Rad;
                corners[i] = center + new Vector3(
                    size * 0.5f * Mathf.Cos(angleRad),
                    0,
                    size * 0.5f * Mathf.Sin(angleRad)
                );
            }
            return corners;
        }

        void OnDrawGizmosSelected()
        {
            if (gameConfig == null || boardOrigin == null) return;
            float cs = gameConfig.cellSize;

            for (int r = 0; r < rows; r++)
            {
                Gizmos.color = r < playerRows ? Color.green : Color.red;
                for (int c = 0; c < cols; c++)
                {
                    Vector3 center = HexWorldPos(r, c);
                    var corners = GetHexCorners(center, cs);
                    for (int i = 0; i < 6; i++)
                        Gizmos.DrawLine(corners[i], corners[(i + 1) % 6]);
                }
            }

            if (benchOrigin != null)
            {
                Gizmos.color = Color.yellow;
                for (int i = 0; i < gameConfig.benchSlots; i++)
                {
                    Vector3 pos = benchOrigin.position + new Vector3(i * cs, 0, 0);
                    Gizmos.DrawWireCube(pos, new Vector3(cs * 0.9f, 0.1f, cs * 0.9f));
                }
            }
        }
    }
}
