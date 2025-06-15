using System;

namespace Utilities.Observables
{
    public interface IObservable
    {
        /// <summary>
        /// Invoke event when inner values of object change.
        /// </summary>
        event Action OnChanged;
    }
}
