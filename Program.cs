using System.Reflection;
using System.ServiceProcess;
using System.Configuration.Install;


namespace Capybara
{
    class Program
    {
        static void Main(string[] args)
        {
            if (System.Environment.UserInteractive)
            {
                var parameter = string.Concat(args);
                switch (parameter)
                {
                    case "--install":
                        ManagedInstallerClass.InstallHelper(new[] { Assembly.GetExecutingAssembly().Location });
                        break;
                    case "--uninstall":
                        ManagedInstallerClass.InstallHelper(new[] { "/u", Assembly.GetExecutingAssembly().Location });
                        break;
                }
                return;
            }

            ServiceBase.Run(new Service());
        }
    }
}
