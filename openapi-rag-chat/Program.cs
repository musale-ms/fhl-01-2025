#pragma warning disable SKEXP0070, SKEXP0001
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.VectorData;
using openapi_rag_chat;
using Qdrant.Client;


var collectionName = "graph_openapi_data";
var modelId = "all-minilm";
var endpoint = new Uri("http://localhost:11434");

var builder = Kernel.CreateBuilder();
builder.Services.AddOllamaTextEmbeddingGeneration(modelId, endpoint);

var kernel = builder.Build();
var textCompletionService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();

var collection = new QdrantVectorStoreRecordCollection<Glossary>(new QdrantClient("localhost"), collectionName);

// Wrap the collection using a decorator that allows us to expose a version that uses string keys, but internally
// we convert to and from ulong.
var stringKeyCollection = new MappingVectorStoreRecordCollection<string, ulong, Glossary, Glossary>(
    collection,
    p => ulong.Parse(p),
    i => i.ToString(),
    p => new Glossary { Key = ulong.Parse(p.Key), Category = p.Category, Term = p.Term, Definition = p.Definition, DefinitionEmbedding = p.DefinitionEmbedding },
    i => new Glossary { Key = i.Key.ToString("D"), Category = i.Category, Term = i.Term, Definition = i.Definition, DefinitionEmbedding = i.DefinitionEmbedding });

// Ingest data into the collection using the same code as we used in Step1 with the InMemory Vector Store.
await IngestDataIntoVectorStoreAsync(stringKeyCollection, textCompletionService);

// Search the vector store using the same code as we used in Step2 with the InMemory Vector Store.
var searchResultItem = await SearchVectorStoreAsync(
    stringKeyCollection,
    "What is an Application Programming Interface?",
    textCompletionService);

// Write the search result with its score to the console.
Console.WriteLine(searchResultItem.Record.Definition);
Console.WriteLine(searchResultItem.Score);

async Task<IEnumerable<string>> IngestDataIntoVectorStoreAsync(
    IVectorStoreRecordCollection<string, Glossary> collection,
    ITextEmbeddingGenerationService textEmbeddingGenerationService)
{
    // Create the collection if it doesn't exist.
    await collection.CreateCollectionIfNotExistsAsync();

    // Create glossary entries and generate embeddings for them.
    var glossaryEntries = CreateGlossaryEntries().ToList();
    var tasks = glossaryEntries.Select(entry => Task.Run(async () =>
    {
        entry.DefinitionEmbedding = await textEmbeddingGenerationService.GenerateEmbeddingAsync(entry.Definition);
    }));
    await Task.WhenAll(tasks);

    // Upsert the glossary entries into the collection and return their keys.
    var upsertedKeysTasks = glossaryEntries.Select(x => collection.UpsertAsync(x));
    var result = await Task.WhenAll(upsertedKeysTasks);
    return result;
}

async Task<VectorSearchResult<Glossary>> SearchVectorStoreAsync(
    IVectorStoreRecordCollection<string, Glossary> collection,
    string searchString,
    ITextEmbeddingGenerationService textEmbeddingGenerationService)
{
    // Generate an embedding from the search string.
    var searchVector = await textEmbeddingGenerationService.GenerateEmbeddingAsync(searchString);

    // Search the store and get the single most relevant result.
    var searchResult = await collection.VectorizedSearchAsync(
        searchVector,
        new()
        {
            Top = 1
        });
    var searchResultItems = await searchResult.Results.ToListAsync();
    return searchResultItems.First();
}

IEnumerable<Glossary> CreateGlossaryEntries()
{
    yield return new Glossary
    {
        Key = "1",
        Category = "Software",
        Term = "API",
        Definition = "Application Programming Interface. A set of rules and specifications that allow software components to communicate and exchange data."
    };

    yield return new Glossary
    {
        Key = "2",
        Category = "Software",
        Term = "SDK",
        Definition = "Software development kit. A set of libraries and tools that allow software developers to build software more easily."
    };

    yield return new Glossary
    {
        Key = "3",
        Category = "SK",
        Term = "Connectors",
        Definition = "Semantic Kernel Connectors allow software developers to integrate with various services providing AI capabilities, including LLM, AudioToText, TextToAudio, Embedding generation, etc."
    };

    yield return new Glossary
    {
        Key = "4",
        Category = "SK",
        Term = "Semantic Kernel",
        Definition = "Semantic Kernel is a set of libraries that allow software developers to more easily develop applications that make use of AI experiences."
    };

    yield return new Glossary
    {
        Key = "5",
        Category = "AI",
        Term = "RAG",
        Definition = "Retrieval Augmented Generation - a term that refers to the process of retrieving additional data to provide as context to an LLM to use when generating a response (completion) to a user’s question (prompt)."
    };

    yield return new Glossary
    {
        Key = "6",
        Category = "AI",
        Term = "LLM",
        Definition = "Large language model. A type of artificial ingelligence algorithm that is designed to understand and generate human language."
    };
}
// using Microsoft.Extensions.AI;
// using Microsoft.OpenApi.Readers;
// using Microsoft.SemanticKernel.Connectors.Qdrant;
// using openapi_rag_chat;
// using Qdrant.Client;

// // initialize qdrant
// var qdrantStore = new QdrantVectorStore(new QdrantClient("localhost"));
// var qdrantCollection = qdrantStore.GetCollection<Guid, OpenApiData>(collectionName);

// await qdrantCollection.CreateCollectionIfNotExistsAsync();

// Console.WriteLine(qdrantCollection.CollectionName);

// var ollamaGenerator = new OllamaEmbeddingGenerator(new Uri("http://localhost:11434"), "all-minilm");

// using var stream = new FileStream("assets/ccs-beta-openapi.yml", FileMode.Open, FileAccess.Read);
// var openApiDocument = new OpenApiStreamReader().Read(stream, out var diagnostic);
// // var batchData = new List<OpenApiData>();

// //     Console.WriteLine($"Title: {openApiDocument.Info.Title}");
// //     Console.WriteLine($"Version: {openApiDocument.Info.Version}");
// foreach (var path in openApiDocument.Paths)
// {
//     OpenApiData openApiData = new()
//     {
//         Key = Guid.NewGuid(),
//         Version = openApiDocument.Info.Version,
//         Server = (List<string>?)openApiDocument.Servers,
//         PathKey = path.Key
//     };
//     //         Console.WriteLine($"Path: {path.Key}");
//     foreach (var operation in path.Value.Operations)
//     {
//         openApiData.Description = operation.Value.Description;
//         openApiData.Operation = operation.Key.ToString();
//         openApiData.Summary = operation.Value.Summary;
//         var summaryVector = await ollamaGenerator.GenerateEmbeddingVectorAsync(operation.Value.Summary);
//         // var descriptionVector = await ollamaGenerator.GenerateEmbeddingVectorAsync(operation.Value.Description);
//         // var vectorsDict = new Dictionary<string, ReadOnlyMemory<float>>
//         // {
//         //    {"summary", summaryVector},
//         //     {"description", descriptionVector }
//         // };
//         // openApiData.Vector = new ReadOnlyMemory<Dictionary<string, ReadOnlyMemory<float>>>(new Dictionary<string, ReadOnlyMemory<float>>[] { vectorsDict });
//         openApiData.Vector = summaryVector;
//         await qdrantCollection.UpsertAsync(openApiData);
//         //             Console.WriteLine($"Operation: {operation.Key}");
//         //             Console.WriteLine($"Summary: {operation.Value.Summary}");
//     }
// }


//     // await qdrantCollection.UpsertBatchAsync(batchData)




