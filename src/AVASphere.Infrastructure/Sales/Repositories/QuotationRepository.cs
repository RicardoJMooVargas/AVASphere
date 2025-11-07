using MongoDB.Driver;
using AVASphere.ApplicationCore.Sales.Entities;
using AVASphere.ApplicationCore.Sales.Interfaces;

namespace AVASphere.Infrastructure.Sales.Repositories;

public class QuotationRepository //: IQuotationRepository
{
    private readonly MasterDbContext _context;
}