using HotChocolate;
using HotChocolate.Types;
using LoggingService.Storage;

namespace LoggingService.GraphQL;

public sealed class LogQueries
{
    [GraphQLDescription("Obtiene los logs paginados provenientes del colector gRPC.")]
    public LogPage GetLogs(
        [Service] ILogStore store,
        int page = 1,
        int pageSize = 25)
    {
        return store.GetPage(page, pageSize);
    }
}
