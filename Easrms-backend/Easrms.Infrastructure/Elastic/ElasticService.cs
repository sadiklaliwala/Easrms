using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Easrms.Infrastructure.Elastic.ElasticDocuments;
using Nest;

namespace Easrms.Infrastructure.Elastic;

public class ElasticService : IElasticService
{
    private readonly IElasticClient _client;
    private const string IndexName = "easrms-requests";

    public ElasticService(IElasticClient client)
    {
        _client = client;
    }

    public async Task IndexRequestAsync(RequestDocument document)
    {
        await _client.IndexAsync(document, i => i.Index(IndexName).Id(document.RequestId.ToString()));
    }

    public async Task UpdateRequestAsync(RequestDocument document)
    {
        await _client.UpdateAsync<RequestDocument>(document.RequestId.ToString(), u => u.Index(IndexName).Doc(document));
    }

    public async Task DeleteRequestAsync(Guid requestId)
    {
        await _client.DeleteAsync<RequestDocument>(requestId.ToString(), d => d.Index(IndexName));
    }

    public async Task<List<RequestDocument>> SearchRequestsAsync(string searchTerm, int page, int pageSize)
    {
        var response = await _client.SearchAsync<RequestDocument>(s => s
            .Index(IndexName)
            .From((page - 1) * pageSize)
            .Size(pageSize)
            .Query(q => q
                .MultiMatch(m => m
                    .Fields(f => f
                        .Field(p => p.Title, 3)
                        .Field(p => p.RequestNumber, 2)
                        .Field(p => p.Description)
                        .Field(p => p.CategoryName)
                        .Field(p => p.EmployeeName)
                        .Field(p => p.AssigneeName))
                    .Query(searchTerm)
                    .Fuzziness(Fuzziness.Auto))));

        return response.Documents.ToList();
    }

    public async Task SeedIndexAsync(IEnumerable<RequestDocument> documents)
    {
        var exists = await _client.Indices.ExistsAsync(IndexName);
        if (!exists.Exists)
        {
            await _client.Indices.CreateAsync(IndexName, c => c
                .Map<RequestDocument>(m => m.AutoMap())
            );
        }

        var bulk = new BulkDescriptor();
        foreach (var doc in documents)
        {
            bulk.Index<RequestDocument>(i => i.Index(IndexName).Document(doc).Id(doc.RequestId.ToString()));
        }

        await _client.BulkAsync(bulk);
    }
}
