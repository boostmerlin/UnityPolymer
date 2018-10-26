using System.Collections;

namespace Ginkgo
{
    public interface IService : IEventService
    {
        /// <summary>
        /// The setup method is called when the controller is first created and has been injected.  Use this
        /// to subscribe to any events on the EventAggregator
        /// </summary>
        void Setup();

        /// <summary>
        /// The SetupAsync method is called after the Setup method is invoke, if this service needs to load anything 
        /// async, this is the perfect place to do it.  (e.g. Logging a user in, download player data..etc) 
        /// </summary>
        /// <returns></returns>
        IEnumerator SetupAsync();

        /// <summary>
        /// The loaded method is invoked after all setup methods are invoked on a Service
        /// </summary>
        void Loaded();
    }
}