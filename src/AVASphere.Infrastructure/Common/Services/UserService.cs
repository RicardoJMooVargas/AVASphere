    using System.Linq.Expressions;
    using Microsoft.Extensions.Logging;
    using AVASphere.ApplicationCore.Common.Entities;
    using AVASphere.ApplicationCore.Common.Interfaces;
    using AVASphere.ApplicationCore.Common.DTOs;
    using AVASphere.ApplicationCore.Common.Entities.General;
    using AVASphere.ApplicationCore.Common.Enums;
    using AVASphere.ApplicationCore.Common.Extensions;

    namespace AVASphere.Infrastructure.Common.Services;

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEncryptionService _encryptionService;
        private readonly IConfigSysService _configSysService; 
        private readonly IRolRepository _rolRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository userRepository, 
            IEncryptionService encryptionService,
            IConfigSysService configSysService,
            IRolRepository rolRepository,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
            _configSysService = configSysService ?? throw new ArgumentNullException(nameof(configSysService));
            _rolRepository = rolRepository ?? throw new ArgumentNullException(nameof(rolRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<UserResponse>> SearchUsersAsync(int? idUsers = null, string? userName = null)
        {
            try
            {
                if (idUsers.HasValue && !string.IsNullOrWhiteSpace(userName))
                    throw new ArgumentException("Solo se puede buscar por ID o por UserName, no ambos");

                _logger.LogInformation("Iniciando búsqueda de usuario (ID={IdUser}, UserName={UserName})", idUsers, userName);

                if (!idUsers.HasValue && string.IsNullOrWhiteSpace(userName))
                {
                    // Obtener todos los usuarios
                    var users = await _userRepository.GetAllAsync();
                    _logger.LogInformation("Obtenidos {Count} usuarios", users.Count());
                    return users.Select(u => u.ToResponse());
                }

                var userCriteria = new User();

                if (idUsers.HasValue)
                {
                    userCriteria.IdUser = idUsers.Value;
                }
                else
                {
                    userCriteria.UserName = userName!.Trim();
                }

                var user = await _userRepository.SelectUserAsync(userCriteria);

                if (user == null)
                {
                    string criterio = idUsers.HasValue ? $"ID {idUsers}" : $"UserName '{userName}'";
                    _logger.LogWarning("Usuario con {Criterio} no encontrado", criterio);
                    throw new KeyNotFoundException($"Usuario con {criterio} no encontrado");
                }

                _logger.LogInformation("Usuario encontrado correctamente (ID={IdUser}, UserName={UserName})", user.IdUser, user.UserName);
                return new List<UserResponse> { user.ToResponse() };
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar usuario (ID={IdUser}, UserName={UserName})", idUsers, userName);
                throw new ApplicationException($"Error al buscar el usuario: {ex.Message}", ex);
            }
        }

        public async Task<UserResponse> NewUsersAsync(UserCreateRequest request)
        {
            try
            {
                _logger.LogInformation("Creando nuevo usuario: {UserName}", request.UserName);

                // Validaciones básicas
                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (string.IsNullOrWhiteSpace(request.UserName))
                    throw new ArgumentException("El nombre de usuario es requerido");

                if (string.IsNullOrWhiteSpace(request.Password))
                    throw new ArgumentException("La contraseña es requerida");

                // ✅ VALIDAR QUE CONFIG SYS EXISTA
                var configExists = await _configSysService.GetConfigAsync();
                if (configExists == null)
                {
                    _logger.LogWarning("No existe configuración del sistema. Creando configuración por defecto...");
                    
                    // Crear configuración por defecto
                    var defaultConfig = new ConfigSys
                    {
                        CompanyName = "Mi Empresa",
                        BranchName = "Sucursal Principal",
                        LogoUrl = "",
                        Colors = new List<ColorsJson>(),
                        NotUseModules = new List<NotUseModuleJson>(),
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    configExists = await _configSysService.SaveConfigAsync(defaultConfig);
                    _logger.LogInformation("Configuración por defecto creada con ID: {IdConfigSys}", configExists.IdConfigSys);
                }

                // Validar que el UserName no esté duplicado
                var existingUser = await _userRepository.SelectUserAsync(new User { UserName = request.UserName });
                if (existingUser != null)
                {
                    _logger.LogWarning("Intento de crear usuario con nombre duplicado: {UserName}", request.UserName);
                    throw new InvalidOperationException($"El nombre de usuario '{request.UserName}' ya está en uso");
                }

                // Mapear request a entidad
                var user = request.ToEntity();

                // ✅ ASIGNAR ID CONFIG SYS (usar el existente o el proporcionado en el request)
                user.IdConfigSys = configExists.IdConfigSys;

                // Encriptar la contraseña
                user.HashPassword = _encryptionService.HashPassword(request.Password);

                // Establecer valores por defecto
                user.Status = !string.IsNullOrWhiteSpace(user.Status) ? user.Status : "Active";
                user.Verified = false;
                user.CreateAt = null;

                await _userRepository.CreateUsersAsync(user);
                _logger.LogInformation("Usuario creado exitosamente con ID: {IdUser}", user.IdUser);

                return user.ToResponse();
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear nuevo usuario: {UserName}", request.UserName);
                throw new ApplicationException($"Error al crear el usuario: {ex.Message}", ex);
            }
        }

        public async Task<UserResponse> EditUsersAsync(UserUpdateRequest request)
        {
            try
            {
                _logger.LogInformation("Actualizando usuario con ID: {IdUser}", request.IdUsers);

                // Verificar que el usuario existe
                var existingUser = await _userRepository.SelectByIdAsync(request.IdUsers);
                if (existingUser == null)
                {
                    _logger.LogWarning("Intento de actualizar usuario inexistente con ID: {IdUser}", request.IdUsers);
                    throw new KeyNotFoundException($"Usuario con ID {request.IdUsers} no encontrado");
                }

                // Validar que el UserName no esté duplicado (si se está cambiando)
                if (!string.IsNullOrEmpty(request.UserName) && request.UserName != existingUser.UserName)
                {
                    var userWithSameName = await _userRepository.SelectUserAsync(new User { UserName = request.UserName });
                    if (userWithSameName != null && userWithSameName.IdUser != request.IdUsers)
                    {
                        _logger.LogWarning("Intento de cambiar a nombre de usuario duplicado: {UserName}", request.UserName);
                        throw new InvalidOperationException($"El nombre de usuario '{request.UserName}' ya está en uso");
                    }
                }

                // ✅ VALIDAR CONFIG SYS SI SE PROPORCIONA
                if (request.IdConfigSys.HasValue && request.IdConfigSys.Value > 0)
                {
                    var configExists = await _configSysService.GetConfigAsync();
                    if (configExists == null || configExists.IdConfigSys != request.IdConfigSys.Value)
                    {
                        throw new ArgumentException($"La configuración del sistema con ID {request.IdConfigSys.Value} no existe");
                    }
                }

                // Mapear request a entidad (preservando datos existentes)
                var user = request.ToEntity(existingUser);

                // ✅ PRESERVAR ID CONFIG SYS SI NO SE PROPORCIONA UNO NUEVO
                if (!request.IdConfigSys.HasValue || request.IdConfigSys.Value <= 0)
                {
                    user.IdConfigSys = existingUser.IdConfigSys;
                }

                // Si se proporciona una nueva contraseña, encriptarla
                if (!string.IsNullOrEmpty(request.Password))
                {
                    user.HashPassword = _encryptionService.HashPassword(request.Password);
                }
                else
                {
                    user.HashPassword = existingUser.HashPassword;
                }

                await _userRepository.UpdateUsersAsync(user);
                _logger.LogInformation("Usuario con ID {IdUser} actualizado exitosamente", user.IdUser);

                return user.ToResponse();
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario con ID: {IdUser}", request.IdUsers);
                throw new ApplicationException($"Error al actualizar el usuario: {ex.Message}", ex);
            }
        }
        public async Task<LoginResponse> AuthenticateUserAsync(LoginDTOs loginDtOs)
        {
            try
            {
                _logger.LogInformation("Intentando autenticar usuario: {UserName}", loginDtOs.UserName);

                if (loginDtOs == null)
                    throw new ArgumentNullException(nameof(loginDtOs));

                if (string.IsNullOrWhiteSpace(loginDtOs.UserName))
                    throw new ArgumentException("El nombre de usuario es requerido");

                if (string.IsNullOrWhiteSpace(loginDtOs.Password))
                    throw new ArgumentException("La contraseña es requerida");

                var user = await _userRepository.SelectUserAsync(new User { UserName = loginDtOs.UserName });
                
                if (user == null)
                {
                    _logger.LogWarning("Intento de login con usuario inexistente: {UserName}", loginDtOs.UserName);
                    return LoginResponse.FailureResponse("Credenciales inválidas");
                }

                _logger.LogDebug("Usuario encontrado: ID={IdUser}, UserName={UserName}, Status={Status}, HashLength={HashLength}", 
                    user.IdUser, user.UserName, user.Status, user.HashPassword?.Length ?? 0);

                if (!string.Equals(user.Status, "Active", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Intento de login con usuario inactivo: {UserName}, Status={Status}", 
                        loginDtOs.UserName, user.Status);
                    return LoginResponse.FailureResponse("La cuenta de usuario está deshabilitada");
                }

                if (string.IsNullOrEmpty(user.HashPassword))
                {
                    _logger.LogError("Usuario {UserName} no tiene contraseña configurada", loginDtOs.UserName);
                    return LoginResponse.FailureResponse("Credenciales inválidas");
                }

                bool passwordValid = _encryptionService.VerifyPassword(loginDtOs.Password, user.HashPassword);
                if (!passwordValid)
                {
                    _logger.LogWarning("Contraseña incorrecta para usuario: {UserName}", loginDtOs.UserName);
                    return LoginResponse.FailureResponse("Credenciales inválidas");
                }

                string token = string.Empty;
                
                // ✅ OBTENER CONFIG SYS DEL USUARIO
                ConfigSysResponseDto? configSysResponse = null;
                
                try
                {
                    if (user.ConfigSys != null)
                    {
                        _logger.LogDebug("Procesando ConfigSys del usuario. IdConfigSys={IdConfigSys}", user.ConfigSys.IdConfigSys);
                        
                        configSysResponse = new ConfigSysResponseDto
                        {
                            IdConfigSys = user.ConfigSys.IdConfigSys,
                            CompanyName = user.ConfigSys.CompanyName ?? "",
                            BranchName = user.ConfigSys.BranchName ?? "",
                            LogoUrl = user.ConfigSys.LogoUrl ?? "",
                            Colors = user.ConfigSys.Colors?.Select(c => new ColorResponseDto
                            {
                                Index = c.Index,
                                NameColor = c.NameColor ?? "",
                                ColorCode = c.ColorCode ?? "",
                                ColorRgb = c.ColorRgb ?? ""
                            }).ToList() ?? new List<ColorResponseDto>(),
                            NotUseModules = user.ConfigSys.NotUseModules?.Select(m => new NotUseModuleResponseDto
                            {
                                Index = m.Index,
                                NameModule = m.NameModule ?? ""
                            }).ToList() ?? new List<NotUseModuleResponseDto>(),
                            CreatedAt = user.ConfigSys.CreatedAt
                        };
                        
                        _logger.LogDebug("ConfigSys procesado correctamente");
                    }
                    else
                    {
                        _logger.LogWarning("Usuario {UserName} no tiene configuración del sistema asociada. Buscando configuración por defecto...", user.UserName);
                        
                        // ✅ OBTENER CONFIGURACIÓN POR DEFECTO SI EL USUARIO NO TIENE
                        var defaultConfig = await _configSysService.GetConfigAsync();
                        if (defaultConfig != null)
                        {
                            _logger.LogDebug("Configuración por defecto encontrada. IdConfigSys={IdConfigSys}", defaultConfig.IdConfigSys);
                            
                            configSysResponse = new ConfigSysResponseDto
                            {
                                IdConfigSys = defaultConfig.IdConfigSys,
                                CompanyName = defaultConfig.CompanyName ?? "",
                                BranchName = defaultConfig.BranchName ?? "",
                                LogoUrl = defaultConfig.LogoUrl ?? "",
                                Colors = defaultConfig.Colors?.Select(c => new ColorResponseDto
                                {
                                    Index = c.Index,
                                    NameColor = c.NameColor ?? "",
                                    ColorCode = c.ColorCode ?? "",
                                    ColorRgb = c.ColorRgb ?? ""
                                }).ToList() ?? new List<ColorResponseDto>(),
                                NotUseModules = defaultConfig.NotUseModules?.Select(m => new NotUseModuleResponseDto
                                {
                                    Index = m.Index,
                                    NameModule = m.NameModule ?? ""
                                }).ToList() ?? new List<NotUseModuleResponseDto>(),
                                CreatedAt = defaultConfig.CreatedAt
                            };
                        }
                        else
                        {
                            _logger.LogWarning("No se encontró configuración por defecto. Usando configuración vacía.");
                            
                            // Crear configuración vacía para evitar null
                            configSysResponse = new ConfigSysResponseDto
                            {
                                IdConfigSys = 0,
                                CompanyName = "Sin configuración",
                                BranchName = "Sin configuración",
                                LogoUrl = "",
                                Colors = new List<ColorResponseDto>(),
                                NotUseModules = new List<NotUseModuleResponseDto>(),
                                CreatedAt = DateTime.UtcNow
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al procesar ConfigSys para usuario: {UserName}", loginDtOs.UserName);
                    
                    // Crear configuración vacía en caso de error
                    configSysResponse = new ConfigSysResponseDto
                    {
                        IdConfigSys = 0,
                        CompanyName = "Error al cargar configuración",
                        BranchName = "Error al cargar configuración",
                        LogoUrl = "",
                        Colors = new List<ColorResponseDto>(),
                        NotUseModules = new List<NotUseModuleResponseDto>(),
                        CreatedAt = DateTime.UtcNow
                    };
                }
                
                _logger.LogInformation("Autenticación exitosa para usuario: {UserName}", loginDtOs.UserName);
                
                return LoginResponse.SuccessResponse(token, user.ToResponse(), configSysResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la autenticación del usuario: {UserName}", loginDtOs.UserName);
                return LoginResponse.FailureResponse("Error interno durante la autenticación");
            }
        }

        public async Task<UserResponse> SetupAdminUserAsync(string userName, string password)
        {
            try
            {
                _logger.LogInformation("Configurando usuario administrador: {UserName}", userName);

                // Validar parámetros
                if (string.IsNullOrWhiteSpace(userName))
                    throw new ArgumentException("El nombre de usuario es requerido");

                if (string.IsNullOrWhiteSpace(password))
                    throw new ArgumentException("La contraseña es requerida");

                // Obtener configuración del sistema
                var config = await _configSysService.GetConfigAsync();
                if (config == null)
                    throw new InvalidOperationException("La configuración del sistema debe estar configurada primero");

                // Verificar si el rol administrador ya existe
                var adminRol = await _rolRepository.GetByNameAsync("Administrador");
                if (adminRol == null)
                {
                    // Crear el rol administrador
                    adminRol = new Rol
                    {
                        Name = "Administrador",
                        NormalizedName = "admin",
                        Permissions = new List<Permission> { new Permission { Index = 0, Name = "All", Normalized = "all", Type = "All", Status = "Active" } },
                        Modules = Enum.GetValues<SystemModule>().Select((m, i) => new Module { Index = i, Name = m.ToString() }).ToList(),
                        IdArea = 1 // Asumir área 1
                    };
                    await _rolRepository.CreateAsync(adminRol);
                    _logger.LogInformation("Rol administrador creado con ID: {IdRol}", adminRol.IdRol);
                }

                // Verificar si el usuario ya existe
                var existingUser = await _userRepository.SelectUserAsync(new User { UserName = userName });
                if (existingUser != null)
                    throw new InvalidOperationException($"El usuario '{userName}' ya existe");

                // Crear el usuario administrador
                var user = new User
                {
                    UserName = userName,
                    Name = userName,
                    LastName = "",
                    HashPassword = _encryptionService.HashPassword(password),
                    Status = "Active",
                    Aux = "",
                    CreateAt = null,// el repositorio lo pone si ingresas null
                    Verified = true,
                    IdConfigSys = config.IdConfigSys,
                    IdRol = adminRol.IdRol
                };

                await _userRepository.CreateUsersAsync(user);
                _logger.LogInformation("Usuario administrador creado exitosamente con ID: {IdUser}", user.IdUser);

                return user.ToResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al configurar usuario administrador: {UserName}", userName);
                throw new ApplicationException($"Error al configurar el usuario administrador: {ex.Message}", ex);
            }
        }
    }

