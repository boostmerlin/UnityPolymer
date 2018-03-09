using ML.IOC;
using UniRx;
namespace ML
{
    public class MComponent : IEventService, IDisposableContainer, IIOCService
    {
        private CompositeDisposable _disposer;
        CompositeDisposable IDisposableContainer.Disposer
        {
            get { return _disposer ?? (_disposer = new CompositeDisposable()); }
        }

        public IEventAggregator EventAggregator
        {
            get
            {
                return MSystem.EventAggregator;
            }
        }

        public IContainer Container
        {
            get
            {
                return MSystem.Container;
            }
        }

        public void Dispose()
        {
            if (_disposer != null)
            {
                _disposer.Dispose();
            }
        }

        public IObservable<TEvent> OnEvent<TEvent>()
        {
            return EventAggregator.GetEvent<TEvent>();
        }

        public void Publish<TEvent>(TEvent eventMessage)
        {
            EventAggregator.Publish(eventMessage);
        }
    }
}
