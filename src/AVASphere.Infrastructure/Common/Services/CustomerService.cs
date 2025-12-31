using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.ApplicationCore.Common.Interfaces;
using AVASphere.ApplicationCore.Common.Entities.General;
using AVASphere.ApplicationCore.Common.Entities.Jsons;

namespace AVASphere.Infrastructure.Common.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _repository;

        public CustomerService(ICustomerRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<CustomerDto>> GetAsync(CustomerFilterDto? filters)
        {
            var customers = await _repository.SelectAsync(filters?.IdCustomer, filters?.LastName, filters?.ExternalId);
            return customers.Select(MapToDto);
        }

        public async Task<CustomerDto> NewAsync(CustomerCreateRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Convertir DTOs simplificados a entidades con índices auto-generados
            SettingsCustomerJson? settingsJson = null;
            if (request.Settings != null)
            {
                settingsJson = new SettingsCustomerJson
                {
                    Index = await _repository.GetNextIndexForSettingsAsync(),
                    Route = request.Settings.Route,
                    Type = request.Settings.Type,
                    Discount = request.Settings.Discount
                };
            }

            DirectionJson directionJson;
            if (request.Direction != null)
            {
                directionJson = new DirectionJson
                {
                    Index = await _repository.GetNextIndexForDirectionAsync(),
                    InteriorNumber = request.Direction.InteriorNumber,
                    ExteriorNumber = request.Direction.ExteriorNumber,
                    NeighboringStreet = request.Direction.NeighboringStreet,
                    NeighboringStreet2 = request.Direction.NeighboringStreet2,
                    Colony = request.Direction.Colony,
                    City = request.Direction.City,
                    Municipality = request.Direction.Municipality
                };
            }
            else
            {
                directionJson = new DirectionJson
                {
                    Index = await _repository.GetNextIndexForDirectionAsync()
                };
            }

            PaymentMethodsJson? paymentMethodsJson = null;
            if (request.PaymentMethod != null)
            {
                paymentMethodsJson = new PaymentMethodsJson
                {
                    Index = await _repository.GetNextIndexForPaymentMethodsAsync(),
                    Code = request.PaymentMethod.Code,
                    Description = request.PaymentMethod.Description,
                    Bank = request.PaymentMethod.Bank,
                    AccountNumber = request.PaymentMethod.AccountNumber,
                    ReferencePayment = request.PaymentMethod.ReferencePayment,
                    Currency = request.PaymentMethod.Currency
                };
            }

            PaymentTermsJson? paymentTermsJson = null;
            if (request.PaymentTerms != null)
            {
                paymentTermsJson = new PaymentTermsJson
                {
                    Index = await _repository.GetNextIndexForPaymentTermsAsync(),
                    PaymentType = request.PaymentTerms.PaymentType,
                    ExpirationDate = request.PaymentTerms.ExpirationDate,
                    TypeOfCurrency = request.PaymentTerms.TypeOfCurrency
                };
            }

            var entity = new Customer
            {
                ExternalId = request.ExternalId,
                Name = request.Name,
                LastName = request.LastName,
                PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? "+00" : request.PhoneNumber,
                Email = request.Email,
                TaxId = request.TaxId,
                SettingsCustomerJson = settingsJson,
                DirectionJson = directionJson,
                PaymentMethodsJson = paymentMethodsJson,
                PaymentTermsJson = paymentTermsJson
            };

            var created = await _repository.InsertAsync(entity);
            return MapToDto(created);
        }

        public async Task<CustomerDto> EditAsync(CustomerUpdateRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Obtener el cliente con tracking habilitado para poder actualizar
            var existing = await _repository.GetByIdForUpdateAsync(request.IdCustomer);
            if (existing == null)
                throw new KeyNotFoundException($"Customer with Id {request.IdCustomer} not found.");

            var anyFieldProvided = request.ExternalId.HasValue ||
                                   request.Name != null ||
                                   request.LastName != null ||
                                   request.PhoneNumber != null ||
                                   request.Email != null ||
                                   request.TaxId != null ||
                                   request.Settings != null ||
                                   request.Direction != null ||
                                   request.PaymentMethod != null ||
                                   request.PaymentTerms != null;

            if (!anyFieldProvided)
                throw new ArgumentException("At least one field must be provided to update the customer.");

            if (request.ExternalId.HasValue)
                existing.ExternalId = request.ExternalId.Value;

            if (request.Name != null)
                existing.Name = request.Name;

            if (request.LastName != null)
                existing.LastName = request.LastName;

            if (request.PhoneNumber != null)
                existing.PhoneNumber = request.PhoneNumber;

            if (request.Email != null)
                existing.Email = request.Email;

            if (request.TaxId != null)
                existing.TaxId = request.TaxId;

            if (request.Settings != null)
            {
                existing.SettingsCustomerJson = new SettingsCustomerJson
                {
                    Index = await _repository.GetNextIndexForSettingsAsync(),
                    Route = request.Settings.Route,
                    Type = request.Settings.Type,
                    Discount = request.Settings.Discount
                };
            }

            if (request.Direction != null)
            {
                existing.DirectionJson = new DirectionJson
                {
                    Index = await _repository.GetNextIndexForDirectionAsync(),
                    InteriorNumber = request.Direction.InteriorNumber,
                    ExteriorNumber = request.Direction.ExteriorNumber,
                    NeighboringStreet = request.Direction.NeighboringStreet,
                    NeighboringStreet2 = request.Direction.NeighboringStreet2,
                    Colony = request.Direction.Colony,
                    City = request.Direction.City,
                    Municipality = request.Direction.Municipality
                };
            }
            else
            {
                // Si no se proporciona dirección en la actualización, establecer valor por defecto
                existing.DirectionJson = new DirectionJson
                {
                    Index = await _repository.GetNextIndexForDirectionAsync(),
                    InteriorNumber = "SIN DIRECCIÓN",
                    ExteriorNumber = null,
                    NeighboringStreet = null,
                    NeighboringStreet2 = null,
                    Colony = null,
                    City = null,
                    Municipality = null
                };
            }

            if (request.PaymentMethod != null)
            {
                existing.PaymentMethodsJson = new PaymentMethodsJson
                {
                    Index = await _repository.GetNextIndexForPaymentMethodsAsync(),
                    Code = request.PaymentMethod.Code,
                    Description = request.PaymentMethod.Description,
                    Bank = request.PaymentMethod.Bank,
                    AccountNumber = request.PaymentMethod.AccountNumber,
                    ReferencePayment = request.PaymentMethod.ReferencePayment,
                    Currency = request.PaymentMethod.Currency
                };
            }

            if (request.PaymentTerms != null)
            {
                existing.PaymentTermsJson = new PaymentTermsJson
                {
                    Index = await _repository.GetNextIndexForPaymentTermsAsync(),
                    PaymentType = request.PaymentTerms.PaymentType,
                    ExpirationDate = request.PaymentTerms.ExpirationDate,
                    TypeOfCurrency = request.PaymentTerms.TypeOfCurrency
                };
            }

            var updated = await _repository.UpdateAsync(existing);
            return MapToDto(updated);
        }

        public async Task<bool> DeleteAsync(int idCustomer)
        {
            return await _repository.DeleteAsync(idCustomer);
        }

        public async Task<IEnumerable<CustomerDto>> SearchAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return new List<CustomerDto>();

            var customers = await _repository.SearchByFullNameAsync(searchText);
            return customers.Select(MapToDto);
        }

        private static CustomerDto MapToDto(Customer c)
        {
            return new CustomerDto
            {
                IdCustomer = c.IdCustomer,
                ExternalId = c.ExternalId,
                Name = c.Name,
                LastName = c.LastName,
                PhoneNumber = c.PhoneNumber,
                Email = c.Email,
                TaxId = c.TaxId,
                SettingsCustomerJson = c.SettingsCustomerJson,
                DirectionJson = c.DirectionJson,
                PaymentMethodsJson = c.PaymentMethodsJson,
                PaymentTermsJson = c.PaymentTermsJson
            };
        }
    }
}