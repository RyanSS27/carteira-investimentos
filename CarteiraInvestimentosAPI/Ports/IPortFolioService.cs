using CarteiraInvestimentos.Domain.Entities;
using CarteiraInvestimentos.DTOs;

namespace CarteiraInvestimentos.Ports;

public interface IPortFolioService
{ 
    public Task AddTransactionAsync(Transaction transaction);
    public PortifolioDto GetPortifolioSummary();
}