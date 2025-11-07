using AVASphere.ApplicationCore.Common.Entities;
using AVASphere.ApplicationCore.Common.Entities.General;

namespace AVASphere.ApplicationCore.Common.Interfaces;

/// <summary>
/// Repositorio para operaciones de persistencia y consulta sobre la entidad <see cref="User"/>.
/// </summary>
/// <remarks>
/// Esta interfaz define las operaciones básicas que deben implementar las capas de acceso a datos
/// para la entidad User. La documentación de cada método explica la semántica esperada; las implementaciones
/// concretas (EF Core, Dapper, MongoDB, etc.) deben respetar estas garantías o documentar cualquier diferencia.
/// </remarks>
public interface IUserRepository
{
    /// <summary>
    /// Busca un usuario usando los datos proporcionados en el objeto <paramref name="user"/>.
    /// </summary>
    /// <remarks>
    /// Comportamiento esperado:
    /// - El método usará los campos no nulos y no vacíos del objeto <paramref name="user"/> como filtros.
    /// - Los filtros se combinan por defecto con AND: sólo se devuelven usuarios que cumplan todos los criterios proporcionados.
    /// - Campos de ejemplo que pueden usarse como filtro: Id, Username, Email, Phone, etc. (dependiendo de los campos de la entidad <see cref="User"/>).
    /// - Las implementaciones pueden definir si las comparaciones de texto son sensibles o no a mayúsculas; si no se especifica, se recomienda usar comparaciones insensibles a mayúsculas para búsquedas por nombre o email.
    /// - Si no se encuentran coincidencias, se debe devolver <c>null</c>.
    /// - Si el objeto <paramref name="user"/> es <c>null</c>, la implementación debe lanzar <see cref="ArgumentNullException"/>.
    /// - Para búsquedas parciales (por ejemplo, búsqueda por prefijo o contains) la implementación debe documentarlo explícitamente; por defecto se espera coincidencia exacta en los campos proporcionados.
    /// </remarks>
    /// <param name="user">Objeto <see cref="User"/> que contiene uno o varios campos usados como criterios de búsqueda.</param>
    /// <returns>
    /// Una tarea que resuelve en la entidad <see cref="User"/> encontrada que cumple los criterios, o <c>null</c> si no existe ninguna.
    /// </returns>
    /// <exception cref="ArgumentNullException">Si <paramref name="user"/> es <c>null</c>.</exception>
    Task<User> SelectUserAsync(User user);

    /// <summary>
    /// Obtiene un usuario por su identificador único.
    /// </summary>
    /// <param name="idUsers">Identificador del usuario.</param>
    /// <returns>Una tarea que resuelve en la entidad <see cref="User"/> o <c>null</c> si no existe.</returns>
    Task<User> SelectByIdAsync(int idUsers);   

    /// <summary>
    /// Crea un nuevo usuario en la persistencia.
    /// </summary>
    /// <param name="user">Entidad <see cref="User"/> a crear. No debe ser <c>null</c>.</param>
    /// <returns>Una tarea que representa la operación asíncrona.</returns>
    Task CreateUsersAsync(User user);

    /// <summary>
    /// Actualiza un usuario existente en la persistencia.
    /// </summary>
    /// <param name="user">Entidad <see cref="User"/> con los datos a actualizar. Debe contener el identificador del usuario.</param>
    /// <returns>Una tarea que representa la operación asíncrona.</returns>
    Task UpdateUsersAsync(User user);
}