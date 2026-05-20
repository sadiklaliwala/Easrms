using System;
using Easrms.Application.DTOs.Request;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Common.Enums;
using Easrms.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Easrms.Application.Interfaces;

namespace Easrms.Application.Features.Request.Commands;

public sealed class EscalateRequestCommand : IRequest
{
    public Guid RequestId { get; init; }
    public Guid CurrentUserId { get; init; }
    public EscalateRequestDto Dto { get; init; } = new();
}

public sealed class EscalateRequestCommandHandler : IRequestHandler<EscalateRequestCommand>
{
    private readonly IRequestRepository _requestRepository;
    private readonly IEscalationRepository _escalationRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<EscalateRequestCommandHandler> _logger;

    public EscalateRequestCommandHandler(
        IRequestRepository requestRepository,
        IEscalationRepository escalationRepository,
        IEmailService emailService,
        ILogger<EscalateRequestCommandHandler> logger)
    {
        _requestRepository = requestRepository;
        _escalationRepository = escalationRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Handle(EscalateRequestCommand request, CancellationToken cancellationToken)
    {
        var entity = await _requestRepository.GetRequestByIdAsync(request.RequestId, cancellationToken);
        if (entity is null)
            throw new KeyNotFoundException($"Request with id '{request.RequestId}' was not found.");

        if (entity.Status == RequestStatusEnum.Closed || entity.Status == RequestStatusEnum.Rejected)
            throw new InvalidOperationException("Cannot escalate a closed or rejected request.");

        if (string.IsNullOrWhiteSpace(request.Dto.EscalationReason))
            throw new InvalidOperationException("Escalation reason is required.");

        entity.IsEscalated = true;
        entity.EscalatedOn = DateTime.UtcNow;
        entity.EscalatedBy = request.CurrentUserId;
        entity.EscalationReason = request.Dto.EscalationReason;

        var history = new RequestEscalationHistory
        {
            EscalationId = Guid.NewGuid(),
            RequestId = entity.RequestId,
            EscalatedBy = request.CurrentUserId,
            EscalatedOn = entity.EscalatedOn.Value,
            EscalationReason = request.Dto.EscalationReason,
            CreatedOn = DateTime.UtcNow
        };

        await _escalationRepository.AddEscalationHistoryAsync(history, cancellationToken);
        _requestRepository.Update(entity);
        await _escalationRepository.SaveChangesAsync(cancellationToken);

        var recipients = new List<string?> { entity.AssignedUser?.Email };

        var capturedNumber = entity.RequestNumber;
        var capturedTitle = entity.Title;
        var capturedReason = request.Dto.EscalationReason;

        _ = Task.Run(async () =>
        {
            try
            {
                foreach (var to in recipients.Where(e => !string.IsNullOrWhiteSpace(e)).Distinct())
                {
                    await _emailService.SendRequestEscalatedAsync(to!, capturedNumber, capturedTitle, capturedReason);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed sending escalation emails for {RequestNumber}", capturedNumber);
            }
        }, CancellationToken.None);
    }
}
