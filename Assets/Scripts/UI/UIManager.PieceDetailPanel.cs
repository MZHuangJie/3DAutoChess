using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AutoChess.Core;
using AutoChess.Data;

namespace AutoChess.UI
{
    public partial class UIManager
    {
        private GameObject pieceDetailPanel;
        private TextMeshProUGUI pieceDetailText;
        private GameObject pieceDetailEquipContainer;
        private GameObject factionHeroContainer;
        private ChessPiece currentDetailPiece;
        private int lastDetailEquipHash = -1;
        private GameObject equipDetailPanel;
        private TextMeshProUGUI equipDetailText;

        void CreatePieceDetailPanel()
        {
            if (pieceDetailPanel != null) return;

            pieceDetailPanel = new GameObject("PieceDetailPanel");
            pieceDetailPanel.transform.SetParent(transform, false);
            var rt = pieceDetailPanel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.zero;
            rt.pivot = new Vector2(0, 0.5f);
            rt.sizeDelta = new Vector2(320, 450);

            var bg = pieceDetailPanel.AddComponent<Image>();
            bg.color = new Color(0.06f, 0.06f, 0.1f, 0.92f);

            var textGO = new GameObject("DetailText");
            textGO.transform.SetParent(pieceDetailPanel.transform, false);
            var textRt = textGO.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(14, 10);
            textRt.offsetMax = new Vector2(-14, -10);
            pieceDetailText = textGO.AddComponent<TextMeshProUGUI>();
            pieceDetailText.fontSize = 14;
            pieceDetailText.alignment = TextAlignmentOptions.TopLeft;
            pieceDetailText.color = Color.white;
            pieceDetailText.lineSpacing = 4;

            pieceDetailEquipContainer = new GameObject("EquipIcons");
            pieceDetailEquipContainer.transform.SetParent(pieceDetailPanel.transform, false);
            var eqRt = pieceDetailEquipContainer.AddComponent<RectTransform>();
            eqRt.anchorMin = new Vector2(0, 0);
            eqRt.anchorMax = new Vector2(1, 0);
            eqRt.pivot = new Vector2(0, 0);
            eqRt.offsetMin = new Vector2(14, 10);
            eqRt.offsetMax = new Vector2(-14, 46);

            factionHeroContainer = new GameObject("FactionHeroes");
            factionHeroContainer.transform.SetParent(pieceDetailPanel.transform, false);
            var fhRt = factionHeroContainer.AddComponent<RectTransform>();
            fhRt.anchorMin = new Vector2(0, 0);
            fhRt.anchorMax = new Vector2(1, 0);
            fhRt.pivot = new Vector2(0, 0);
            fhRt.offsetMin = new Vector2(14, 10);
            fhRt.offsetMax = new Vector2(-14, 80);
            factionHeroContainer.SetActive(false);

            equipDetailPanel = new GameObject("EquipDetailPanel");
            equipDetailPanel.transform.SetParent(transform, false);
            var edRt = equipDetailPanel.AddComponent<RectTransform>();
            edRt.anchorMin = Vector2.zero;
            edRt.anchorMax = Vector2.zero;
            edRt.pivot = new Vector2(0, 0.5f);
            edRt.sizeDelta = new Vector2(220, 300);

            var edBg = equipDetailPanel.AddComponent<Image>();
            edBg.color = new Color(0.08f, 0.08f, 0.14f, 0.95f);

            var edTextGO = new GameObject("EquipDetailText");
            edTextGO.transform.SetParent(equipDetailPanel.transform, false);
            var edTextRt = edTextGO.AddComponent<RectTransform>();
            edTextRt.anchorMin = Vector2.zero;
            edTextRt.anchorMax = Vector2.one;
            edTextRt.offsetMin = new Vector2(10, 8);
            edTextRt.offsetMax = new Vector2(-10, -8);
            equipDetailText = edTextGO.AddComponent<TextMeshProUGUI>();
            equipDetailText.fontSize = 13;
            equipDetailText.alignment = TextAlignmentOptions.TopLeft;
            equipDetailText.color = Color.white;
            equipDetailText.lineSpacing = 3;
            equipDetailPanel.SetActive(false);

            pieceDetailPanel.SetActive(false);
        }

