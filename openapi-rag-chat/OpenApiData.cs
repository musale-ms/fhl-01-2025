using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Qdrant.Client.Grpc;

namespace openapi_rag_chat;

public class OpenApiData
{
    [VectorStoreRecordKey]
    public Guid Key { get; set; }
    [VectorStoreRecordData]
    public string? Version { get; set; }
    [VectorStoreRecordData]
    public List<string>? Server { get; set; }
    [VectorStoreRecordData]
    public string? PathKey { get; set; }
    [VectorStoreRecordData]
    public string? Description { get; set; }
    [VectorStoreRecordData]
    public string? Operation { get; set; }
    [VectorStoreRecordData]
    public required string Summary { get; set; }
    [VectorStoreRecordVector(384, DistanceFunction.CosineSimilarity)]
    public ReadOnlyMemory<float> Vector { get; set; }
}
