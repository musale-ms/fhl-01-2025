namespace OpenApiRagChat;
internal interface IDataloader {
    IEnumerable<OpenApiPathData> LoadOpenApiData(string openApiFilePath);
}