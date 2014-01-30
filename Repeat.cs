using System;
using System.Threading;
using System.Threading.Tasks;

namespace Capybara
{
    public static class Repeat
    {
        public static Task Interval(TimeSpan interval, Action action, CancellationToken cancellationToken = new CancellationToken())
        {
            Action body = () =>
            {
                while (true)
                {
                    if (cancellationToken.WaitHandle.WaitOne(interval))
                    {
                        break;
                    }

                    action();
                }
            };

            return Task.Factory.StartNew(body, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
    }
}