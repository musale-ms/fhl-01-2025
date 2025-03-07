using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Embeddings;
namespace OpenApiRagChat;

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
public sealed class DataIngestor(IVectorStore vectorStore, ITextEmbeddingGenerationService textEmbeddingGenerationService)
{
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    readonly string collectionName = "ccs_beta_open_api_data";
    public async Task<IEnumerable<TKey>> ImportDataAsync<TKey>() where TKey : notnull
    {
        // Get and create collection if it doesn't exist.
        var collection = vectorStore.GetCollection<TKey, OpenApiPathData<TKey>>(collectionName);
        await collection.CreateCollectionIfNotExistsAsync();

        // Create glossary entries and generate embeddings for them.
        IDataloader dataloader = new Dataloader();
        var openApiPathData = dataloader.LoadOpenApiData<TKey>("assets/ccs-beta-openapi.yml").ToList();
        var tasks = openApiPathData.Select(path => Task.Run(async () =>
        {
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
            path.SummaryVector = await textEmbeddingGenerationService.GenerateEmbeddingAsync(path.Summary);
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine(path);
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
        }));
        await Task.WhenAll(tasks);

        // Upsert the glossary entries into the collection and return their keys.
        var upsertedKeys = openApiPathData.Select(x => collection.UpsertAsync(x));
        return await Task.WhenAll(upsertedKeys);
    }

    public Task<OpenApiPathData<TKey>?> GetOpenApiPathDataAsync<TKey>(TKey key) where TKey : notnull
    {
        var collection = vectorStore.GetCollection<TKey, OpenApiPathData<TKey>>(collectionName);
        return collection.GetAsync(key, new() { IncludeVectors = true });
    }
}
