using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using AutoChess.Data;

namespace AutoChess.Core
{
    public enum PieceState { Idle, Moving, Attacking, Casting, Dead }

    public class ChessPiece : MonoBehaviour
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

        [Header("State")]
        public PieceState state = PieceState.Idle;
        public ChessPiece currentTarget;
        public Vector2Int boardPosition;
        public bool isOnBench = false;
        public int benchIndex = -1;

        // Equipment
        public List<EquipmentData> equipment = new List<EquipmentData>();
        public const int MaxEquipmentSlots = 3;
        private List<GameObject> equipmentVisuals = new List<GameObject>();

        // Health bar
        private Canvas healthBarCanvas;
        private Image healthFillImage;
        private GameObject healthBarRoot;
        private const float HealthBarY = 2.1f;
        private const float EquipmentY = 1.8f;

        // Internal
        private float attackCooldown = 0f;
        private float manaPerAttack = 10f;
        private float manaPerHit = 10f;
        private float stunTimer = 0f;
        private Material mat;

        public bool IsAlive => currentHealth > 0 && state != PieceState.Dead;
        public bool CanEquip => equipment.Count < MaxEquipmentSlots;

        public void Initialize(HeroData data, PlayerData ownerPlayer, int star = 1)
        {
            heroData = data;
            owner = ownerPlayer;
            starLevel = star;
            ApplyStarMultiplier();

            var renderer = GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                mat = new Material(renderer.material);
                mat.color = data.displayColor;
                renderer.material = mat;
            }

