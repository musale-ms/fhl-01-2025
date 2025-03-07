using Microsoft.OpenApi.Readers;

namespace OpenApiRagChat;
public sealed class Dataloader() : IDataloader
{
    public IEnumerable<OpenApiPathData<TKey>> LoadOpenApiData<TKey>(string openApiFilePath) where TKey : notnull
    {
        using var stream = new FileStream(openApiFilePath, FileMode.Open, FileAccess.Read);
        var openApiDocument = new OpenApiStreamReader().Read(stream, out var diagnostic);
        List<OpenApiPathData<Guid>> data = [];
        foreach (var path in openApiDocument.Paths)
        {
            OpenApiPathData<Guid> openApiPathData = new()
            {
                Key = Guid.NewGuid(),
                Version = openApiDocument.Info.Version,
                // Servers = (List<string>?)openApiDocument.Servers,
                PathKey = path.Key
            };
            foreach (var operation in path.Value.Operations)
            {
                openApiPathData.Description = operation.Value.Description;
                openApiPathData.Operation = operation.Key.ToString();
                openApiPathData.Summary = operation.Value.Summary;
            }
            data.Add(openApiPathData);
        }
        return (IEnumerable<OpenApiPathData<TKey>>)data;
    }
}