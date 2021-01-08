namespace GitLurker.Console
{
    using System;
    using GitLurker.Models;

    class Program
    {
        static void Main(string[] args)
        {
            var wp = new Workspace(@"D:\Github");
            Console.WriteLine("Hello World!");
        }
    }
}
