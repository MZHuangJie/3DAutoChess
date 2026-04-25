using UnityEngine;

namespace AutoChess.Core
{
    [RequireComponent(typeof(BoardManager))]
    public class HexGridRenderer : MonoBehaviour
    {
        [SerializeField] private Color playerSideColor = new Color(0.15f, 0.4f, 0.25f, 0.8f);
        [SerializeField] private Color enemySideColor = new Color(0.4f, 0.15f, 0.15f, 0.6f);
        [SerializeField] private Color dividerColor = new Color(0.7f, 0.7f, 0.7f, 0.9f);
        [SerializeField] private Color benchColor = new Color(0.4f, 0.35f, 0.15f, 0.7f);

        private Material lineMaterial;
        private BoardManager board;

        void Start()
        {
            board = GetComponent<BoardManager>();
            CreateLineMaterial();
        }

        void CreateLineMaterial()
        {
            var shader = Shader.Find("Hidden/Internal-Colored");
            if (shader == null) return;
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            lineMaterial.SetInt("_ZWrite", 0);
        }

        void OnRenderObject()
        {
            if (board == null || lineMaterial == null) return;

            lineMaterial.SetPass(0);
            GL.PushMatrix();
            GL.MultMatrix(Matrix4x4.identity);

            float cellSize = board.Config.cellSize;

            // Draw hex grid
            for (int r = 0; r < board.Rows; r++)
            {
                bool isPlayer = r < board.PlayerRows;
                Color fillColor = isPlayer ? playerSideColor : enemySideColor;
                Color lineColor = fillColor;
                lineColor.a = 1f;

                for (int c = 0; c < board.Cols; c++)
                {
                    var slot = board.GetSlot(r, c);
                    if (slot == null) continue;

                    var corners = board.GetHexCorners(slot.worldPos, cellSize);

                    // Fill
                    GL.Begin(GL.TRIANGLES);
                    GL.Color(fillColor * 0.4f);
                    for (int i = 1; i < 5; i++)
                    {
                        GL.Vertex(corners[0] + Vector3.up * 0.01f);
                        GL.Vertex(corners[i] + Vector3.up * 0.01f);
                        GL.Vertex(corners[i + 1] + Vector3.up * 0.01f);
                    }
                    GL.End();

                    // Outline
                    GL.Begin(GL.LINES);
                    GL.Color(lineColor);
                    for (int i = 0; i < 6; i++)
                    {
                        GL.Vertex(corners[i] + Vector3.up * 0.02f);
                        GL.Vertex(corners[(i + 1) % 6] + Vector3.up * 0.02f);
                    }
                    GL.End();
                }
            }

            // Divider line between player and enemy rows
            GL.Begin(GL.LINES);
            GL.Color(dividerColor);
            float divZ = (board.GetSlotWorldPos(board.PlayerRows - 1, 0).z + board.GetSlotWorldPos(board.PlayerRows, 0).z) * 0.5f;
            float minX = board.GetSlotWorldPos(0, 0).x - cellSize * 0.6f;
            float maxX = board.GetSlotWorldPos(0, board.Cols - 1).x + cellSize * 0.6f;
            GL.Vertex(new Vector3(minX, 0.03f, divZ));
            GL.Vertex(new Vector3(maxX, 0.03f, divZ));
            GL.End();

            // Bench slots (rectangles)
            var benchConfig = board.Config;
            for (int i = 0; i < benchConfig.benchSlots; i++)
            {
                var slot = board.GetBenchSlot(i);
                if (slot == null) continue;

                float half = cellSize * 0.45f;
                Vector3 p = slot.worldPos + Vector3.up * 0.01f;

                GL.Begin(GL.QUADS);
                GL.Color(benchColor * 0.3f);
                GL.Vertex(p + new Vector3(-half, 0, -half));
                GL.Vertex(p + new Vector3(-half, 0, half));
                GL.Vertex(p + new Vector3(half, 0, half));
                GL.Vertex(p + new Vector3(half, 0, -half));
                GL.End();

                GL.Begin(GL.LINES);
                GL.Color(benchColor);
                GL.Vertex(p + new Vector3(-half, 0, -half));
                GL.Vertex(p + new Vector3(-half, 0, half));
                GL.Vertex(p + new Vector3(-half, 0, half));
                GL.Vertex(p + new Vector3(half, 0, half));
                GL.Vertex(p + new Vector3(half, 0, half));
                GL.Vertex(p + new Vector3(half, 0, -half));
                GL.Vertex(p + new Vector3(half, 0, -half));
                GL.Vertex(p + new Vector3(-half, 0, -half));
                GL.End();
            }

            GL.PopMatrix();
        }

        void OnDestroy()
        {
            if (lineMaterial != null)
                DestroyImmediate(lineMaterial);
        }
    }
}
