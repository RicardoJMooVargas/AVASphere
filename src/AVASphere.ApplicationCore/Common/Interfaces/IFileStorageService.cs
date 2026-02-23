namespace AVASphere.ApplicationCore.Common.Interfaces;

/// <summary>
/// Interfaz para el servicio de almacenamiento de archivos (MinIO/S3)
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Sube un archivo al almacenamiento y devuelve la URL pública
    /// </summary>
    /// <param name="fileStream">Stream del archivo a subir</param>
    /// <param name="fileName">Nombre del archivo con extensión</param>
    /// <param name="folder">Carpeta destino (ej: "products", "users")</param>
    /// <param name="contentType">Tipo MIME del archivo</param>
    /// <param name="fileSize">Tamaño del archivo en bytes</param>
    /// <returns>URL pública del archivo subido</returns>
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string folder, string contentType, long fileSize);

    /// <summary>
    /// Elimina un archivo del almacenamiento
    /// </summary>
    /// <param name="fileUrl">URL completa del archivo a eliminar</param>
    Task<bool> DeleteFileAsync(string fileUrl);

    /// <summary>
    /// Obtiene la URL pública de un archivo
    /// </summary>
    /// <param name="objectName">Nombre del objeto en el bucket</param>
    /// <returns>URL pública del archivo</returns>
    Task<string> GetFileUrlAsync(string objectName);
}
