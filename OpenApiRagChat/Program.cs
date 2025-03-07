using OpenApiRagChat;

var openApiFilePath = "assets/ccs-beta-openapi.yml";

IDataloader dataloader = new Dataloader();
var data = dataloader.LoadOpenApiData(openApiFilePath);
foreach (var d in data)
{
    Console.WriteLine($"{d.Operation}\t {d.PathKey}: {d.Summary}");
}