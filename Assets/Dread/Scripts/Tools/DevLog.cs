using System;
using UnityEngine;

[Flags]
public enum LogCategory
{
    None = 0,
    General = 1 << 0,
    Bullet = 1 << 1,
    Enemy = 1 << 2,
    Spawn = 1 << 3,
    UI = 1 << 4,
    Audio = 1 << 5,
    Input = 1 << 6,
    Effect = 1 << 7,
    Turret = 1 << 8,
    All = ~0,
}

public static class DevLog
{
    private static string ColoringByCategory(string message, LogCategory category)
    {
        var col = category switch
        {
            LogCategory.General => "#FFFFFF",
            LogCategory.Bullet => "#FFFF00",
            LogCategory.Enemy => "#FF6666",
            LogCategory.Spawn => "#66CCFF",
            LogCategory.UI => "#00FF99",
            LogCategory.Audio => "#CC99FF",
            LogCategory.Input => "#CCCCCC",
            LogCategory.Effect => "#FF99CC",
            LogCategory.Turret => "#6666FF",
            _ => "#AAAAAA"
        };

        return $"<color={col}>{message}</color>";
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private static readonly Logger Logger = new Logger(Debug.unityLogger.logHandler);

    // 表示対象のカテゴリ（Editorから操作可能）
    public static LogCategory EnabledCategories = LogCategory.All;
#endif

    /// <summary>
    /// 通常ログ
    /// </summary>
    public static void Log(string message, LogCategory category = LogCategory.General)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if ((EnabledCategories & category) != 0)
        {
            Logger.Log(LogType.Log, ColoringByCategory($"[{category}] {message}", category));
        }
#endif
    }

    /// <summary>
    /// 警告ログ
    /// </summary>
    public static void LogWarning(string message, LogCategory category = LogCategory.General)
    {
        Debug.LogWarning(ColoringByCategory($"[{category}] {message}", category));
    }

    /// <summary>
    /// エラーログ
    /// </summary>
    public static void LogError(string message, LogCategory category = LogCategory.General)
    {
        Debug.LogError(ColoringByCategory($"[{category}] {message}", category));
    }

    /// <summary>
    /// エクセプション出力ログ
    /// </summary>
    public static void LogException(Exception ex, LogCategory category = LogCategory.General)
    {
        Debug.LogException(new Exception($"[{category}] {ex.Message}", ex));
    }
}
