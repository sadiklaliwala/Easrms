using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Easrms.Infrastructure.Elastic.ElasticDocuments;

namespace Easrms.Infrastructure.Elastic;

public interface IElasticService
{
    Task IndexRequestAsync(RequestDocument document);
    Task UpdateRequestAsync(RequestDocument document);
    Task DeleteRequestAsync(Guid requestId);
    Task<List<RequestDocument>> SearchRequestsAsync(string searchTerm, int page, int pageSize);
    Task SeedIndexAsync(IEnumerable<RequestDocument> documents);
}
