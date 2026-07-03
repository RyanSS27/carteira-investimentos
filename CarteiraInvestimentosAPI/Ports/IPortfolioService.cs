using CarteiraInvestimentos.Domain.Entities;
using CarteiraInvestimentos.DTOs;

namespace CarteiraInvestimentos.Ports;

public interface IPortfolioService
{ 
    public Task AddTransactionAsync(Transaction transaction);
    public PortifolioDto GetPortifolioSummary();
}