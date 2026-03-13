using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.ApplicationCore.Common.Interfaces;
using AVASphere.ApplicationCore.Common.Entities.General;
using AVASphere.ApplicationCore.Common.Entities.Jsons;
using ClosedXML.Excel;
using System.Globalization;

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
                    Discount = request.Settings.Discount,
                    Agente = request.Settings.Agente,
                    TipoCliente = request.Settings.TipoCliente,
                    TipoPrecio = request.Settings.TipoPrecio,
                    Credito = request.Settings.Credito,
                    LimiteCredito = request.Settings.LimiteCredito,
                    DiasPP = request.Settings.DiasPP,
                    PorcPP = request.Settings.PorcPP,
                    RF = request.Settings.RF,
                    Descripcion = request.Settings.Descripcion,
                    Alta = request.Settings.Alta
                };
            }

            DirectionJson directionJson;
            if (request.Direction != null)
            {
                directionJson = new DirectionJson
                {
                    Index = await _repository.GetNextIndexForDirectionAsync(),
                    Street = request.Direction.Street,
                    InteriorNumber = request.Direction.InteriorNumber,
                    ExteriorNumber = request.Direction.ExteriorNumber,
                    NeighboringStreet = request.Direction.NeighboringStreet,
                    NeighboringStreet2 = request.Direction.NeighboringStreet2,
                    Colony = request.Direction.Colony,
                    City = request.Direction.City,
                    Municipality = request.Direction.Municipality,
                    PostalCode = request.Direction.PostalCode
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
                    Discount = request.Settings.Discount,
                    Agente = request.Settings.Agente,
                    TipoCliente = request.Settings.TipoCliente,
                    TipoPrecio = request.Settings.TipoPrecio,
                    Credito = request.Settings.Credito,
                    LimiteCredito = request.Settings.LimiteCredito,
                    DiasPP = request.Settings.DiasPP,
                    PorcPP = request.Settings.PorcPP,
                    RF = request.Settings.RF,
                    Descripcion = request.Settings.Descripcion,
                    Alta = request.Settings.Alta
                };
            }

            if (request.Direction != null)
            {
                existing.DirectionJson = new DirectionJson
                {
                    Index = await _repository.GetNextIndexForDirectionAsync(),
                    Street = request.Direction.Street,
                    InteriorNumber = request.Direction.InteriorNumber,
                    ExteriorNumber = request.Direction.ExteriorNumber,
                    NeighboringStreet = request.Direction.NeighboringStreet,
                    NeighboringStreet2 = request.Direction.NeighboringStreet2,
                    Colony = request.Direction.Colony,
                    City = request.Direction.City,
                    Municipality = request.Direction.Municipality,
                    PostalCode = request.Direction.PostalCode
                };
            }
            else
            {
                // Si no se proporciona dirección en la actualización, establecer valor por defecto
                existing.DirectionJson = new DirectionJson
                {
                    Index = await _repository.GetNextIndexForDirectionAsync(),
                    Street = null,
                    InteriorNumber = "SIN DIRECCIÓN",
                    ExteriorNumber = null,
                    NeighboringStreet = null,
                    NeighboringStreet2 = null,
                    Colony = null,
                    City = null,
                    Municipality = null,
                    PostalCode = null
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

        public async Task<CustomerImportResultDto> ImportFromExcelAsync(Stream excelFileStream)
        {
            var result = new CustomerImportResultDto();
            var importedCustomers = new List<CustomerDto>();

            try
            {
                using var workbook = new XLWorkbook(excelFileStream);
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RowsUsed().Skip(1).ToList(); // Materializar las filas

                result.TotalRows = rows.Count;

                // OPTIMIZACIÓN 1: Pre-cargar todos los ExternalIds existentes en una sola consulta
                var allExternalIds = rows
                    .Select(r => ParseInt(r.Cell(1).GetString()) ?? 0)
                    .Where(id => id > 0)
                    .Distinct()
                    .ToList();

                var existingIds = await _repository.GetExistingExternalIdsAsync(allExternalIds);
                var existingIdsSet = new HashSet<int>(existingIds);

                // HashSet para detectar duplicados dentro del mismo archivo
                var processedIdsInBatch = new HashSet<int>();

                // OPTIMIZACIÓN 2: Pre-calcular los índices base para evitar múltiples llamadas a BD
                var baseIndexDirection = await _repository.GetNextIndexForDirectionAsync();
                var baseIndexSettings = await _repository.GetNextIndexForSettingsAsync();

                // Lista para acumular clientes válidos y hacer inserción batch
                var customersToInsert = new List<Customer>();
                var currentDirectionIndex = baseIndexDirection;
                var currentSettingsIndex = baseIndexSettings;

                // OPTIMIZACIÓN 3: Procesar todas las filas sin await en el loop
                foreach (var row in rows)
                {
                    try
                    {
                        // Mapeo de columnas según el Excel proporcionado
                        // id, nombre, rfc, agente, nombreagt, calle, numero, interior, entre, colonia, 
                        // poblacion, estado, pais, cp, telefono1, email, tipo, tipocliente, tipoprecio, 
                        // credito, limitecredito, diaspp, porcpp, rf, descripcion, alta

                        var externalId = ParseInt(row.Cell(1).GetString()) ?? 0;

                        // Verificar si el cliente ya existe en BD usando el HashSet pre-cargado
                        if (existingIdsSet.Contains(externalId))
                        {
                            result.SkippedCount++;
                            result.SkippedRecords.Add($"ID {externalId} (Fila {row.RowNumber()}) - Ya existe en BD");
                            continue; // Saltar este registro, ya existe
                        }

                        // Verificar si el ExternalId ya fue procesado en este batch (duplicado en el archivo)
                        if (processedIdsInBatch.Contains(externalId))
                        {
                            result.SkippedCount++;
                            result.SkippedRecords.Add($"ID {externalId} (Fila {row.RowNumber()}) - Duplicado en el archivo");
                            continue; // Saltar este registro, es duplicado
                        }

                        // Marcar este ExternalId como procesado
                        processedIdsInBatch.Add(externalId);

                        var nombre = TruncateString(row.Cell(2).GetString(), 100); // Truncar a 100 caracteres
                        var rfc = TruncateString(row.Cell(3).GetString(), 50); // Truncar a 50 caracteres
                        var email = TruncateString(row.Cell(16).GetString(), 255); // Truncar a 255 caracteres
                        var telefono1 = TruncateString(row.Cell(15).GetString(), 20); // Truncar a 20 caracteres

                        // Datos de dirección
                        var directionData = new CustomerDirectionExcelDto
                        {
                            Calle = row.Cell(6).GetString(),
                            Numero = row.Cell(7).GetString(),
                            Interior = row.Cell(8).GetString(),
                            Entre = row.Cell(9).GetString(),
                            Colonia = row.Cell(10).GetString(),
                            Poblacion = row.Cell(11).GetString(),
                            Estado = row.Cell(12).GetString(),
                            Pais = row.Cell(13).GetString(),
                            CP = row.Cell(14).GetString()
                        };

                        // Datos de configuración
                        var settingsData = new CustomerSettingsExcelDto
                        {
                            Agente = row.Cell(4).GetString(),
                            Tipo = row.Cell(17).GetString(),
                            TipoCliente = row.Cell(18).GetString(),
                            TipoPrecio = row.Cell(19).GetString(),
                            Credito = row.Cell(20).GetString(),
                            LimiteCredito = ParseDecimal(row.Cell(21).GetString()),
                            DiasPP = ParseInt(row.Cell(22).GetString()),
                            PorcPP = ParseDecimal(row.Cell(23).GetString()),
                            RF = row.Cell(24).GetString(),
                            Descripcion = row.Cell(25).GetString(),
                            Alta = ParseDateTime(row.Cell(26).GetString())
                        };

                        // Crear DirectionJson con índice auto-incrementado localmente
                        var directionJson = new DirectionJson
                        {
                            Index = currentDirectionIndex++,
                            Street = directionData.Calle,
                            InteriorNumber = directionData.Interior,
                            ExteriorNumber = directionData.Numero,
                            NeighboringStreet = directionData.Entre,
                            Colony = directionData.Colonia,
                            City = directionData.Poblacion,
                            Municipality = directionData.Estado,
                            PostalCode = directionData.CP
                        };

                        // Crear SettingsCustomerJson con índice auto-incrementado localmente
                        var settingsJson = new SettingsCustomerJson
                        {
                            Index = currentSettingsIndex++,
                            Type = settingsData.Tipo ?? "General",
                            Discount = 0.0,
                            Agente = settingsData.Agente,
                            TipoCliente = settingsData.TipoCliente,
                            TipoPrecio = settingsData.TipoPrecio,
                            Credito = settingsData.Credito,
                            LimiteCredito = settingsData.LimiteCredito,
                            DiasPP = settingsData.DiasPP,
                            PorcPP = settingsData.PorcPP,
                            RF = settingsData.RF,
                            Descripcion = settingsData.Descripcion,
                            Alta = settingsData.Alta
                        };

                        // Crear la entidad Customer
                        var customer = new Customer
                        {
                            ExternalId = externalId,
                            Name = nombre,
                            LastName = "", // No viene en el Excel
                            PhoneNumber = string.IsNullOrWhiteSpace(telefono1) ? "+00" : telefono1,
                            Email = email,
                            TaxId = rfc,
                            SettingsCustomerJson = settingsJson,
                            DirectionJson = directionJson,
                            PaymentMethodsJson = null,
                            PaymentTermsJson = null
                        };

                        customersToInsert.Add(customer);
                    }
                    catch (Exception ex)
                    {
                        result.ErrorCount++;
                        var errorMsg = $"Fila {row.RowNumber()} (ID: {row.Cell(1).GetString()}): {ex.InnerException?.Message ?? ex.Message}";
                        result.Errors.Add(errorMsg);
                    }
                }

                // OPTIMIZACIÓN 4: Inserción batch de todos los clientes válidos
                if (customersToInsert.Any())
                {
                    try
                    {
                        var insertedCustomers = await _repository.InsertBatchAsync(customersToInsert);
                        result.SuccessCount = insertedCustomers.Count();

                        // No devolver clientes en la respuesta para optimizar rendimiento
                    }
                    catch (Exception batchEx)
                    {
                        // Mostrar error detallado de la inserción batch
                        var errorMsg = $"Error en inserción batch: {batchEx.Message}";
                        if (batchEx.InnerException != null)
                        {
                            errorMsg += $" | Inner: {batchEx.InnerException.Message}";
                            if (batchEx.InnerException.InnerException != null)
                            {
                                errorMsg += $" | Inner2: {batchEx.InnerException.InnerException.Message}";
                            }
                        }
                        result.Errors.Add(errorMsg);
                        result.ErrorCount = customersToInsert.Count;
                    }
                }

                result.ImportedCustomers = importedCustomers;
            }
            catch (Exception ex)
            {
                var errorMsg = $"Error general: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMsg += $" | Inner: {ex.InnerException.Message}";
                    if (ex.InnerException.InnerException != null)
                    {
                        errorMsg += $" | Inner2: {ex.InnerException.InnerException.Message}";
                    }
                }
                result.Errors.Add(errorMsg);
            }

            return result;
        }

        // Métodos auxiliares para parsear valores
        private static int? ParseInt(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            // Remover comas y espacios
            value = value.Replace(",", "").Trim();

            if (int.TryParse(value, out int result))
                return result;

            return null;
        }

        private static decimal? ParseDecimal(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            // Remover comas y símbolos de moneda
            value = value.Replace(",", "").Replace("$", "").Trim();

            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                return result;

            return null;
        }

        private static DateTime? ParseDateTime(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            // Intentar varios formatos
            string[] formats = { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "MM/dd/yyyy" };

            if (DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                return result;

            if (DateTime.TryParse(value, out result))
                return result;

            return null;
        }

        private static string? TruncateString(string? value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            value = value.Trim();

            if (value.Length <= maxLength)
                return value;

            return value.Substring(0, maxLength);
        }

        public async Task<bool> ResetTableAsync()
        {
            return await _repository.ResetTableAsync();
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