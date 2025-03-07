namespace OpenApiRagChat;
internal interface IDataloader {
    IEnumerable<OpenApiPathData<TKey>> LoadOpenApiData<TKey>(string openApiFilePath) where TKey : notnull;
}