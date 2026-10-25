using System;

namespace NomadChef
{
    /// <summary>Anything that shows a progress bar (cutting board, kazan...). Progress is normalized 0..1.</summary>
    public interface IHasProgress
    {
        event Action<float> ProgressChanged;
    }
}
