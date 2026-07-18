using System.Net;
using Microsoft.AspNetCore.Mvc;
using NestHub.Application.Common.Exceptions;
using NestHub.Domain.Common;
using ValidationException = NestHub.Application.Common.Exceptions.ValidationException;

namespace NestHub.API.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var problemDetails = exception switch
        {
            ValidationException validationException => new ProblemDetails
            {
                Title = "One or more validation errors occurred.",
                Status = (int)HttpStatusCode.BadRequest,
                Extensions = { ["errors"] = validationException.Errors }
            },
            NotFoundException => new ProblemDetails
            {
                Title = exception.Message,
                Status = (int)HttpStatusCode.NotFound
            },
            DomainException => new ProblemDetails
            {
                Title = exception.Message,
                Status = (int)HttpStatusCode.BadRequest
            },
            UnauthorizedAccessException => new ProblemDetails
            {
                Title = exception.Message,
                Status = (int)HttpStatusCode.Unauthorized
            },
            InvalidOperationException => new ProblemDetails
            {
                Title = exception.Message,
                Status = (int)HttpStatusCode.Conflict
            },
            _ => new ProblemDetails
            {
                Title = "An unexpected error occurred.",
                Status = (int)HttpStatusCode.InternalServerError
            }
        };

        if (problemDetails.Status == (int)HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception processing {Path}", context.Request.Path);

        context.Response.StatusCode = problemDetails.Status!.Value;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
