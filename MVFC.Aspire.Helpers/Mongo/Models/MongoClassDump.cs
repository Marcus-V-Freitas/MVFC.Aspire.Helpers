namespace MVFC.Aspire.Helpers.Mongo.Models;

/// <summary>
/// Representa um "dump" de uma coleção do MongoDB, contendo informações sobre o banco de dados,
/// a coleção, a quantidade de documentos e um gerador de dados fictícios para os documentos.
/// </summary>
/// <typeparam name="T">
/// Tipo dos documentos que serão gerados e incluídos no dump. Deve ser uma classe.
/// </typeparam>
/// <param name="DatabaseName">Nome do banco de dados MongoDB associado ao dump.</param>
/// <param name="CollectionName">Nome da coleção MongoDB associada ao dump.</param>
/// <param name="Quantity">Quantidade de documentos a serem gerados no dump.</param>
/// <param name="Faker">Instância do gerador de dados fictícios (<see cref="Faker{T}"/>) para criar os documentos.</param>
public record class MongoClassDump<T>(
    string DatabaseName,
    string CollectionName,
    int Quantity,
    Faker<T> Faker) : IMongoClassDump where T : class;