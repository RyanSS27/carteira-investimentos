using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CarteiraInvestimentos.Domain.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace CarteiraInvestimentos.Domain.Entities;

[Table("Transaction")]
public class Transaction
{
    [Key]
    [Column("transaction_id")]
    public Guid TransactionId { get; private set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(10)]
    [Column("transaction_date")]
    public String Ticker { get; set; }
    
    [Required]
    [Column("quantity")]
    public int Quantity { get; set; }
    
    [Required]
    [Precision(18, 2)] // Número máximo de 18 casas (contando com os 2 pós vírgula)
    [Column("unit_amount")]
    public decimal UnitPrice { get; set; }
    
    [Required]
    [Column("transaction_type")]
    public TransactionType TransactionType { get; set; }
    
    [Required]
    [Column("transaction_date")]
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    
    public Transaction() { }
}

