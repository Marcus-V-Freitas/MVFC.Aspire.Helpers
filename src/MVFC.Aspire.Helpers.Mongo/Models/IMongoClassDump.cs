namespace MVFC.Aspire.Helpers.Mongo.Models;

/// <summary>
/// Define a estrutura básica para um "dump" de uma coleção do MongoDB.
/// Implementações dessa interface devem fornecer informações essenciais sobre
/// o banco de dados, a coleção e a quantidade de documentos envolvidos na operação.
/// </summary>
public interface IMongoClassDump
{
    /// <summary>
    /// Nome do banco de dados MongoDB associado ao dump.
    /// </summary>
    string DatabaseName { get; }

    /// <summary>
    /// Nome da coleção MongoDB associada ao dump.
    /// </summary>
    string CollectionName { get; }

    /// <summary>
    /// Quantidade de documentos incluídos no dump.
    /// </summary>
    int Quantity { get; }
}