            gameObject.name = $"{data.heroName}_⭐{star}";
            CreateHealthBar();
        }

        private GameObject healthDividerContainer;
        private const float HealthPerSegment = 100f;

        void CreateHealthBar()
        {
            if (healthBarRoot != null)
            {
                Destroy(healthBarRoot);
                healthBarRoot = null;
                healthBarCanvas = null;
                healthFillImage = null;
                healthDividerContainer = null;
            }

            healthBarRoot = new GameObject("HealthBar");
            healthBarRoot.transform.SetParent(transform);
            healthBarRoot.transform.localPosition = new Vector3(0, HealthBarY, 0);

            healthBarCanvas = healthBarRoot.AddComponent<Canvas>();
            healthBarCanvas.renderMode = RenderMode.WorldSpace;
            healthBarCanvas.sortingOrder = 100;

            var rt = healthBarRoot.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(104f, 14f);
            rt.localScale = Vector3.one * 0.01f;

            var borderObj = new GameObject("Border");
            borderObj.transform.SetParent(healthBarRoot.transform, false);
            var borderImage = borderObj.AddComponent<Image>();
            borderImage.color = Color.black;
            var borderRt = borderObj.GetComponent<RectTransform>();
            borderRt.anchorMin = Vector2.zero;
            borderRt.anchorMax = Vector2.one;
            borderRt.offsetMin = Vector2.zero;
            borderRt.offsetMax = Vector2.zero;

            var bgObj = new GameObject("BG");
            bgObj.transform.SetParent(healthBarRoot.transform, false);
            var bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            var bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = new Vector2(2f, 2f);
            bgRt.offsetMax = new Vector2(-2f, -2f);

            var fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(healthBarRoot.transform, false);
            healthFillImage = fillObj.AddComponent<Image>();
            healthFillImage.color = new Color(0.9f, 0.2f, 0.2f, 1f);
            var fillRt = fillObj.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.pivot = new Vector2(0, 0.5f);
            fillRt.offsetMin = new Vector2(2f, 2f);
            fillRt.offsetMax = new Vector2(-2f, -2f);

            healthDividerContainer = new GameObject("Dividers");
            healthDividerContainer.transform.SetParent(healthBarRoot.transform, false);
            var divRt = healthDividerContainer.AddComponent<RectTransform>();
            divRt.anchorMin = Vector2.zero;
            divRt.anchorMax = Vector2.one;
            divRt.offsetMin = new Vector2(2f, 2f);
            divRt.offsetMax = new Vector2(-2f, -2f);

            healthBarRoot.SetActive(!isOnBench);
        }

        void RebuildHealthDividers()
        {
            if (healthDividerContainer == null) return;
            for (int i = healthDividerContainer.transform.childCount - 1; i >= 0; i--)
                Destroy(healthDividerContainer.transform.GetChild(i).gameObject);

            int segments = Mathf.Max(1, Mathf.CeilToInt(maxHealth / HealthPerSegment));
            if (segments <= 1) return;

            for (int i = 1; i < segments; i++)
            {
                float xNorm = (float)i / segments;
                var line = new GameObject($"Div_{i}");
                line.transform.SetParent(healthDividerContainer.transform, false);
                var lineRt = line.AddComponent<RectTransform>();
                lineRt.anchorMin = new Vector2(xNorm, 0);
                lineRt.anchorMax = new Vector2(xNorm, 1);
                lineRt.pivot = new Vector2(0.5f, 0.5f);
                lineRt.sizeDelta = new Vector2(1.5f, 0);
                var lineImg = line.AddComponent<Image>();
                lineImg.color = new Color(0, 0, 0, 0.6f);
                lineImg.raycastTarget = false;
            }
        }

        public void UpdateHealthBar()
        {
            if (healthFillImage == null) return;
            float ratio = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
            var rt = healthFillImage.GetComponent<RectTransform>();
            rt.anchorMax = new Vector2(ratio, 1f);
        }

        public void SetHealthBarVisible(bool visible)
        {
            if (healthBarRoot != null)
                healthBarRoot.SetActive(visible);
        }

        void LateUpdate()
        {
            if (healthBarRoot != null && healthBarRoot.activeSelf)
            {
                healthBarRoot.transform.position = transform.position + new Vector3(0, HealthBarY, 0);
                healthBarRoot.transform.localScale = Vector3.one * 0.01f;
                var cam = Camera.main;
                if (cam != null)
                    healthBarRoot.transform.rotation = cam.transform.rotation;
            }
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
        public void ApplyFactionBonuses(int healthBonus, int attackBonus, float attackSpeedBonus)
        {
            bonusHealth = healthBonus;
            bonusAttack = attackBonus;
            bonusAttackSpeed = attackSpeedBonus;

            maxHealth += bonusHealth;
            currentHealth += bonusHealth;
            attackDamage += bonusAttack;
            attackSpeed *= (1f + bonusAttackSpeed);
        }

        public void ClearFactionBonuses()
        {
            maxHealth -= bonusHealth;
            currentHealth -= bonusHealth;
            attackDamage -= bonusAttack;
            if (bonusAttackSpeed != 0f)
                attackSpeed /= (1f + bonusAttackSpeed);

            bonusHealth = 0;
            bonusAttack = 0;
            bonusAttackSpeed = 0f;
        }

        // --- Equipment ---

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

        // --- Combat ---

        public void OnCombatStart()
        {
            state = PieceState.Idle;
            currentTarget = null;
            attackCooldown = 0f;
            stunTimer = 0f;
            SetHealthBarVisible(true);
            RebuildHealthDividers();
            UpdateHealthBar();
        }

        public void TickCombat(float deltaTime, GameConfig config)
        {
            if (!IsAlive) return;

            if (stunTimer > 0f)
            {
                stunTimer -= deltaTime;
                return;
            }

            switch (state)
            {
                case PieceState.Idle:
                    if (currentMana >= maxMana && heroData != null && heroData.skillType != SkillType.None)
                    {
                        state = PieceState.Casting;
                        break;
                    }
                    FindTarget();
                    if (currentTarget != null && currentTarget.IsAlive)
                    {
                        float dist = Vector3.Distance(transform.position, currentTarget.transform.position);
                        if (dist <= attackRange)
                        {
                            state = PieceState.Attacking;
                            attackCooldown = 0f;
                        }
                        else
                        {
                            state = PieceState.Moving;
                        }
                    }
                    break;

                case PieceState.Moving:
                    if (currentTarget == null || !currentTarget.IsAlive)
                    {
                        state = PieceState.Idle;
                        break;
                    }
                    if (currentMana >= maxMana && heroData != null && heroData.skillType != SkillType.None)
                    {
                        state = PieceState.Casting;
                        break;
                    }
                    MoveTowardTarget(deltaTime, config.moveSpeed);
                    float d = Vector3.Distance(transform.position, currentTarget.transform.position);
                    if (d <= attackRange)
                    {
                        state = PieceState.Attacking;
                        attackCooldown = 0f;
                    }
                    break;

                case PieceState.Attacking:
                    if (currentTarget == null || !currentTarget.IsAlive)
                    {
                        state = PieceState.Idle;
                        currentTarget = null;
                        break;
                    }
                    if (currentMana >= maxMana && heroData != null && heroData.skillType != SkillType.None)
                    {
                        state = PieceState.Casting;
                        break;
                    }
                    transform.LookAt(currentTarget.transform);
                    attackCooldown -= deltaTime;
                    if (attackCooldown <= 0f)
                    {
                        PerformAttack();
                        attackCooldown = 1f / attackSpeed;
                    }
                    float rd = Vector3.Distance(transform.position, currentTarget.transform.position);
                    if (rd > attackRange * 1.2f)
                    {
                        state = PieceState.Moving;
                    }
                    break;

                case PieceState.Casting:
                    CastSkill();
                    currentMana = 0;
                    state = PieceState.Idle;
                    break;
            }
        }

        void CastSkill()
        {
            float skillMult = starLevel == 2 ? 1.5f : (starLevel == 3 ? 2f : 1f);
            int spellBonus = GetSpellDamageBonus();
            int baseDmg = Mathf.RoundToInt((heroData.skillDamage + spellBonus) * skillMult);

            switch (heroData.skillType)
            {
                case SkillType.Damage:
                    if (currentTarget != null && currentTarget.IsAlive)
                    {
                        if (heroData.skillIsMagic)
                            currentTarget.TakeMagicDamage(baseDmg);
                        else
                            currentTarget.TakeDamage(baseDmg);
                    }
                    break;

                case SkillType.AreaDamage:
                    var enemies = CombatManager.Instance?.GetEnemiesInRange(this, heroData.skillRange);
                    if (enemies != null)
                    {
                        foreach (var e in enemies)
                        {
                            if (heroData.skillIsMagic)
                                e.TakeMagicDamage(baseDmg);
                            else
                                e.TakeDamage(baseDmg);
                        }
                    }
                    break;

                case SkillType.Heal:
                    ChessPiece healTarget = heroData.skillTargetType == SkillTargetType.Self
                        ? this
                        : CombatManager.Instance?.GetLowestHpAlly(this);
                    if (healTarget != null && healTarget.IsAlive)
                    {
                        healTarget.currentHealth = Mathf.Min(healTarget.maxHealth, healTarget.currentHealth + baseDmg);
                        healTarget.UpdateHealthBar();
                    }
                    break;

                case SkillType.Stun:
                    if (currentTarget != null && currentTarget.IsAlive)
                    {
                        if (heroData.skillIsMagic)
                            currentTarget.TakeMagicDamage(baseDmg);
                        else
                            currentTarget.TakeDamage(baseDmg);
                        currentTarget.ApplyStun(heroData.skillStunDuration);
                    }
                    break;
            }
        }

        int GetSpellDamageBonus()
        {
            int bonus = 0;
            foreach (var eq in equipment)
                if (eq != null) bonus += eq.spellDamageBonus;
            return bonus;
        }

        float GetLifestealPercent()
        {
            float total = 0f;
            foreach (var eq in equipment)
                if (eq != null) total += eq.lifestealPercent;
            return total;
        }

        public void ApplyStun(float duration)
        {
            if (duration > stunTimer)
                stunTimer = duration;
        }

        void FindTarget()
        {
            if (CombatManager.Instance == null) return;
            currentTarget = CombatManager.Instance.FindNearestEnemy(this);
        }

        void MoveTowardTarget(float deltaTime, float speed)
        {
            if (currentTarget == null) return;
            Vector3 dir = (currentTarget.transform.position - transform.position).normalized;
            dir.y = 0;
            transform.position += dir * speed * deltaTime;
        }

        void PerformAttack()
        {
            if (currentTarget == null || !currentTarget.IsAlive) return;

            int damage = Mathf.Max(1, attackDamage - currentTarget.armor / 2);
            currentTarget.TakeDamage(damage);

            // Lifesteal from equipment
            float lifesteal = GetLifestealPercent();
            if (lifesteal > 0f)
            {
                int heal = Mathf.RoundToInt(damage * lifesteal);
                currentHealth = Mathf.Min(maxHealth, currentHealth + heal);
                UpdateHealthBar();
            }

            currentMana = Mathf.Min(maxMana, currentMana + Mathf.RoundToInt(manaPerAttack));
        }

        public void TakeDamage(int damage)
        {
            if (!IsAlive) return;
            currentHealth -= damage;
            currentMana = Mathf.Min(maxMana, currentMana + Mathf.RoundToInt(manaPerHit));
            UpdateHealthBar();

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
            }
        }

        public void TakeMagicDamage(int damage)
        {
            if (!IsAlive) return;
            int reduced = Mathf.Max(1, damage - magicResist / 2);
            currentHealth -= reduced;
            currentMana = Mathf.Min(maxMana, currentMana + Mathf.RoundToInt(manaPerHit));
            UpdateHealthBar();

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
            }
        }

        void Die()
        {
            state = PieceState.Dead;
            gameObject.SetActive(false);
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
    }
}