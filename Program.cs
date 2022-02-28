using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Threading;
namespace DirectMessagesDeleter
{
    // By Mohamed | @afph | https://xmohamed.com
    internal class Program
    {
        static Login Login;
        public static int sleep, UsersLength;
        static void Main(string[] args)
        {
            Console.WindowWidth = 80; Console.WindowHeight = 15; Console.Title = "By Mohamed | @afph";
            Console.Write("Username : ");
            var username = Console.ReadLine();
            Console.Write("Password : ");
            var password = Console.ReadLine();

            Login = new Login(username, password);
            var Response = Login.StartWorker().GetAwaiter().GetResult();
            if (Response)
            {
                Console.WriteLine("Logged In");
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("Enter Sleep Durations Between Each Message [ In Seconds ] : ");
                Console.ForegroundColor = ConsoleColor.Magenta;
                sleep = Int32.Parse(Console.ReadLine());
Label1:
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("Drag And Drop The usernames .txt File : ");
                Console.ForegroundColor = ConsoleColor.Magenta;
                string path = Console.ReadLine();
                string[] users;
                try
                {
                    users = File.ReadAllLines(path);
                }catch (Exception ex)
                {
                    goto Label1; 
                }
                UsersLength = users.Length;
                foreach (string user in users)
                {
                    new MainWorker(Login.cookie, user).StartWorker();
                }
                Console.WriteLine("Finished..\nPress Anything to Exit");
                Console.ReadLine();
                Environment.Exit(0);
            }
            else
            {
                Console.WriteLine("Error in Username Or Password");
                Console.ReadLine();
                Environment.Exit(0);
            }
            Console.ReadLine();
            Environment.Exit(0);
            
        }
    }
}
