using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using OpenApiRagChat;
using Qdrant.Client;

var ollamaEndpoint = new Uri("http://localhost:11434");
var ollamaModelId = "phi3";

var kernelBuilder = Kernel.CreateBuilder();

kernelBuilder.Services.AddLogging(logger => logger.AddConsole().SetMinimumLevel(LogLevel.Debug));

#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
kernelBuilder.AddOllamaTextEmbeddingGeneration(ollamaModelId, ollamaEndpoint);
kernelBuilder.Services.AddOllamaChatCompletion(ollamaModelId, ollamaEndpoint);
#pragma warning restore SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

kernelBuilder.AddQdrantVectorStore("localhost");
kernelBuilder.Services.AddTransient<DataIngestor>();

var kernel = kernelBuilder.Build();

var dataIngestor = kernel.GetRequiredService<DataIngestor>();

// await dataIngestor.ImportDataAsync<Guid>();

var collection = dataIngestor.GetCollection<Guid>();
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
var textEmbeddingGenerationService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
var vectorStoreSearch = new VectorStoreTextSearch<OpenApiPathData<Guid>>(collection, textEmbeddingGenerationService);
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
kernel.Plugins.Add(vectorStoreSearch.CreateWithGetTextSearchResults("SearchPlugin"));


// Chat

while (true)
{
    // Get user input
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("User > ");
    var question = Console.ReadLine()!;
    // Clean resources and exit the demo if the user input is null or empty
    if (question is null || string.IsNullOrWhiteSpace(question))
    {
        // To avoid any potential memory leak all disposable
        // services created by the kernel are disposed
        DisposeServices(kernel);
        return;
    }
    var searchVector = await textEmbeddingGenerationService.GenerateEmbeddingAsync(question);

    // Search the store with a filter and get the single most relevant result.
    var searchResult = await collection.VectorizedSearchAsync(
        searchVector,
        new()
        {
            Top = 1
        });
    var searchResultItems = searchResult.Results.ToBlockingEnumerable();

    foreach (var item in searchResultItems)
    {
        Console.WriteLine($"-----Result Score {item.Score}------");
        Console.WriteLine(item.Record.Summary);
        Console.WriteLine($"{item.Record.Operation}\t{item.Record.PathKey}");
        Console.WriteLine();
    }
    Console.WriteLine();
}

void DisposeServices(Kernel kernel)
{
    foreach (var target in kernel
        .GetAllServices<IChatCompletionService>()
        .OfType<IDisposable>())
    {
        target.Dispose();
    }
}