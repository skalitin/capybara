using System.ServiceProcess;

namespace Capybara
{
    class Program
    {
        static void Main(string[] args)
        {
            var servicesToRun = new ServiceBase[] { new Service()  };
            ServiceBase.Run(servicesToRun);
        }
    }
}
