using UniRx;

namespace ML
{
    public interface IEventService
    {
        IObservable<TEvent> OnEvent<TEvent>();
        void Publish<TEvent>(TEvent eventMessage);
        IEventAggregator EventAggregator { get; }
    }
}