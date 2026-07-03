using CarteiraInvestimentos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarteiraInvestimentos.Adapters.Infrastructure.Database;

/*
    Essa classe contém todas as minhas conexões com as tabelas e suas configurações particulares.
    Aqui, não tenho um repository para cada Entity, tenho 1 arquivo de contexto com todas as conexões.
    Logo, as "Specifications" também estão aqui.
    
    Está classe receberá uma interface que será injetada no service.
*/
public class ApplicationDbContext : DbContext
{
    public  ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) // Recebe a string de conexão
        : base(options)
    {}
    
    /*
        Este é meu acesso às tabelas, com as operações de .Add(), .Remove(), .FIND(), etc.
        - Consultas LINQ (queries)
    */
    public DbSet<Transaction> Transactions { get; set; } 
    
    /*
        Executado quando a aplicação .NET inicia para mapear as tabelas e as especificações que eu definir
        (como a que faz Enum -> String)
    */
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Enum -> String
        modelBuilder.Entity<Transaction>()
            .Property(t => t.TransactionType)
            .HasConversion<string>();
    }
}