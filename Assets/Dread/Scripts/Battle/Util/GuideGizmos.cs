using UnityEngine;
using System;
using Sirenix.OdinInspector;

namespace Dread.Battle.Util
{
    /// <summary>
    /// ゲーム内のガイドとなるギズモを表示するためのクラス
    /// </summary>
    public class GuideGizmos : MonoBehaviour
    {
        [Serializable]
        public class GizmoSettings
        {
            [LabelText("表示"), ToggleLeft]
            public bool isVisible = true;

            [LabelText("色"), ColorUsage(true, false)]
            public Color color = Color.white;

            [LabelText("サイズ"), MinValue(0.1f)]
            public Vector3 size = Vector3.one;

            [LabelText("位置オフセット")]
            public Vector3 offset = Vector3.zero;
        }

        [FoldoutGroup("戦艦ガイド設定", expanded: true)]
        [SerializeField]
        private GizmoSettings _shipGuide = new GizmoSettings
        {
            color = new Color(0, 1, 0, 0.5f), // 緑色半透明
            size = new Vector3(5, 2, 10), // 戦艦の一般的なサイズ
        };

        [FoldoutGroup("バトルエリア設定", expanded: true)]
        [SerializeField]
        private GizmoSettings _battleArea = new GizmoSettings
        {
            color = new Color(1, 0, 0, 0.3f), // 赤色半透明
            size = new Vector3(50, 20, 100), // バトルエリアの大きさ
        };

        private void OnDrawGizmos()
        {
            DrawGizmo(_shipGuide);
            DrawGizmo(_battleArea);
        }

        [InfoBox("シーンビューでギズモを確認できます。ゲーム実行中は表示されません。")]
        [Button("シーンビューを更新")]
        private void RepaintSceneView()
        {
#if UNITY_EDITOR
            UnityEditor.SceneView.RepaintAll();
#endif
        }

        private void DrawGizmo(GizmoSettings settings)
        {
            if (!settings.isVisible)
                return;

            // 元のギズモの色を保存
            Color originalColor = Gizmos.color;

            // 新しい色を設定
            Gizmos.color = settings.color;

            // ワイヤーキューブを描画
            Matrix4x4 originalMatrix = Gizmos.matrix;
            Vector3 position = transform.position + settings.offset;

            // ローカル座標系でのギズモ描画
            Gizmos.matrix = Matrix4x4.TRS(position, transform.rotation, settings.size);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

            // 元の行列に戻す
            Gizmos.matrix = originalMatrix;

            // 元の色に戻す
            Gizmos.color = originalColor;
        }
    }
}
