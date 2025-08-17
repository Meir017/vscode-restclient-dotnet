using RESTClient.NET.Core;
using RESTClient.NET.Core.Processing;
using System;

namespace SystemVariablesDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("RESTClient.NET System Variables Demo");
            Console.WriteLine("====================================");
            Console.WriteLine();

            // Read the demo HTTP file
            var httpFilePath = "demo-system-variables.http";
            if (!File.Exists(httpFilePath))
            {
                Console.WriteLine($"Error: {httpFilePath} not found!");
                return;
            }

            var content = File.ReadAllText(httpFilePath);
            Console.WriteLine("Original HTTP file content:");
            Console.WriteLine(new string('-', 50));
            Console.WriteLine(content);
            Console.WriteLine();

            // Parse the HTTP file
            var httpFileProcessor = new HttpFileProcessor();
            var httpFile = httpFileProcessor.ParseHttpFile(content);

            Console.WriteLine($"Parsed {httpFile.Requests.Count} requests:");
            Console.WriteLine(new string('-', 50));

            for (int i = 0; i < httpFile.Requests.Count; i++)
            {
                var request = httpFile.Requests[i];
                Console.WriteLine($"Request {i + 1}: {request.Name ?? "Unnamed"}");
                Console.WriteLine($"Method: {request.Method}");
                Console.WriteLine($"URL: {request.Url}");
                
                if (request.Headers.Any())
                {
                    Console.WriteLine("Headers:");
                    foreach (var header in request.Headers)
                    {
                        Console.WriteLine($"  {header.Key}: {header.Value}");
                    }
                }

                if (!string.IsNullOrEmpty(request.Body))
                {
                    Console.WriteLine("Body:");
                    Console.WriteLine(request.Body);
                }

                Console.WriteLine();
            }

            // Now process variables and show the resolved requests
            Console.WriteLine("Requests with variables resolved:");
            Console.WriteLine(new string('-', 50));

            var processedFile = VariableProcessor.ProcessHttpFile(httpFile);

            for (int i = 0; i < processedFile.Requests.Count; i++)
            {
                var request = processedFile.Requests[i];
                Console.WriteLine($"Processed Request {i + 1}: {request.Name ?? "Unnamed"}");
                Console.WriteLine($"Method: {request.Method}");
                Console.WriteLine($"URL: {request.Url}");
                
                if (request.Headers.Any())
                {
                    Console.WriteLine("Headers:");
                    foreach (var header in request.Headers)
                    {
                        Console.WriteLine($"  {header.Key}: {header.Value}");
                    }
                }

                if (!string.IsNullOrEmpty(request.Body))
                {
                    Console.WriteLine("Body:");
                    Console.WriteLine(request.Body);
                }

                Console.WriteLine();
            }

            Console.WriteLine("Demo completed! Press any key to exit...");
            Console.ReadKey();
        }
    }
}
