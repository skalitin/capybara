using System.ServiceProcess;
using NLog;

namespace Capybara
{
    partial class Service : ServiceBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Watcher _watcher;

        public Service()
        {
            InitializeComponent();
            _watcher = new Watcher();
        }

        protected override void OnStart(string[] args)
        {
            Logger.Info("Starting watcher...");
            _watcher.Watch();
        }

        protected override void OnStop()
        {
            Logger.Info("Stopping watcher...");
            if (_watcher != null)
            {
                _watcher.Stop();
            }
        }
    }
}
