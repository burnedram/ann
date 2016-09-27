using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnnLab
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
#if DEBUG
                Task3.Run(args.Skip(1));
#else
                Console.WriteLine("Please specify a task to run");
#endif
                return;
            }
            switch(args[0])
            {
                case "task1":
                    Task1.Run(args.Skip(1));
                    break;
                case "task2":
                    Task2.Run(args.Skip(1));
                    break;
                case "task3":
                    Task3.Run(args.Skip(1));
                    break;
                default:
                    Console.WriteLine("Unknown task \"" + args[0] + "\"");
                    break;
            }
        }
    }
}
