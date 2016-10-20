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
#if DEBUG
            args = new string[] { "lab2.task3" };
#endif
            if (args.Length < 1)
            {
                Console.WriteLine("Please specify a task to run");
                return;
            }
            int taskIndex;
            if (args[0].StartsWith("--matlab="))
            {
                taskIndex = 1;
                MATLAB.AddPath = args[0].Substring("--matlab=".Length);
            }
            else
            {
                taskIndex = 0;
                MATLAB.AddPath = "C:\\ann";
            }
            switch (args[taskIndex])
            {
                case "lab1.task1":
                    Lab1.Task1.Run(args.Skip(taskIndex + 1));
                    break;
                case "lab1.task2":
                    Lab1.Task2.Run(args.Skip(taskIndex + 1));
                    break;
                case "lab1.task3":
                    Lab1.Task3.Run(args.Skip(taskIndex + 1));
                    break;
                case "lab1.task4a":
                    Lab1.Task4a.Run(args.Skip(taskIndex + 1));
                    break;
                case "lab1.task4b":
                    Lab1.Task4b.Run(args.Skip(taskIndex + 1));
                    break;
                case "lab2.task1":
                    Lab2.Task1.Run(args.Skip(taskIndex + 1));
                    break;
                case "lab2.task2":
                    Lab2.Task2.Run(args.Skip(taskIndex + 1));
                    break;
                case "lab2.task3":
                    Lab2.Task3.Run(args.Skip(taskIndex + 1));
                    break;
                default:
                    Console.WriteLine("Unknown task \"" + args[taskIndex] + "\"");
                    break;
            }
        }
    }
}
