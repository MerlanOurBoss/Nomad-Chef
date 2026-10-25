using NomadChef.Player;

namespace NomadChef.Counters
{
    public enum WorkMotion
    {
        Chop,
        Scrub,
    }

    /// <summary>A station a chef keeps working at while standing there (cutting board, sink). Drives the work animation.</summary>
    public interface IWorkStation
    {
        WorkMotion Motion { get; }
        bool IsWorkedBy(PlayerController player);
    }
}
