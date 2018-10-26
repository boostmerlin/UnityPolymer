using UniRx;

namespace Ginkgo
{
    public interface IEventService
    {
        IObservable<TEvent> OnEvent<TEvent>();
        void Publish<TEvent>(TEvent eventMessage);
        IEventAggregator EventAggregator { get; }
    }
}