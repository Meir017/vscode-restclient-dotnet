using System;
using System.IO;
using System.Threading.Tasks;
using RESTClient.NET.Core.Parsing;

namespace RESTClient.NET.FileBodyDemo
{
    /// <summary>
    /// Demonstrates the file body functionality in RESTClient.NET
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("RESTClient.NET File Body Demo");
            Console.WriteLine("============================");
            Console.WriteLine();

            // Load the demo HTTP file
            var httpFilePath = "file-body-demo.http";
            if (!File.Exists(httpFilePath))
            {
                Console.WriteLine($"Demo file '{httpFilePath}' not found!");
                Console.WriteLine("Make sure to run this demo from the repository root directory.");
                return;
            }

            try
            {
                // Parse the HTTP file
                var parser = new HttpFileParser();
                var content = await File.ReadAllTextAsync(httpFilePath);
                var httpFile = await parser.ParseAsync(content);

                Console.WriteLine($"Parsed HTTP file with {httpFile.Requests.Count} requests:");
                Console.WriteLine();

                // Display file variables
                Console.WriteLine("File Variables:");
                foreach (var variable in httpFile.FileVariables)
                {
                    Console.WriteLine($"  @{variable.Key} = {variable.Value}");
                }
                Console.WriteLine();

                // Display each request with file body information
                foreach (var request in httpFile.Requests)
                {
                    Console.WriteLine($"Request: {request.Name}");
                    Console.WriteLine($"  Method: {request.Method}");
                    Console.WriteLine($"  URL: {request.Url}");
                    
                    if (request.Headers.Count > 0)
                    {
                        Console.WriteLine("  Headers:");
                        foreach (var header in request.Headers)
                        {
                            Console.WriteLine($"    {header.Key}: {header.Value}");
                        }
                    }

                    if (request.FileBodyReference != null)
                    {
                        Console.WriteLine("  File Body Reference:");
                        Console.WriteLine($"    Path: {request.FileBodyReference.FilePath}");
                        Console.WriteLine($"    Process Variables: {request.FileBodyReference.ProcessVariables}");
                        Console.WriteLine($"    Encoding: {request.FileBodyReference.Encoding?.WebName ?? "Default (UTF-8)"}");
                        Console.WriteLine($"    Line Number: {request.FileBodyReference.LineNumber}");
                        
                        // Check if the referenced file exists
                        var fileExists = File.Exists(request.FileBodyReference.FilePath);
                        Console.WriteLine($"    File Exists: {fileExists}");
                        
                        if (fileExists)
                        {
                            try
                            {
                                var fileInfo = new FileInfo(request.FileBodyReference.FilePath);
                                Console.WriteLine($"    File Size: {fileInfo.Length} bytes");
                                Console.WriteLine($"    Last Modified: {fileInfo.LastWriteTime}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"    Error reading file info: {ex.Message}");
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(request.Body))
                    {
                        Console.WriteLine("  Inline Body:");
                        var bodyPreview = request.Body.Length > 100 
                            ? request.Body.Substring(0, 100) + "..."
                            : request.Body;
                        Console.WriteLine($"    {bodyPreview}");
                    }

                    if (request.Metadata.Expectations.Count > 0)
                    {
                        Console.WriteLine("  Expectations:");
                        foreach (var expectation in request.Metadata.Expectations)
                        {
                            Console.WriteLine($"    {expectation.Type}: {expectation.Value}");
                        }
                    }

                    Console.WriteLine();
                }

                // Demonstrate file body syntax parsing
                Console.WriteLine("File Body Syntax Examples:");
                Console.WriteLine("=========================");

                var syntaxExamples = new[]
                {
                    "< ./static-data.xml",
                    "<@ ./template.json", 
                    "<@utf8 ./unicode-data.txt",
                    "<@latin1 ./legacy-data.txt",
                    "<@windows1252 ./windows-data.txt",
                    "< C:\\absolute\\path\\data.xml",
                    "<@ /unix/absolute/path/template.json"
                };

                foreach (var syntax in syntaxExamples)
                {
                    try
                    {
                        var testContent = $@"# @name test
POST https://example.com/api
Content-Type: application/json

{syntax}";

                        var testHttpFile = await parser.ParseAsync(testContent);
                        var testRequest = testHttpFile.GetRequestByName("test");
                        
                        if (testRequest.FileBodyReference != null)
                        {
                            Console.WriteLine($"Syntax: {syntax}");
                            Console.WriteLine($"  → File Path: {testRequest.FileBodyReference.FilePath}");
                            Console.WriteLine($"  → Process Variables: {testRequest.FileBodyReference.ProcessVariables}");
                            Console.WriteLine($"  → Encoding: {testRequest.FileBodyReference.Encoding?.WebName ?? "Default"}");
                            Console.WriteLine();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error parsing syntax '{syntax}': {ex.Message}");
                    }
                }

                Console.WriteLine("Demo completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing HTTP file: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
