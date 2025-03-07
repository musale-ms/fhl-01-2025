using Microsoft.OpenApi.Readers;

namespace OpenApiRagChat;
internal sealed class Dataloader() : IDataloader
{
    IEnumerable<OpenApiPathData> IDataloader.LoadOpenApiData(string openApiFilePath)
    {
        using var stream = new FileStream(openApiFilePath, FileMode.Open, FileAccess.Read);
        var openApiDocument = new OpenApiStreamReader().Read(stream, out var diagnostic);
        List<OpenApiPathData> data = [];
        foreach (var path in openApiDocument.Paths)
        {
            OpenApiPathData openApiPathData = new()
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
        return data;
    }
}