        void PositionPanelNearScreenPoint(RectTransform panelRt, Vector2 screenPoint, float gap = 10f)
        {
            var canvasRt = GetComponent<RectTransform>();
            Vector2 canvasSize = canvasRt.sizeDelta;
            float scaleX = canvasSize.x / Screen.width;
            float scaleY = canvasSize.y / Screen.height;
            Vector2 panelSize = panelRt.sizeDelta;

            float canvasX = screenPoint.x * scaleX;
            float canvasY = screenPoint.y * scaleY;

            float rightX = canvasX + gap;
            float leftX = canvasX - gap - panelSize.x;

            float posX;
            if (rightX + panelSize.x <= canvasSize.x)
                posX = rightX;
            else if (leftX >= 0)
                posX = leftX;
            else
                posX = Mathf.Clamp(canvasX - panelSize.x * 0.5f, 0, canvasSize.x - panelSize.x);

            float posY = Mathf.Clamp(canvasY, panelSize.y * 0.5f, canvasSize.y - panelSize.y * 0.5f);

            panelRt.anchoredPosition = new Vector2(posX, posY - panelSize.y * 0.5f);
            panelRt.pivot = new Vector2(0, 0);
        }

        public void ShowPieceDetail(ChessPiece piece)
        {
            if (piece == null) return;
            if (pieceDetailPanel == null) CreatePieceDetailPanel();

            currentDetailPiece = piece;
            lastDetailEquipHash = -1;
            RefreshPieceDetailContent(piece);

            pieceDetailPanel.SetActive(true);

            Vector2 screenPos = Input.mousePosition;
            PositionPanelNearScreenPoint(pieceDetailPanel.GetComponent<RectTransform>(), screenPos, 20f);
        }

        public void HidePieceDetail()
        {
            currentDetailPiece = null;
            lastDetailEquipHash = -1;
            if (pieceDetailPanel != null)
                pieceDetailPanel.SetActive(false);
            if (equipDetailPanel != null)
                equipDetailPanel.SetActive(false);
        }

        void LateUpdate()
        {
            if (currentDetailPiece != null && pieceDetailPanel != null && pieceDetailPanel.activeSelf)
            {
                if (currentDetailPiece.IsAlive)
                    RefreshPieceDetailContent(currentDetailPiece);
                else
                    HidePieceDetail();
            }
        }

        int ComputeEquipHash(ChessPiece piece)
        {
            int hash = piece.equipment.Count;
            for (int i = 0; i < piece.equipment.Count; i++)
            {
                if (piece.equipment[i] != null)
                    hash = hash * 31 + piece.equipment[i].GetInstanceID();
            }
            return hash;
        }

