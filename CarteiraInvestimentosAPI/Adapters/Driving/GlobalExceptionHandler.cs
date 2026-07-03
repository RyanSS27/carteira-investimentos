using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CarteiraInvestimentos.Adapters.Driving.GlobalExceptionHandler;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, 
        Exception exception, 
        CancellationToken cancellationToken)
    {
        // 1. Loga o erro real no console do servidor para o desenvolvedor ver
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        // 2. Mapeia o tipo de exceção para o Status Code HTTP correto
        var (statusCode, title) = exception switch
        {
            // Se foi o nosso erro de saldo insuficiente, vira 400 (Bad Request)
            InvalidOperationException => ((int)HttpStatusCode.BadRequest, "Business Rule Violation"),
            
            // Qualquer outro erro inesperado vira 500 (Internal Server Error)
            _ => ((int)HttpStatusCode.InternalServerError, "Server Error")
        };

        // 3. Monta uma resposta padrão limpa (RFC 7807 Problem Details)
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;

        // 4. Envia o JSON mastigado para o requisitor
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true; // Avisa o .NET que o erro já foi tratado com sucesso
    }
}