#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class DevLogWindow : EditorWindow
{
    [MenuItem("Tools/DevLog 設定")]
    public static void ShowWindow()
    {
        GetWindow<DevLogWindow>("DevLog");
    }

    private void OnGUI()
    {
        GUILayout.Label("ログ出力カテゴリ", EditorStyles.boldLabel);

        // フラグとして選択
        DevLog.EnabledCategories = (LogCategory)
            EditorGUILayout.EnumFlagsField("表示カテゴリ", DevLog.EnabledCategories);

        GUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "この設定は Editor / Development Build にのみ有効です。\nリリースビルドでは Log() は無効化されますが、LogError/Exception は残ります。",
            MessageType.Info
        );
    }
}
#endif