        void RefreshPieceDetailContent(ChessPiece piece)
        {
            factionHeroContainer.SetActive(false);

            var h = piece.heroData;
            var sb = new System.Text.StringBuilder();

            string pieceName = h != null ? h.heroName : piece.gameObject.name.Replace("Creep_", "");
            int starLevel = piece.starLevel;
            var stars = new string('★', starLevel);

            if (h != null)
            {
                int sellValue = h.cost * (int)Mathf.Pow(3, starLevel - 1);
                sb.AppendLine($"<size=20><b>{pieceName}</b></size>  {stars}    <color=#FFD700>{sellValue}金币</color>");
            }
            else
            {
                sb.AppendLine($"<size=20><b>{pieceName}</b></size>  <color=#FF6060>野怪</color>");
            }
            sb.AppendLine();

            sb.AppendLine($"血量: {piece.currentHealth}/{piece.maxHealth}");
            sb.AppendLine($"蓝量: {piece.currentMana}/{piece.maxMana}");
            sb.AppendLine();

            if (h != null && h.factions != null && h.factions.Length > 0)
            {
                sb.AppendLine($"<color=#80D0FF>羁绊: {string.Join(" / ", h.factions)}</color>");
                sb.AppendLine();
            }

            if (h != null && h.skillType != SkillType.None && !string.IsNullOrEmpty(h.skillName))
            {
                sb.AppendLine($"<color=#FFA040>技能: {h.skillName}</color>");
                string typeStr = h.skillType switch
                {
                    SkillType.Damage => "单体伤害",
                    SkillType.AreaDamage => "范围伤害",
                    SkillType.Heal => "治疗",
                    SkillType.Stun => "眩晕",
                    _ => h.skillType.ToString()
                };
                sb.Append($"类型: {typeStr}");
                if (h.skillDamage > 0) sb.Append($"  伤害: {h.skillDamage}");
                sb.AppendLine();
                sb.Append($"范围: {h.skillRange}");
                if (h.skillStunDuration > 0) sb.Append($"  眩晕: {h.skillStunDuration}s");
                sb.AppendLine();
                sb.AppendLine();
            }

            string atkType = (h != null ? h.attackType : AttackType.Melee) == AttackType.Melee ? "近战" : "远程";
            sb.AppendLine($"攻击力: {piece.attackDamage}  攻速: {piece.attackSpeed:F2}");
            sb.AppendLine($"护甲: {piece.armor}  魔抗: {piece.magicResist}");
            sb.AppendLine($"攻击距离: {piece.attackRange} ({atkType})");
            sb.AppendLine();

            int equipHash = ComputeEquipHash(piece);
            bool equipChanged = equipHash != lastDetailEquipHash;

            if (equipChanged)
            {
                lastDetailEquipHash = equipHash;
                foreach (Transform child in pieceDetailEquipContainer.transform)
                    Destroy(child.gameObject);
            }

            if (piece.equipment.Count > 0)
            {
                sb.AppendLine("<color=#40FF80>装备:</color>");
                pieceDetailEquipContainer.SetActive(true);
                if (equipChanged)
                {
                    for (int i = 0; i < piece.equipment.Count; i++)
                    {
                        var eq = piece.equipment[i];
                        if (eq == null) continue;

                        var iconGO = new GameObject($"EqIcon_{i}");
                        iconGO.transform.SetParent(pieceDetailEquipContainer.transform, false);
                        var iconRt = iconGO.AddComponent<RectTransform>();
                        iconRt.anchorMin = new Vector2(0, 0.5f);
                        iconRt.anchorMax = new Vector2(0, 0.5f);
                        iconRt.pivot = new Vector2(0, 0.5f);
                        iconRt.sizeDelta = new Vector2(32, 32);
                        iconRt.anchoredPosition = new Vector2(i * 38, 0);

                        var iconImg = iconGO.AddComponent<Image>();
                        iconImg.color = eq.displayColor;

                        var btn = iconGO.AddComponent<Button>();
                        var capturedEq = eq;
                        btn.onClick.AddListener(() => ShowEquipmentDetail(capturedEq));

                        var nameGO = new GameObject("Name");
                        nameGO.transform.SetParent(iconGO.transform, false);
                        var nameRt = nameGO.AddComponent<RectTransform>();
                        nameRt.anchorMin = Vector2.zero;
                        nameRt.anchorMax = Vector2.one;
                        nameRt.offsetMin = Vector2.zero;
                    nameRt.offsetMax = Vector2.zero;
                    var nameTmp = nameGO.AddComponent<TextMeshProUGUI>();
                    nameTmp.text = eq.equipmentName.Substring(0, 1);
                    nameTmp.fontSize = 14;
                    nameTmp.alignment = TextAlignmentOptions.Center;
                    nameTmp.color = Color.white;
                    nameTmp.raycastTarget = false;
                    }
                }
            }
            else
            {
                sb.AppendLine("<color=#808080>装备: 无</color>");
                pieceDetailEquipContainer.SetActive(false);
            }

            pieceDetailText.text = sb.ToString();
        }

        public void ShowEquipmentDetail(EquipmentData eq)
        {
            if (eq == null) return;
            if (equipDetailPanel == null) CreatePieceDetailPanel();

            var sb = new System.Text.StringBuilder();
            string typeStr = eq.equipmentType == EquipmentType.Base ? "基础装备" : "合成装备";
            sb.AppendLine($"<size=18><b><color=#40FF80>{eq.equipmentName}</color></b></size>");
            sb.AppendLine($"<color=#808080>{typeStr}</color>");
            sb.AppendLine();

            sb.AppendLine("<color=#FFA040>属性加成:</color>");
            if (eq.healthBonus != 0) sb.AppendLine($"  生命 +{eq.healthBonus}");
            if (eq.attackBonus != 0) sb.AppendLine($"  攻击力 +{eq.attackBonus}");
            if (eq.armorBonus != 0) sb.AppendLine($"  护甲 +{eq.armorBonus}");
            if (eq.magicResistBonus != 0) sb.AppendLine($"  魔抗 +{eq.magicResistBonus}");
            if (eq.attackSpeedBonus != 0) sb.AppendLine($"  攻速 +{eq.attackSpeedBonus:P0}");
            if (eq.manaBonus != 0) sb.AppendLine($"  法力 +{eq.manaBonus}");

            if (eq.lifestealPercent > 0 || eq.spellDamageBonus > 0 || eq.dodgeChance > 0)
            {
                sb.AppendLine();
                sb.AppendLine("<color=#FFA040>特殊效果:</color>");
                if (eq.lifestealPercent > 0) sb.AppendLine($"  吸血 {eq.lifestealPercent:P0}");
                if (eq.spellDamageBonus > 0) sb.AppendLine($"  法术伤害 +{eq.spellDamageBonus}");
                if (eq.dodgeChance > 0) sb.AppendLine($"  闪避 {eq.dodgeChance:P0}");
            }

            if (eq.equipmentType == EquipmentType.Combined && eq.recipe1 != null && eq.recipe2 != null)
            {
                sb.AppendLine();
                sb.AppendLine($"<color=#808080>合成: {eq.recipe1.equipmentName} + {eq.recipe2.equipmentName}</color>");
            }

            equipDetailText.text = sb.ToString();
            equipDetailPanel.SetActive(true);

            Vector2 screenPos = Input.mousePosition;
            PositionPanelNearScreenPoint(equipDetailPanel.GetComponent<RectTransform>(), screenPos, 10f);
        }

