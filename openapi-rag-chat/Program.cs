using System;
using System.IO;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

class Program
{
    static void Main(string[] args)
    {
        using (var stream = new FileStream("assets/ccs-beta-openapi.yml", FileMode.Open, FileAccess.Read))
        {
            var openApiDocument = new OpenApiStreamReader().Read(stream, out var diagnostic);

            Console.WriteLine($"Title: {openApiDocument.Info.Title}");
            Console.WriteLine($"Version: {openApiDocument.Info.Version}");
            foreach (var path in openApiDocument.Paths)
            {
                Console.WriteLine($"Path: {path.Key}");
                foreach (var operation in path.Value.Operations)
                {
                    Console.WriteLine($"Operation: {operation.Key}");
                    Console.WriteLine($"Summary: {operation.Value.Summary}");
                }
            }
        }
    }
}