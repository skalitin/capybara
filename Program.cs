using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;

namespace Capybara
{
    class Program
    {
        static void Main(string[] args)
        {
            new Watcher().Watch();
            Console.ReadLine();
        }
    }
}
