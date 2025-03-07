using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using OpenApiRagChat;

// var openApiFilePath = "assets/ccs-beta-openapi.yml";
var ollamaEndpoint = new Uri("http://localhost:11434");
var ollamaModelId = "phi3";

// IDataloader dataloader = new Dataloader();
// var data = dataloader.LoadOpenApiData(openApiFilePath);
// foreach (var d in data)
// {
//     Console.WriteLine($"{d.Operation}\t {d.PathKey}: {d.Summary}");
// }

var kernelBuilder = Kernel.CreateBuilder();

kernelBuilder.Services.AddLogging(logger => logger.AddConsole().SetMinimumLevel(LogLevel.Debug));

#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
kernelBuilder.AddOllamaTextEmbeddingGeneration(ollamaModelId, ollamaEndpoint);
#pragma warning restore SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

kernelBuilder.AddQdrantVectorStore("localhost");
kernelBuilder.Services.AddTransient<DataIngestor>();

var kernel = kernelBuilder.Build();

var dataIngestor = kernel.GetRequiredService<DataIngestor>();

await dataIngestor.ImportDataAsync<Guid>();
