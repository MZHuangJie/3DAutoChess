using UnityEngine;
using UnityEngine.UI;

namespace AutoChess.Core
{
    public partial class ChessPiece
    {
        // Health bar
        private Canvas healthBarCanvas;
        private Image healthFillImage;
        private GameObject healthBarRoot;
        private const float HealthBarY = 2.1f;

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
    }
}
