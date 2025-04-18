namespace Dread.Battle.Util
{
    /// <summary>
    /// 体力情報を提供するインターフェース
    /// </summary>
    public interface IHealthProvider
    {
        int CurrentHealth { get; }
        int MaxHealth { get; }
    }
}
