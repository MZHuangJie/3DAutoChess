using UnityEngine;
using UnityEngine.EventSystems;
using AutoChess.Data;
using AutoChess.UI;

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
        private readonly float sellZoneWidth = 0.20f;
        private readonly float sellZoneHeight = 0.30f;

        // Move-to-drag state
        private ChessPiece pendingPiece;
        private Vector3 pendingOriginalPos;
        private Vector3 pendingDragOffset;
        private Vector3 mouseDownScreenPos;
        private const float DragMoveThreshold = 5f;

        // Equipment drag state
        private EquipmentData draggedEquipment;
        private int draggedEquipmentIndex = -1;
        private bool isDraggingEquipment = false;

        // Pending equipment (click vs drag)
        private int pendingEquipmentIndex = -1;
        private Vector3 pendingEquipmentMousePos;

        public bool IsDraggingEquipment => isDraggingEquipment;
        public EquipmentData DraggedEquipment => draggedEquipment;

        public void SetPendingEquipment(int inventoryIndex)
        {
            pendingEquipmentIndex = inventoryIndex;
            pendingEquipmentMousePos = Input.mousePosition;
        }

        void StartEquipmentDragFromPending()
        {
            var human = GameLoopManager.Instance?.HumanPlayer;
            if (human == null || pendingEquipmentIndex < 0 || pendingEquipmentIndex >= human.equipmentInventory.Count)
            {
                pendingEquipmentIndex = -1;
                return;
            }
            draggedEquipment = human.equipmentInventory[pendingEquipmentIndex];
            draggedEquipmentIndex = pendingEquipmentIndex;
            isDraggingEquipment = true;
            pendingEquipmentIndex = -1;
        }

        void Update()
        {
            bool isPreparation = GameLoopManager.Instance == null ||
                                 GameLoopManager.Instance.CurrentPhase == GamePhase.Preparation;

            if (Input.GetMouseButtonDown(0) && !isDragging && !isDraggingEquipment && pendingEquipmentIndex < 0)
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    return;
                mouseDownScreenPos = Input.mousePosition;
                TryPendingPiece();
                return;
            }

            // Equipment pending: move = drag, no move = show detail
            if (Input.GetMouseButton(0) && pendingEquipmentIndex >= 0 && !isDraggingEquipment)
            {
                float dist = Vector3.Distance(Input.mousePosition, pendingEquipmentMousePos);
                if (dist >= DragMoveThreshold)
                {
                    StartEquipmentDragFromPending();
                }
                return;
            }
            if (Input.GetMouseButtonUp(0) && pendingEquipmentIndex >= 0 && !isDraggingEquipment)
            {
                var human = GameLoopManager.Instance?.HumanPlayer;
                if (human != null && pendingEquipmentIndex < human.equipmentInventory.Count)
                {
                    var uiMgr = Object.FindFirstObjectByType<UIManager>();
                    uiMgr?.ShowEquipmentDetail(human.equipmentInventory[pendingEquipmentIndex]);
                }
                pendingEquipmentIndex = -1;
                return;
            }
            if (Input.GetMouseButtonUp(0) && isDraggingEquipment)
            {
                EndEquipmentDrag();
                return;
            }

            bool canDrag = pendingPiece != null && (isPreparation || pendingPiece.isOnBench);
            if (Input.GetMouseButton(0) && pendingPiece != null && !isDragging && canDrag)
            {
                float dist = Vector3.Distance(Input.mousePosition, mouseDownScreenPos);
                if (dist >= DragMoveThreshold)
                {
                    StartDragFromPending();
                }
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
            else if (Input.GetMouseButtonUp(0) && pendingPiece != null)
            {
                var uiMgr = Object.FindFirstObjectByType<UIManager>();
                uiMgr?.ShowPieceDetail(pendingPiece);
                pendingPiece = null;
            }

            if (Input.GetMouseButtonDown(1) && isPreparation)
            {
                TrySellUnderCursor();
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

        bool IsInSellZone()
        {
            float screenX = Input.mousePosition.x / Screen.width;
            float screenY = Input.mousePosition.y / Screen.height;
            return screenX < sellZoneWidth && screenY < sellZoneHeight;
        }

        void CheckSellZone()
        {
            if (draggedPiece == null) return;
            isSellMode = IsInSellZone();
            draggedPiece.SetHighlight(!isSellMode);
            draggedPiece.SetSellHighlight(isSellMode);
        }

        void TryPendingPiece()
        {
            Ray ray = gameCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, pieceLayer))
            {
                var piece = hit.collider.GetComponentInParent<ChessPiece>();
                if (piece != null && piece.owner.isHuman && piece.IsAlive)
                {
                    pendingPiece = piece;
                    pendingOriginalPos = piece.transform.position;
                    pendingDragOffset = piece.transform.position - hit.point;
                    pendingDragOffset.y = 0;
                }
            }
            else
            {
                // Clicked empty area — hide detail panel
                var uiMgr = Object.FindFirstObjectByType<UIManager>();
                uiMgr?.HidePieceDetail();
            }
        }

        void StartDragFromPending()
        {
            draggedPiece = pendingPiece;
            originalPosition = pendingOriginalPos;
            dragOffset = pendingDragOffset;
            isDragging = true;
            draggedPiece.SetHighlight(true);
            pendingPiece = null;
        }

        void UpdateDrag()
        {
            if (draggedPiece == null) return;

            Ray ray = gameCamera.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
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
            if (IsInSellZone())
            {
                ShopManager.Instance?.SellPiece(draggedPiece, true);
                draggedPiece = null;
                isDragging = false;
                return;
            }

            // First check board slots
            var boardSlot = boardManager.GetSlotAtWorldPosition(dropPos);
            if (boardSlot != null && boardSlot.isPlayerSide)
            {
                if (!boardSlot.IsOccupied)
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
                else if (boardSlot.piece != null && boardSlot.piece != draggedPiece)
                {
                    boardManager.SwapPieces(draggedPiece, boardSlot.piece);
                }
                else
                {
                    draggedPiece.transform.position = originalPosition;
                }
            }
            else
            {
                var benchSlot = boardManager.GetBenchSlotAtWorldPosition(dropPos);
                if (benchSlot != null && !benchSlot.IsOccupied)
                {
                    boardManager.PlacePieceOnBench(draggedPiece, benchSlot.gridPos.y);
                }
                else if (benchSlot != null && benchSlot.piece != null && benchSlot.piece != draggedPiece)
                {
                    boardManager.SwapPieces(draggedPiece, benchSlot.piece);
                }
                else
                {
                    draggedPiece.transform.position = originalPosition;
                }
            }

            var uiMgr = Object.FindFirstObjectByType<UIManager>();
            uiMgr?.UpdateUI();

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

            var uiMgr = Object.FindFirstObjectByType<UIManager>();
            uiMgr?.UpdateUI();

            isDraggingEquipment = false;
            draggedEquipment = null;
            draggedEquipmentIndex = -1;
        }
    }
}
