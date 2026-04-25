using System.Collections.Generic;
using UnityEngine;
using AutoChess.Data;

namespace AutoChess.Core
{
    public class EquipmentManager : MonoBehaviour
    {
        public static EquipmentManager Instance { get; private set; }

        [SerializeField] private List<EquipmentData> allEquipment;
        [SerializeField] private List<EquipmentData> baseEquipment;
        [SerializeField] private List<EquipmentData> combinedEquipment;

        void Awake()
        {
            Instance = this;
        }

        public void Setup(List<EquipmentData> all)
        {
            allEquipment = new List<EquipmentData>(all);
            baseEquipment = new List<EquipmentData>();
            combinedEquipment = new List<EquipmentData>();
            foreach (var eq in all)
            {
                if (eq.equipmentType == EquipmentType.Base)
                    baseEquipment.Add(eq);
                else
                    combinedEquipment.Add(eq);
            }
        }

        public bool EquipItem(ChessPiece piece, EquipmentData item, PlayerData owner)
        {
            if (piece == null || item == null || !piece.CanEquip) return false;

            piece.Equip(item);
            owner.equipmentInventory.Remove(item);

            TryCombine(piece, owner);
            return true;
        }

        void TryCombine(ChessPiece piece, PlayerData owner)
        {
            for (int i = 0; i < piece.equipment.Count; i++)
            {
                var a = piece.equipment[i];
                if (a == null || a.equipmentType != EquipmentType.Base) continue;

                for (int j = i + 1; j < piece.equipment.Count; j++)
                {
                    var b = piece.equipment[j];
                    if (b == null || b.equipmentType != EquipmentType.Base) continue;

                    var combined = FindCombineResult(a, b);
                    if (combined != null)
                    {
                        piece.RemoveEquipment(a);
                        piece.RemoveEquipment(b);
                        piece.Equip(combined);
                        Debug.Log($"[Equipment] Combined {a.equipmentName} + {b.equipmentName} = {combined.equipmentName}");
                        return;
                    }
                }
            }
        }

        EquipmentData FindCombineResult(EquipmentData a, EquipmentData b)
        {
            foreach (var combined in combinedEquipment)
            {
                if (combined.recipe1 == null || combined.recipe2 == null) continue;
                if ((combined.recipe1 == a && combined.recipe2 == b) ||
                    (combined.recipe1 == b && combined.recipe2 == a))
                    return combined;
            }
            return null;
        }

        public void TransferEquipment(ChessPiece from, ChessPiece to, PlayerData owner)
        {
            var items = from.UnequipAll();
            foreach (var item in items)
            {
                if (to.CanEquip)
                    to.Equip(item);
                else
                    owner.equipmentInventory.Add(item);
            }
        }

        public void ReturnEquipmentToOwner(ChessPiece piece)
        {
            if (piece.owner == null) return;
            var items = piece.UnequipAll();
            piece.owner.equipmentInventory.AddRange(items);
        }

        public EquipmentData GetRandomBaseEquipment()
        {
            if (baseEquipment == null || baseEquipment.Count == 0) return null;
            return baseEquipment[Random.Range(0, baseEquipment.Count)];
        }

        public EquipmentData GetRandomCombinedEquipment()
        {
            if (combinedEquipment == null || combinedEquipment.Count == 0) return null;
            return combinedEquipment[Random.Range(0, combinedEquipment.Count)];
        }
    }
}
