using UnityEngine;
using System.Collections.Generic;
using AutoChess.Data;

namespace AutoChess.Core
{
    public partial class ChessPiece : MonoBehaviour
    {
        [Header("Data")]
        public HeroData heroData;
        public PlayerData owner;

        [Header("Runtime Stats")]
        public int starLevel = 1;
        public int currentHealth;
        public int maxHealth;
        public int attackDamage;
        public float attackSpeed;
        public int armor;
        public int magicResist;
        public float attackRange;
        public int currentMana;
        public int maxMana;

        [System.NonSerialized] public int bonusHealth = 0;
        [System.NonSerialized] public int bonusAttack = 0;
        [System.NonSerialized] public float bonusAttackSpeed = 0f;
        [System.NonSerialized] public int bonusArmor = 0;
        [System.NonSerialized] public int bonusMagicResist = 0;

        [Header("State")]
        public PieceState state = PieceState.Idle;
        public ChessPiece currentTarget;
        public Vector2Int boardPosition;
        public bool isOnBench = false;
        public int benchIndex = -1;

        private Material mat;

        public bool IsAlive => currentHealth > 0 && state != PieceState.Dead;

        public void Initialize(HeroData data, PlayerData ownerPlayer, int star = 1)
        {
            heroData = data;
            owner = ownerPlayer;
            starLevel = star;
            ApplyStarMultiplier();

            if (data.modelPrefab != null)
            {
                var body = transform.Find("Body");
                if (body != null) DestroyImmediate(body.gameObject);

                var model = Instantiate(data.modelPrefab, transform);
                model.name = "Body";
                model.transform.localPosition = new Vector3(0, data.modelYOffset, 0);
                model.transform.localScale = Vector3.one * data.modelScale;

                if (model.GetComponentInChildren<Collider>() == null)
                {
                    var capsule = gameObject.AddComponent<CapsuleCollider>();
                    capsule.center = new Vector3(0, 0.5f + data.modelYOffset, 0);
                    capsule.radius = 0.4f;
                    capsule.height = 1.2f;
                }

                SetLayerRecursive(model, gameObject.layer);
            }

            if (data.modelPrefab == null)
            {
                var renderer = GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    mat = new Material(renderer.material);
                    mat.color = data.displayColor;
                    renderer.material = mat;
                }
            }

            gameObject.name = $"{data.heroName}_⭐{star}";
            CreateHealthBar();
        }

        void ApplyStarMultiplier()
        {
            float mult = starLevel == 2 ? 1.8f : (starLevel == 3 ? 3.6f : 1f);
            maxHealth = Mathf.RoundToInt(heroData.maxHealth * mult);
            currentHealth = maxHealth;
            attackDamage = Mathf.RoundToInt(heroData.attackDamage * mult);
            attackSpeed = heroData.attackSpeed;
            armor = heroData.armor;
            magicResist = heroData.magicResist;
            attackRange = heroData.attackRange * 1.2f;
            maxMana = heroData.maxMana;
            currentMana = heroData.startingMana;
            RebuildHealthDividers();
        }

        public void ApplyFactionBonuses(int healthBonus, int attackBonus, float attackSpeedBonus, int armorBonus = 0, int magicResistBonus = 0)
        {
            bonusHealth = healthBonus;
            bonusAttack = attackBonus;
            bonusAttackSpeed = attackSpeedBonus;
            bonusArmor = armorBonus;
            bonusMagicResist = magicResistBonus;

            maxHealth += bonusHealth;
            currentHealth += bonusHealth;
            attackDamage += bonusAttack;
            attackSpeed *= (1f + bonusAttackSpeed);
            armor += bonusArmor;
            magicResist += bonusMagicResist;
        }

        public void ClearFactionBonuses()
        {
            maxHealth -= bonusHealth;
            currentHealth -= bonusHealth;
            attackDamage -= bonusAttack;
            if (bonusAttackSpeed != 0f)
                attackSpeed /= (1f + bonusAttackSpeed);
            armor -= bonusArmor;
            magicResist -= bonusMagicResist;

            bonusHealth = 0;
            bonusAttack = 0;
            bonusAttackSpeed = 0f;
            bonusArmor = 0;
            bonusMagicResist = 0;
        }

        public void ResetForPreparation(Vector3 pos)
        {
            state = PieceState.Idle;
            currentHealth = maxHealth;
            currentMana = heroData != null ? heroData.startingMana : 0;
            currentTarget = null;
            stunTimer = 0f;
            gameObject.SetActive(true);
            transform.position = pos;
            transform.rotation = Quaternion.identity;
            UpdateHealthBar();
            SetHealthBarVisible(!isOnBench);
        }

        public void SetHighlight(bool highlight)
        {
            if (mat != null)
                mat.SetFloat("_Metallic", highlight ? 0.8f : 0f);
        }

        public void SetSellHighlight(bool active)
        {
            if (mat != null)
                mat.color = active ? Color.red : (heroData != null ? heroData.displayColor : Color.white);
        }

        static void SetLayerRecursive(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform)
                SetLayerRecursive(child.gameObject, layer);
        }
    }
}
