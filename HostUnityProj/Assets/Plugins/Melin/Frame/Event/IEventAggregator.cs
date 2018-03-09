/// <summary>
/// This is code from uFrame.
/// https://github.com/micahosborne/uFrame.git
/// </summary>
using UniRx;
using System; // Required for WP8 and Store APPS

namespace ML
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class EventId : Attribute
    {
        public int Identifier
        {
            get; set;
        }
        public EventId(int id)
        {
            Identifier = id;
        }
    }

    public interface IEventAggregator
    {
        IObservable<TEvent> GetEvent<TEvent>();
        void Publish<TEvent>(TEvent evt);
        bool DebugEnabled { get; set; }
    }
}