using System;
using RESTClient.NET.Testing.Extensions;
using RESTClient.NET.Core.Models;

namespace TestConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            HttpFile httpFile = null!;
            try
            {
                HttpFileExtensions.GetTestCases(httpFile);
                Console.WriteLine("No exception thrown!");
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine($"ArgumentNullException: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Other exception: {ex.GetType().Name}: {ex.Message}");
            }
        }
    }
}
