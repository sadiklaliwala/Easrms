using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Easrms.Application.Features.Request.Queries;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Application.DTOs.Request;
using Easrms.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace Easrms.Test.features.Request.Queries;

public class GetRequestByIdQueryHandlerTests
{
    private readonly Mock<IRequestRepository> _reqRepo = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly GetRequestByIdQueryHandler _handler;

    public GetRequestByIdQueryHandlerTests()
    {
        _handler = new GetRequestByIdQueryHandler(_reqRepo.Object, _mapper.Object);
    }

    [Fact]
    public async Task Should_ReturnDto_When_Found()
    {
        var reqId = Guid.NewGuid();
        var entity = new ServiceRequest { RequestId = reqId };
        var dto = new RequestDetailDto { RequestId = reqId };
        _reqRepo.Setup(r => r.GetRequestByIdAsync(reqId, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _mapper.Setup(m => m.Map<RequestDetailDto>(entity)).Returns(dto);
        var res = await _handler.Handle(new GetRequestByIdQuery { RequestId = reqId }, CancellationToken.None);
        res.Should().Be(dto);
    }

    [Fact]
    public async Task Should_ThrowNotFound_When_Missing()
    {
        var reqId = Guid.NewGuid();
        _reqRepo.Setup(r => r.GetRequestByIdAsync(reqId, It.IsAny<CancellationToken>())).ReturnsAsync((ServiceRequest?)null);
        await FluentActions.Invoking(() => _handler.Handle(new GetRequestByIdQuery { RequestId = reqId }, CancellationToken.None)).Should().ThrowAsync<KeyNotFoundException>();
    }
}
