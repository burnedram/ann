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
                Lab2.Task2.Run(args.Skip(1));
#else
                Console.WriteLine("Please specify a task to run");
#endif
                return;
            }
            switch(args[0])
            {
                case "lab1.task1":
                    Lab1.Task1.Run(args.Skip(1));
                    break;
                case "lab1.task2":
                    Lab1.Task2.Run(args.Skip(1));
                    break;
                case "lab1.task3":
                    Lab1.Task3.Run(args.Skip(1));
                    break;
                case "lab1.task4a":
                    Lab1.Task4a.Run(args.Skip(1));
                    break;
                case "lab1.task4b":
                    Lab1.Task4b.Run(args.Skip(1));
                    break;
                case "lab2.task1":
                    Lab2.Task1.Run(args.Skip(1));
                    break;
                case "lab2.task2":
                    Lab2.Task2.Run(args.Skip(1));
                    break;
                default:
                    Console.WriteLine("Unknown task \"" + args[0] + "\"");
                    break;
            }
        }
    }
}
