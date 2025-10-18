using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Mazad.WebApi.Endpoints.Disputes;

public static class DisputeEndpoints
{
    public static void MapDisputeEndpoints(this IEndpointRouteBuilder routes)
    {
        var disputeGroup = routes.MapGroup("/api/v1/disputes")
            .RequireAuthorization("Scope:mazad.api");

        disputeGroup.MapGet("/", ([FromQuery] int page, [FromQuery] int pageSize) =>
        {
            var disputes = new[]
            {
                new DisputeSummary(Guid.NewGuid(), Guid.NewGuid(), "Not as described", "Open", DateTimeOffset.UtcNow.AddHours(-5))
            };

            return Results.Ok(new DisputeListResponse(page, pageSize, disputes));
        });

        routes.MapPost("/api/v1/orders/{orderId:guid}/disputes", (Guid orderId, [FromBody] CreateDisputeRequest request) =>
        {
            var dispute = new DisputeDetails(Guid.NewGuid(), orderId, request.Reason, "Open", DateTimeOffset.UtcNow, new List<DisputeMessage>());
            return Results.Created($"/api/v1/disputes/{dispute.DisputeId}", dispute);
        }).RequireAuthorization("Scope:mazad.api");

        disputeGroup.MapGet("/{disputeId:guid}", (Guid disputeId) =>
        {
            var dispute = new DisputeDetails(disputeId, Guid.NewGuid(), "Item damaged", "Open", DateTimeOffset.UtcNow.AddDays(-1), new List<DisputeMessage>
            {
                new DisputeMessage(Guid.NewGuid(), disputeId, Guid.NewGuid(), "Buyer", "Item arrived with scratches", DateTimeOffset.UtcNow.AddHours(-12))
            });

            return Results.Ok(dispute);
        });

        disputeGroup.MapPost("/{disputeId:guid}/messages", (Guid disputeId, [FromBody] AddDisputeMessageRequest request) =>
        {
            var message = new DisputeMessage(Guid.NewGuid(), disputeId, request.SenderId, request.SenderRole, request.Message, DateTimeOffset.UtcNow);
            return Results.Ok(message);
        });

        disputeGroup.MapPost("/{disputeId:guid}/resolve", (Guid disputeId, [FromBody] ResolveDisputeRequest request) =>
        {
            var resolution = new DisputeResolutionResponse(disputeId, request.ProposedOutcome, "pending-acceptance", DateTimeOffset.UtcNow);
            return Results.Ok(resolution);
        });

        disputeGroup.MapPost("/{disputeId:guid}/escalate", (Guid disputeId) =>
        {
            var escalation = new DisputeEscalationResponse(disputeId, "escalated", DateTimeOffset.UtcNow);
            return Results.Ok(escalation);
        });

        disputeGroup.MapPut("/{disputeId:guid}/decision", (Guid disputeId, [FromBody] DisputeDecisionRequest request) =>
        {
            var decision = new DisputeDecisionResponse(disputeId, request.Decision, request.Notes, DateTimeOffset.UtcNow);
            return Results.Ok(decision);
        }).RequireAuthorization("Scope:mazad.admin");
    }

    public record DisputeSummary(Guid DisputeId, Guid OrderId, string Reason, string Status, DateTimeOffset UpdatedAtUtc);

    public record DisputeListResponse(int Page, int PageSize, IEnumerable<DisputeSummary> Disputes);

    public record CreateDisputeRequest(Guid BuyerId, string Reason, string Description);

    public record DisputeDetails(Guid DisputeId, Guid OrderId, string Reason, string Status, DateTimeOffset CreatedAtUtc, IEnumerable<DisputeMessage> Messages);

    public record DisputeMessage(Guid MessageId, Guid DisputeId, Guid SenderId, string SenderRole, string Message, DateTimeOffset SentAtUtc);

    public record AddDisputeMessageRequest(Guid SenderId, string SenderRole, string Message);

    public record ResolveDisputeRequest(string ProposedOutcome, string? Notes);

    public record DisputeResolutionResponse(Guid DisputeId, string Outcome, string Status, DateTimeOffset SubmittedAtUtc);

    public record DisputeEscalationResponse(Guid DisputeId, string Status, DateTimeOffset EscalatedAtUtc);

    public record DisputeDecisionRequest(string Decision, string? Notes);

    public record DisputeDecisionResponse(Guid DisputeId, string Decision, string? Notes, DateTimeOffset DecidedAtUtc);
}
