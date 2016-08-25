using System;
using Microsoft.Owin.Hosting;

namespace issues_web_api
{
    class Program
    {
        static void Main(string[] args)
        {
            const string baseAddr = "http://localhost:8090";
            using (WebApp.Start<Startup>(baseAddr))
            {
                Console.WriteLine($"Hosted at: {baseAddr}");
                Console.ReadLine();
            }
        }
    }
}
