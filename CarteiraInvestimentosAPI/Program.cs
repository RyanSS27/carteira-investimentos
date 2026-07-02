using CarteiraInvestimentos.Adapters.Driving.Validators;
using CarteiraInvestimentos.Adapters.Infrastructure.Database;
using CarteiraInvestimentos.Domain.Services;
using CarteiraInvestimentos.Ports;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Injeção de dependências
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddControllers();
// Registra o Serviço de Negócio como SCOPED (Uma instância por requisição HTTP)
builder.Services.AddScoped<IPortfolioService, PortfolioService>();
// Varre o projeto buscando todas as classes que herdam de AbstractValidator e as registra automaticamente
builder.Services.AddValidatorsFromAssemblyContaining<CreateTransactionDtoValidator>();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();