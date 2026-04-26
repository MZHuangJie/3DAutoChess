using UnityEngine;
using System.Collections.Generic;
using AutoChess.Data;

namespace AutoChess.Core
{
    public partial class ChessPiece
    {
        // Equipment
        public List<EquipmentData> equipment = new List<EquipmentData>();
        public const int MaxEquipmentSlots = 3;
        private List<GameObject> equipmentVisuals = new List<GameObject>();
        private const float EquipmentY = 1.8f;

        public bool CanEquip => equipment.Count < MaxEquipmentSlots;

        public void Equip(EquipmentData item)
        {
            if (!CanEquip || item == null) return;
            equipment.Add(item);
            ApplyEquipmentStats(item);
            SpawnEquipmentVisual(item);
        }

        public void RemoveEquipment(EquipmentData item)
        {
            if (!equipment.Contains(item)) return;
            equipment.Remove(item);
            RemoveEquipmentStats(item);
            RefreshEquipmentVisuals();
        }

        public List<EquipmentData> UnequipAll()
        {
            var items = new List<EquipmentData>(equipment);
            foreach (var item in items)
                RemoveEquipmentStats(item);
            equipment.Clear();
            ClearEquipmentVisuals();
            return items;
        }

        void ApplyEquipmentStats(EquipmentData item)
        {
            maxHealth += item.healthBonus;
            currentHealth += item.healthBonus;
            attackDamage += item.attackBonus;
            armor += item.armorBonus;
            magicResist += item.magicResistBonus;
            if (item.attackSpeedBonus != 0f) attackSpeed *= (1f + item.attackSpeedBonus);
            maxMana += item.manaBonus;
        }

        void RemoveEquipmentStats(EquipmentData item)
        {
            maxHealth -= item.healthBonus;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            attackDamage -= item.attackBonus;
            armor -= item.armorBonus;
            magicResist -= item.magicResistBonus;
            if (item.attackSpeedBonus != 0f) attackSpeed /= (1f + item.attackSpeedBonus);
            maxMana -= item.manaBonus;
        }

        void SpawnEquipmentVisual(EquipmentData item)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(transform);
            cube.transform.localScale = Vector3.one * 0.15f;
            float xOff = (equipmentVisuals.Count - 1) * 0.2f;
            cube.transform.localPosition = new Vector3(xOff, EquipmentY, 0);
            var col = cube.GetComponent<Collider>();
            if (col != null) Destroy(col);
            var r = cube.GetComponent<Renderer>();
            if (r != null) r.material.color = item.displayColor;
            equipmentVisuals.Add(cube);
        }

        void ClearEquipmentVisuals()
        {
            foreach (var v in equipmentVisuals)
                if (v != null) Destroy(v);
            equipmentVisuals.Clear();
        }

        void RefreshEquipmentVisuals()
        {
            ClearEquipmentVisuals();
            foreach (var item in equipment)
                SpawnEquipmentVisual(item);
        }
    }
}
