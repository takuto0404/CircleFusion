using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Jamaica
{
    public class UGUILineRenderer : Graphic
    {
        public Vector2[] positions = { };
        [SerializeField] private float weight;

        public void SetPositions(Vector2[] newPositions)
        {
            rectTransform.localPosition = Vector3.zero;
            var processedPosition =
                newPositions.Select(position =>
                    new Vector2(position.x - Screen.width / 2f, position.y - Screen.height / 2f)).ToArray();
            positions = processedPosition;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            if (positions.Length < 2) return;

            vh.Clear();

            for (var i = 0; i < positions.Length - 1; i++)
            {
                var position1 = positions[i];
                var position2 = positions[i + 1];

                var pos1To2 = position2 - position1;
                var verticalVector = CalculateVerticalVector(pos1To2);

                var pos1Top = position1 + verticalVector * -weight / 2;
                var pos1Bottom = position1 + verticalVector * weight / 2;
                var pos2Top = position2 + verticalVector * -weight / 2;
                var pos2Bottom = position2 + verticalVector * weight / 2;

                AddVert(vh, pos1Top);
                AddVert(vh, pos1Bottom);
                AddVert(vh, pos2Top);
                AddVert(vh, pos2Bottom);

                var indexBuffer = i * 4;

                vh.AddTriangle(0 + indexBuffer, 1 + indexBuffer, 2 + indexBuffer);
                vh.AddTriangle(1 + indexBuffer, 2 + indexBuffer, 3 + indexBuffer);
            }
        }

        private void AddVert(VertexHelper vh, Vector2 pos)
        {
            var vert = UIVertex.simpleVert;
            vert.position = pos;
            vert.color = color;
            vh.AddVert(vert);
        }

        private static Vector2 CalculateVerticalVector(Vector2 vec)
        {
            if (vec.y == 0)
            {
                return Vector2.up;
            }

            var verticalVector = new Vector2(1.0f, -vec.x / vec.y);
            return verticalVector.normalized;
        }
    }
}