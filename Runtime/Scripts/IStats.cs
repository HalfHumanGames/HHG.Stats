namespace HHG.StatSystem.Runtime
{
    public interface IStats
    {
        Stat this[string name] { get; }
    }
}