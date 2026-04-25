using UnityEngine;
using AutoChess.Data;

namespace AutoChess.Core
{
    public class DragController : MonoBehaviour
    {
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private Camera gameCamera;
        [SerializeField] private LayerMask pieceLayer;
        [SerializeField] private LayerMask groundLayer;

        private ChessPiece draggedPiece;
        private Vector3 dragOffset;
        private Vector3 originalPosition;
        private bool isDragging = false;
        private bool isSellMode = false;
        private readonly float sellZoneScreenHeight = 0.15f;

        // Equipment drag state
        private EquipmentData draggedEquipment;
        private int draggedEquipmentIndex = -1;
        private bool isDraggingEquipment = false;

        public bool IsDraggingEquipment => isDraggingEquipment;
        public EquipmentData DraggedEquipment => draggedEquipment;

        public void StartEquipmentDrag(int inventoryIndex)
        {
            var human = GameLoopManager.Instance?.HumanPlayer;
            if (human == null || inventoryIndex < 0 || inventoryIndex >= human.equipmentInventory.Count) return;

            draggedEquipment = human.equipmentInventory[inventoryIndex];
            draggedEquipmentIndex = inventoryIndex;
            isDraggingEquipment = true;
        }

        void Update()
        {
            if (GameLoopManager.Instance != null && 
                GameLoopManager.Instance.CurrentPhase != GamePhase.Preparation)
                return;

            // Right-click to sell piece under cursor
            if (Input.GetMouseButtonDown(1))
            {
                TrySellUnderCursor();
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                TryStartDrag();
            }
            else if (Input.GetMouseButton(0) && isDragging)
            {
                UpdateDrag();
                CheckSellZone();
            }
            else if (Input.GetMouseButtonUp(0) && isDragging)
            {
                EndDrag();
            }
            else if (Input.GetMouseButtonUp(0) && isDraggingEquipment)
            {
                EndEquipmentDrag();
            }
        }

        void TrySellUnderCursor()
        {
            Ray ray = gameCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, pieceLayer))
            {
                var piece = hit.collider.GetComponentInParent<ChessPiece>();
                if (piece != null && piece.owner.isHuman && piece.IsAlive)
                {
                    ShopManager.Instance?.SellPiece(piece);
                }
            }
        }

        void CheckSellZone()
        {
            if (draggedPiece == null) return;
            float screenY = Input.mousePosition.y / Screen.height;
            isSellMode = screenY < sellZoneScreenHeight;
            draggedPiece.SetHighlight(isSellMode ? false : true);
            if (isSellMode)
            {
                // Visual feedback for sell
                draggedPiece.SetSellHighlight(true);
            }
            else
            {
                draggedPiece.SetSellHighlight(false);
            }
        }

        void TryStartDrag()
        {
            Ray ray = gameCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, pieceLayer))
            {
                var piece = hit.collider.GetComponentInParent<ChessPiece>();
                if (piece != null && piece.owner.isHuman && piece.IsAlive)
                {
                    draggedPiece = piece;
                    originalPosition = draggedPiece.transform.position;
                    dragOffset = draggedPiece.transform.position - hit.point;
                    dragOffset.y = 0;
                    isDragging = true;
                    draggedPiece.SetHighlight(true);
                }
            }
        }

        void UpdateDrag()
        {
            if (draggedPiece == null) return;

            Ray ray = gameCamera.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, originalPosition);
            if (groundPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                draggedPiece.transform.position = hitPoint + dragOffset + Vector3.up * 0.5f;
            }
        }

        void EndDrag()
        {
            if (draggedPiece == null) return;
            draggedPiece.SetHighlight(false);
            draggedPiece.SetSellHighlight(false);

            Vector3 dropPos = draggedPiece.transform.position;
            dropPos.y = 0;

            var player = draggedPiece.owner;
            bool wasOnBench = draggedPiece.isOnBench;

            // Check sell zone first
            float screenY = Input.mousePosition.y / Screen.height;
            if (screenY < sellZoneScreenHeight)
            {
                ShopManager.Instance?.SellPiece(draggedPiece);
                draggedPiece = null;
                isDragging = false;
                return;
            }

            // First check board slots
            var boardSlot = boardManager.GetSlotAtWorldPosition(dropPos);
            if (boardSlot != null && boardSlot.isPlayerSide && !boardSlot.IsOccupied)
            {
                if (wasOnBench && !ShopManager.Instance.CanPlaceOnBoard(player))
                {
                    Debug.Log("Board full! Upgrade population to place more units.");
                    draggedPiece.transform.position = originalPosition;
                }
                else
                {
                    boardManager.PlacePiece(draggedPiece, boardSlot.gridPos.x, boardSlot.gridPos.y);
                }
            }
            else
            {
                var benchSlot = boardManager.GetBenchSlotAtWorldPosition(dropPos);
                if (benchSlot != null && !benchSlot.IsOccupied)
                {
                    boardManager.PlacePieceOnBench(draggedPiece, benchSlot.gridPos.y);
                }
                else
                {
                    draggedPiece.transform.position = originalPosition;
                }
            }

            draggedPiece = null;
            isDragging = false;
        }

        void EndEquipmentDrag()
        {
            if (draggedEquipment == null)
            {
                isDraggingEquipment = false;
                return;
            }

            var human = GameLoopManager.Instance?.HumanPlayer;
            if (human == null)
            {
                isDraggingEquipment = false;
                draggedEquipment = null;
                return;
            }

            Ray ray = gameCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, pieceLayer))
            {
                var piece = hit.collider.GetComponentInParent<ChessPiece>();
                if (piece != null && piece.owner == human && piece.IsAlive)
                {
                    if (EquipmentManager.Instance != null)
                    {
                        EquipmentManager.Instance.EquipItem(piece, draggedEquipment, human);
                        Debug.Log($"[Equipment] Equipped {draggedEquipment.equipmentName} on {piece.heroData.heroName}");
                    }
                }
            }

            isDraggingEquipment = false;
            draggedEquipment = null;
            draggedEquipmentIndex = -1;
        }
    }
}