        public void ShowFactionDetail(string factionName)
        {
            var allFactions = FactionManager.Instance?.AllFactions;
            if (allFactions == null) return;

            FactionData factionData = null;
            foreach (var fd in allFactions)
            {
                if (fd != null && fd.factionName == factionName)
                { factionData = fd; break; }
            }
            if (factionData == null) return;

            if (pieceDetailPanel == null) CreatePieceDetailPanel();

            int currentCount = 0;
            var human = GameLoopManager.Instance?.HumanPlayer;
            if (human != null)
            {
                var pieces = BoardManager.Instance?.GetPiecesByOwner(human);
                if (pieces != null)
                {
                    foreach (var p in pieces)
                    {
                        if (p != null && !p.isOnBench && p.heroData?.factions != null)
                        {
                            foreach (var f in p.heroData.factions)
                                if (f == factionName) currentCount++;
                        }
                    }
                }
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"<size=20><b><color=#80D0FF>{factionName}</color></b></size>");
            sb.AppendLine($"当前: {currentCount} 个棋子");
            sb.AppendLine();

            if (factionData.thresholds != null)
            {
                sb.AppendLine("<color=#FFA040>羁绊效果:</color>");
                foreach (var t in factionData.thresholds)
                {
                    bool reached = currentCount >= t.count;
                    string mark = reached ? "<color=#40FF80>✓</color>" : " ";
                    sb.Append($"{mark} ({t.count}) ");
                    if (!string.IsNullOrEmpty(t.description))
                        sb.Append(t.description);
                    else
                    {
                        var parts = new System.Collections.Generic.List<string>();
                        if (t.healthBonus > 0) parts.Add($"生命+{t.healthBonus}");
                        if (t.attackBonus > 0) parts.Add($"攻击+{t.attackBonus}");
                        if (t.attackSpeedBonus > 0) parts.Add($"攻速+{t.attackSpeedBonus:P0}");
                        sb.Append(string.Join(", ", parts));
                    }
                    sb.AppendLine();
                }
            }

            pieceDetailText.text = sb.ToString();
            pieceDetailEquipContainer.SetActive(false);

            foreach (Transform child in factionHeroContainer.transform)
                Destroy(child.gameObject);

            var allHeroes = HeroPool.Instance?.AllHeroes;
            if (allHeroes != null)
            {
                var boardHeroNames = new System.Collections.Generic.HashSet<string>();
                if (human != null)
                {
                    var bp = BoardManager.Instance?.GetPiecesByOwner(human);
                    if (bp != null)
                        foreach (var p in bp)
                            if (p != null && !p.isOnBench && p.heroData != null)
                                boardHeroNames.Add(p.heroData.heroName);
                }

                int col = 0;
                float iconSize = 24f;
                float gap = 4f;
                foreach (var hd in allHeroes)
                {
                    if (hd?.factions == null) continue;
                    bool belongs = false;
                    foreach (var f in hd.factions)
                        if (f == factionName) { belongs = true; break; }
                    if (!belongs) continue;

                    int row = col / 8;
                    int c = col % 8;
                    float x = c * (iconSize + gap);
                    float y = -(row * (iconSize + gap + 14));

                    var entry = new GameObject(hd.heroName);
                    entry.transform.SetParent(factionHeroContainer.transform, false);
                    var entryRt = entry.AddComponent<RectTransform>();
                    entryRt.anchorMin = new Vector2(0, 1);
                    entryRt.anchorMax = new Vector2(0, 1);
                    entryRt.pivot = new Vector2(0, 1);
                    entryRt.sizeDelta = new Vector2(iconSize, iconSize + 14);
                    entryRt.anchoredPosition = new Vector2(x, y);

                    var iconGO = new GameObject("Icon");
                    iconGO.transform.SetParent(entry.transform, false);
                    var iconRt = iconGO.AddComponent<RectTransform>();
                    iconRt.anchorMin = new Vector2(0.5f, 1);
                    iconRt.anchorMax = new Vector2(0.5f, 1);
                    iconRt.pivot = new Vector2(0.5f, 1);
                    iconRt.sizeDelta = new Vector2(iconSize, iconSize);
                    iconRt.anchoredPosition = Vector2.zero;
                    var iconImg = iconGO.AddComponent<Image>();
                    var c2 = hd.displayColor;
                    bool onBoard = boardHeroNames.Contains(hd.heroName);
                    iconImg.color = new Color(c2.r, c2.g, c2.b, onBoard ? 1f : 0.4f);

                    if (onBoard)
                    {
                        var border = new GameObject("Border");
                        border.transform.SetParent(iconGO.transform, false);
                        var bRt = border.AddComponent<RectTransform>();
                        bRt.anchorMin = Vector2.zero;
                        bRt.anchorMax = Vector2.one;
                        bRt.sizeDelta = new Vector2(4, 4);
                        bRt.anchoredPosition = Vector2.zero;
                        var bImg = border.AddComponent<Image>();
                        bImg.color = new Color(1, 1, 1, 0.6f);
                        bImg.raycastTarget = false;
                        border.transform.SetAsFirstSibling();
                    }

                    var labelGO = new GameObject("Label");
                    labelGO.transform.SetParent(entry.transform, false);
                    var labelRt = labelGO.AddComponent<RectTransform>();
                    labelRt.anchorMin = new Vector2(0.5f, 0);
                    labelRt.anchorMax = new Vector2(0.5f, 0);
                    labelRt.pivot = new Vector2(0.5f, 0);
                    labelRt.sizeDelta = new Vector2(iconSize + 8, 12);
                    labelRt.anchoredPosition = Vector2.zero;
                    var labelTmp = labelGO.AddComponent<TextMeshProUGUI>();
                    labelTmp.text = hd.heroName.Length > 3 ? hd.heroName.Substring(0, 3) : hd.heroName;
                    labelTmp.fontSize = 8;
                    labelTmp.alignment = TextAlignmentOptions.Center;
                    labelTmp.color = Color.white;
                    labelTmp.enableWordWrapping = false;
                    labelTmp.overflowMode = TextOverflowModes.Truncate;

                    col++;
                }
            }

            factionHeroContainer.SetActive(true);
            pieceDetailPanel.SetActive(true);

            Vector2 screenPos = Input.mousePosition;
            PositionPanelNearScreenPoint(pieceDetailPanel.GetComponent<RectTransform>(), screenPos, 20f);
        }

