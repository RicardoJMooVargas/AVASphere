using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;
using AVASphere.ApplicationCore.Common.Interfaces;

namespace AVASphere.Infrastructure.Common.Services;

/// <summary>
/// Servicio para gestionar el almacenamiento de archivos en MinIO/S3
/// </summary>
public class MinioFileStorageService : IFileStorageService
{
    private readonly IMinioClient _minioClient;
    private readonly string _bucketName;
    private readonly string _endpoint;
    private readonly bool _useSSL;

    public MinioFileStorageService(IConfiguration configuration)
    {
        var endpoint = configuration["MinIO:Endpoint"] ?? throw new ArgumentNullException("MinIO:Endpoint");
        var accessKey = configuration["MinIO:AccessKey"] ?? throw new ArgumentNullException("MinIO:AccessKey");
        var secretKey = configuration["MinIO:SecretKey"] ?? throw new ArgumentNullException("MinIO:SecretKey");
        _bucketName = configuration["MinIO:BucketName"] ?? "avasphere-products";
        _useSSL = bool.Parse(configuration["MinIO:UseSSL"] ?? "true");
        _endpoint = endpoint;

        // Configurar el cliente de MinIO
        _minioClient = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey)
            .WithSSL(_useSSL)
            .Build();
    }

    /// <summary>
    /// Sube un archivo al bucket de MinIO
    /// </summary>
    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string folder, string contentType, long fileSize)
    {
        if (fileStream == null || fileSize == 0)
            throw new ArgumentException("El archivo está vacío o es nulo.");

        // Validar tipo de archivo (solo imágenes)
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(fileExtension))
            throw new ArgumentException($"Tipo de archivo no permitido. Solo se permiten: {string.Join(", ", allowedExtensions)}");

        // Asegurar que el bucket existe
        await EnsureBucketExistsAsync();

        // Construir el nombre del objeto
        var objectName = $"{folder}/{fileName}";

        // Subir el archivo
        var putObjectArgs = new PutObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(objectName)
            .WithStreamData(fileStream)
            .WithObjectSize(fileSize)
            .WithContentType(contentType);

        await _minioClient.PutObjectAsync(putObjectArgs);

        // Retornar la URL pública del archivo
        return await GetFileUrlAsync(objectName);
    }

    /// <summary>
    /// Elimina un archivo del bucket de MinIO
    /// </summary>
    public async Task<bool> DeleteFileAsync(string fileUrl)
    {
        try
        {
            // Extraer el nombre del objeto de la URL
            var uri = new Uri(fileUrl);
            var objectName = uri.AbsolutePath.TrimStart('/');

            // Remover el nombre del bucket si está en la ruta
            if (objectName.StartsWith($"{_bucketName}/"))
                objectName = objectName.Substring(_bucketName.Length + 1);

            var removeObjectArgs = new RemoveObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName);

            await _minioClient.RemoveObjectAsync(removeObjectArgs);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Obtiene la URL pública de un archivo
    /// </summary>
    public async Task<string> GetFileUrlAsync(string objectName)
    {
        // Generar URL pública permanente
        var protocol = _useSSL ? "https" : "http";
        return $"{protocol}://{_endpoint}/{_bucketName}/{objectName}";
    }

    /// <summary>
    /// Asegura que el bucket existe, si no lo crea
    /// </summary>
    private async Task EnsureBucketExistsAsync()
    {
        var bucketExistsArgs = new BucketExistsArgs()
            .WithBucket(_bucketName);

        bool found = await _minioClient.BucketExistsAsync(bucketExistsArgs);

        if (!found)
        {
            var makeBucketArgs = new MakeBucketArgs()
                .WithBucket(_bucketName);

            await _minioClient.MakeBucketAsync(makeBucketArgs);

            // Configurar política de acceso público para lectura
            var policy = $@"{{
                ""Version"": ""2012-10-17"",
                ""Statement"": [
                    {{
                        ""Effect"": ""Allow"",
                        ""Principal"": {{""AWS"": [""*""]}},
                        ""Action"": [""s3:GetObject""],
                        ""Resource"": [""arn:aws:s3:::{_bucketName}/*""]
                    }}
                ]
            }}";

            var setPolicyArgs = new SetPolicyArgs()
                .WithBucket(_bucketName)
                .WithPolicy(policy);

            await _minioClient.SetPolicyAsync(setPolicyArgs);
        }
    }
}
