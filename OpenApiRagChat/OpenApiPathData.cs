using Microsoft.Extensions.VectorData;
using Microsoft.OpenApi.Models;

namespace OpenApiRagChat;


public class OpenApiPathData<TKey> where TKey : notnull
{
    [VectorStoreRecordKey]
    public required TKey Key { get; set; }
    [VectorStoreRecordData]
    public string? Version { get; set; }
    // [VectorStoreRecordData]
    // public List<OpenApiserver>? Servers { get; set; }
    [VectorStoreRecordData]
    public string? PathKey { get; set; }
    [VectorStoreRecordData]
    public string? Description { get; set; }
    [VectorStoreRecordData]
    public string? Operation { get; set; }
    [VectorStoreRecordData]
    public string? Summary { get; set; }
    [VectorStoreRecordVector(3072, DistanceFunction.CosineSimilarity)]
    public ReadOnlyMemory<float> SummaryVector { get; set; }
}
