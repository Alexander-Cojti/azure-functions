[FunctionName("GetAllPaged")]
public static async Task<HttpResponseMessage> ReadAll(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetAllPaged/{pageSize?}/{token?}")]HttpRequestMessage req,[CosmosDB(databaseName: "taskDatabase", collectionName: "Boleta", ConnectionStringSetting = "Connection")]DocumentClient client,
    int? pageSize, string token, ILogger log, [Inject]IComponent<EventModel> component)
{
    try
    {
        log.LogInformation("Get all events");

        var response = await component.GetAll_Paged(pageSize, token);

        return req.CreateResponse(HttpStatusCode.OK, response);
    }
    catch (Exception ex)
    {
        log.LogError(ex.Message, ex);
        return req.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
    }
}