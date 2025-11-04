using MongoDB.Bson;
using MongoDB.Driver;
using AVASphere.ApplicationCore.Sales.Entities;
using AVASphere.ApplicationCore.Sales.Interfaces;

namespace AVASphere.Infrastructure.Sales.Repositories;

public class SaleRepository // : ISaleRepository
{
    private readonly MasterDbContext _context;
}