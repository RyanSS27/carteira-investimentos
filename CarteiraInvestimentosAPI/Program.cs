using CarteiraInvestimentos.Adapters.Driving.GlobalExceptionHandler;
using CarteiraInvestimentos.Adapters.Driving.Validators;
using CarteiraInvestimentos.Adapters.Infrastructure.Database;
using CarteiraInvestimentos.Adapters.Infrastructure.ExternalServices;
using CarteiraInvestimentos.Domain.Services;
using CarteiraInvestimentos.Ports;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Registra o tratador global de exceções e a formatação Problem Details
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Injeção de dependências
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Permite que a API entenda e responda Enums como Texto (Strings) no JSON
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Customiza a resposta de erros de validação/desserialização nativos do .NET
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0 && e.Key != "dto") 
                .Select(e => new
                {
                    // Simplifica o caminho do campo (ex: "$.transactionType" vira apenas "transactionType")
                    property = e.Key.Replace("$.", ""), 
                    message = "O valor fornecido é invalido ou malformado."
                });

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(errors);
        };
    });

// Registra o BrapiService configurando a URL base de forma global (Sem necessidade de Token)
builder.Services.AddHttpClient<IMercadoFinanceiroService, BrapiService>(client =>
{
    client.BaseAddress = new Uri("https://brapi.dev/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

// Registra o Serviço de Negócio como SCOPED (Uma instância por requisição HTTP)
builder.Services.AddScoped<IPortfolioService, PortfolioService>();
// Varre o projeto buscando todas as classes que herdam de AbstractValidator e as registra automaticamente
builder.Services.AddValidatorsFromAssemblyContaining<CreateTransactionDtoValidator>();

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// CORREÇÃO CRÍTICA: Ativa o roteamento dos seus Controllers (TransactionsController, etc.)
app.MapControllers();

app.Run();