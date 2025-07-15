using System;

namespace Utilities.Observables
{
    public interface IObservable<out T, out TC>
        where TC : IChange<T>
    {
        /// <summary>
        /// Invoke event when inner values of object change.
        /// </summary>
        event Action<TC> OnChanged;
    }
}
