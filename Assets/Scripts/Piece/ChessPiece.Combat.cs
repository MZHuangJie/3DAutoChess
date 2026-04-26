using UnityEngine;
using AutoChess.Data;

namespace AutoChess.Core
{
    public partial class ChessPiece
    {
        // Combat internals
        private float attackCooldown = 0f;
        private float manaPerAttack = 10f;
        private float manaPerHit = 10f;
        private float stunTimer = 0f;

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
    }
}