        public void ShowHeroDataDetail(HeroData h)
        {
            if (h == null) return;
            if (pieceDetailPanel == null) CreatePieceDetailPanel();

            currentDetailPiece = null;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"<size=20><b>{h.heroName}</b></size>    <color=#FFD700>{h.cost}费</color>");
            sb.AppendLine();

            if (h.factions != null && h.factions.Length > 0)
            {
                sb.AppendLine($"<color=#80D0FF>羁绊: {string.Join(" / ", h.factions)}</color>");
                sb.AppendLine();
            }

            if (h.skillType != SkillType.None && !string.IsNullOrEmpty(h.skillName))
            {
                sb.AppendLine($"<color=#FFA040>技能: {h.skillName}</color>");
                string typeStr = h.skillType switch
                {
                    SkillType.Damage => "单体伤害",
                    SkillType.AreaDamage => "范围伤害",
                    SkillType.Heal => "治疗",
                    SkillType.Stun => "眩晕",
                    _ => h.skillType.ToString()
                };
                sb.Append($"类型: {typeStr}");
                if (h.skillDamage > 0) sb.Append($"  伤害: {h.skillDamage}");
                sb.AppendLine();
                sb.Append($"范围: {h.skillRange}");
                if (h.skillStunDuration > 0) sb.Append($"  眩晕: {h.skillStunDuration}s");
                sb.AppendLine();
                sb.AppendLine();
            }

            string atkType = h.attackType == AttackType.Melee ? "近战" : "远程";
            sb.AppendLine($"生命: {h.maxHealth}  攻击: {h.attackDamage}");
            sb.AppendLine($"攻速: {h.attackSpeed:F2}  护甲: {h.armor}");
            sb.AppendLine($"魔抗: {h.magicResist}  距离: {h.attackRange} ({atkType})");

            pieceDetailText.text = sb.ToString();
            pieceDetailEquipContainer.SetActive(false);
            factionHeroContainer.SetActive(false);
            pieceDetailPanel.SetActive(true);

            Vector2 screenPos = Input.mousePosition;
            PositionPanelNearScreenPoint(pieceDetailPanel.GetComponent<RectTransform>(), screenPos, 20f);
        }
    }
}
