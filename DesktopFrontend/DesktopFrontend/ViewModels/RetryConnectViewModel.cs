using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace DesktopFrontend.ViewModels
{
    public class RetryConnectViewModel : ViewModelBase
    {
        public IObservable<long> RetryAttempt { get; private set; }

        public RetryConnectViewModel()
        {
            RetryAttempt = Observable.Interval(TimeSpan.FromSeconds(2));
        }
    }
}