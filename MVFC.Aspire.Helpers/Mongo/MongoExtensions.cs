namespace MVFC.Aspire.Helpers.Mongo;

/// <summary>
/// Fornece métodos de extensão para facilitar a configuração e integração de um serviço MongoDB (com Replica Set)
/// em aplicações distribuídas, utilizando containers baseados na imagem oficial do MongoDB.
/// </summary>
public static class MongoExtensions {
    private const string DEFAULT_MONGO_IMAGE = "mongo";
    private const string DEFAULT_CONNECTION_STRING = "mongodb://localhost:27017";
    private const int DEFAULT_TIMEOUT_IN_SECONDS = 300;

    /// <summary>
    /// Adiciona um recurso de MongoDB configurado como Replica Set à aplicação distribuída, utilizando um container baseado na imagem oficial do MongoDB.
    /// </summary>
    /// <param name="builder">O construtor da aplicação distribuída (<see cref="IDistributedApplicationBuilder"/>).</param>
    /// <param name="name">Nome do recurso de MongoDB a ser criado.</param>
    /// <param name="tag">(Opcional) Tag da imagem do MongoDB a ser utilizada. Padrão: "latest".</param>
    /// <param name="volumeName">
    /// (Opcional) Nome do volume Docker a ser montado no container do MongoDB, permitindo persistência dos dados.
    /// Se não informado, o volume não será configurado.
    /// </param>
    /// <returns>
    /// Um <see cref="IResourceBuilder{ContainerResource}"/> representando o recurso de MongoDB configurado como Replica Set.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Lançada se o parâmetro <paramref name="name"/> ou <paramref name="tag"/> for nulo, vazio ou composto apenas por espaços em branco.
    /// </exception>
    public static IResourceBuilder<ContainerResource> AddMongoReplicaSet(
        this IDistributedApplicationBuilder builder,
        string name,
        string tag = "latest",
        string? volumeName = null) {

        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        ArgumentException.ThrowIfNullOrWhiteSpace(tag, nameof(tag));

        return builder.AddContainer(name, DEFAULT_MONGO_IMAGE, tag)
                      .WithArgs("--replSet", "rs0", "--bind_ip_all")
                      .AddContainerVolume(volumeName)
                      .WithEndpoint(27017, 27017, "mongodb", isProxied: false, isExternal: true)
                      .WithContainerFiles("/docker-entrypoint-initdb.d", [
                          new ContainerFile
                          {
                              Name = "init-replica-set.js",
                              Contents = GetReplicaSetScript(),
                              Mode = UnixFileMode.UserRead | UnixFileMode.UserWrite |
                                     UnixFileMode.GroupRead | UnixFileMode.OtherRead
                          }
                      ]);
    }

    /// <summary>
    /// Configura o projeto para aguardar a inicialização do recurso de MongoDB e define a variável de ambiente de conexão.
    /// </summary>
    /// <param name="project">O recurso do projeto que irá depender do MongoDB.</param>
    /// <param name="mongoDbResource">O recurso de MongoDB a ser aguardado.</param>
    /// <param name="connectionStringSection">
    /// (Opcional) Nome da variável de ambiente para a string de conexão. Padrão: "ConnectionStrings:mongo".
    /// </param>
    /// <param name="dumps">(Opcional) Lista de configurações de coleções a serem inseridas no MongoDB após a inicialização.</param>
    /// <returns>
    /// O <see cref="IResourceBuilder{ProjectResource}"/> do projeto, configurado para aguardar o MongoDB.
    /// </returns>
    public static IResourceBuilder<ProjectResource> WaitForMongoReplicaSet(
        this IResourceBuilder<ProjectResource> project,
        IResourceBuilder<ContainerResource> mongoDbResource,
        string connectionStringSection = "ConnectionStrings:mongo",
        IList<IMongoClassDump>? dumps = null) {

        project.WaitFor(mongoDbResource)
               .WithEnvironment(connectionStringSection, DEFAULT_CONNECTION_STRING)
               .OnResourceReady(async (_, _, ct) => await MongoDumpAsync(DEFAULT_CONNECTION_STRING, dumps, ct));

        return project;
    }

    /// <summary>
    /// Adiciona e integra o recurso de MongoDB (Replica Set) ao projeto, configurando dependências e conexão.
    /// </summary>
    /// <param name="project">O recurso do projeto que irá utilizar o MongoDB.</param>
    /// <param name="builder">O construtor da aplicação distribuída.</param>
    /// <param name="name">Nome do recurso de MongoDB a ser criado.</param>
    /// <param name="tag">(Opcional) Tag da imagem do MongoDB a ser utilizada. Padrão: "latest".</param>
    /// <param name="volumeName">
    /// (Opcional) Nome do volume Docker a ser montado no container do MongoDB, permitindo persistência dos dados.
    /// Se não informado, o volume não será configurado.
    /// </param>
    /// <param name="connectionStringSection">
    /// (Opcional) Nome da variável de ambiente para a string de conexão. Padrão: "ConnectionStrings:mongo".
    /// </param>
    /// <param name="dumps">(Opcional) Lista de configurações de coleções a serem inseridas no MongoDB após a inicialização.</param>
    /// <returns>
    /// O <see cref="IResourceBuilder{ProjectResource}"/> do projeto, configurado para utilizar o MongoDB.
    /// </returns>
    public static IResourceBuilder<ProjectResource> WithMongoReplicaSet(
        this IResourceBuilder<ProjectResource> project,
        IDistributedApplicationBuilder builder,
        string name,
        string tag = "latest",
        string? volumeName = null,
        string connectionStringSection = "ConnectionStrings:mongo",
        IList<IMongoClassDump>? dumps = null) {

        var mongo = builder.AddMongoReplicaSet(name, tag, volumeName);

        return project.WaitForMongoReplicaSet(mongo, connectionStringSection, dumps);
    }

    /// <summary>
    /// Realiza o dump (inserção em massa) das coleções configuradas no MongoDB.
    /// </summary>
    /// <param name="connectionString">String de conexão com o MongoDB.</param>
    /// <param name="dumps">Coleções e configurações a serem inseridas.</param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>Task representando a operação assíncrona.</returns>
    public static async Task MongoDumpAsync(
        string connectionString,
        IEnumerable<IMongoClassDump>? dumps,
        CancellationToken ct) {

        if (dumps == null)
            return;

        // Configura o timeout do MongoClient para 30 segundos
        var settings = MongoClientSettings.FromConnectionString(connectionString);
        settings.ConnectTimeout = TimeSpan.FromSeconds(DEFAULT_TIMEOUT_IN_SECONDS);
        settings.ServerSelectionTimeout = TimeSpan.FromSeconds(DEFAULT_TIMEOUT_IN_SECONDS);

        var mongo = new MongoClient(settings);

        foreach (var dump in dumps) {
            await ProcessMongoDumpConfig(mongo, dump, ct);
        }
    }

    /// <summary>
    /// Adiciona um volume Docker ao recurso de container do MongoDB, permitindo persistência dos dados.
    /// </summary>
    /// <param name="resource">O recurso de container MongoDB ao qual o volume será adicionado.</param>
    /// <param name="volumeName">
    /// (Opcional) Nome do volume Docker a ser montado no container.
    /// Se não informado ou vazio, nenhum volume será configurado.
    /// </param>
    /// <returns>
    /// O <see cref="IResourceBuilder{ContainerResource}"/> atualizado, com o volume configurado se <paramref name="volumeName"/> for válido.
    /// </returns>
    private static IResourceBuilder<ContainerResource> AddContainerVolume(
        this IResourceBuilder<ContainerResource> resource,
        string? volumeName) {

        if (!string.IsNullOrWhiteSpace(volumeName))
            resource.WithVolume(volumeName, "/data/db");

        return resource;
    }

    /// <summary>
    /// Processa a configuração de dump para uma coleção específica, utilizando reflexão para invocar o método genérico.
    /// </summary>
    /// <param name="mongo">Instância do MongoClient.</param>
    /// <param name="dump">Configuração da coleção a ser processada.</param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>Task representando a operação assíncrona.</returns>
    private static async Task ProcessMongoDumpConfig(MongoClient mongo, IMongoClassDump dump, CancellationToken ct) {
        Exception? error = null;
        try {
            var genericType = GetGenericType(dump);
            var genericMethod = GetDumpCollectionMethod(genericType);

            var taskObj = genericMethod.Invoke(null, [mongo, dump, ct]);
            if (taskObj is Task task)
                await task;
        }
        catch (Exception ex) {
            error = ex;
        }
        finally {
            if (error != null)
                Console.WriteLine($"Erro ao processar configuração Mongo: {error.GetType().Name} - {error.Message}");
        }
    }

    /// <summary>
    /// Obtém o tipo genérico da configuração de dump.
    /// </summary>
    /// <param name="configObj">Configuração da coleção.</param>
    /// <returns>Tipo genérico da coleção.</returns>
    private static Type GetGenericType(IMongoClassDump configObj) {
        var configType = configObj.GetType();
        return configType.GetGenericArguments().FirstOrDefault()
            ?? throw new InvalidOperationException("Tipo genérico não encontrado.");
    }

    /// <summary>
    /// Obtém o método genérico responsável por realizar o dump da coleção.
    /// </summary>
    /// <param name="genericType">Tipo genérico da coleção.</param>
    /// <returns>Instância de MethodInfo do método genérico.</returns>
    private static MethodInfo GetDumpCollectionMethod(Type genericType) {
        var method = typeof(MongoExtensions).GetMethod(nameof(DumpCollectionAsync), BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new MissingMethodException("Método DumpCollectionAsync não encontrado.");

        return method.MakeGenericMethod(genericType)
            ?? throw new InvalidOperationException("Falha ao criar método genérico.");
    }

    /// <summary>
    /// Realiza a inserção em massa dos documentos gerados para a coleção MongoDB.
    /// </summary>
    /// <typeparam name="T">Tipo da entidade da coleção.</typeparam>
    /// <param name="mongo">Instância do MongoClient.</param>
    /// <param name="dump">Configuração da coleção e gerador de dados.</param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>Task representando a operação assíncrona.</returns>
    private static async Task DumpCollectionAsync<T>(MongoClient mongo, MongoClassDump<T> dump, CancellationToken ct)
        where T : class {

        var database = mongo.GetDatabase(dump.DatabaseName);
        var collection = database.GetCollection<T>(dump.CollectionName);

        var items = dump.Faker.Generate(dump.Quantity);

        await collection.InsertManyAsync(items, cancellationToken: ct);
    }

    /// <summary>
    /// Retorna o script de inicialização do Replica Set para o MongoDB.
    /// </summary>
    /// <returns>Script JavaScript para inicializar e verificar o status do Replica Set.</returns>
    private static string GetReplicaSetScript() => """
                        sleep(2000);

                        try {
                            var config = {
                                "_id": "rs0",
                                "members": [{
                                    "_id": 0,
                                    "host": "localhost:27017"
                                }]
                            };

                            var result = rs.initiate(config);

                            print("Replica set initiated: " + tojson(result));

                        } catch (e) {
                            if (e.message.includes("already initialized")) {
                                print("Replica set already initialized.");
                            } else {
                                try {
                                    rs.initiate();
                                    print("Replica set initiated with default configuration.");
                                } catch (ex) {
                                    print("Error initiating replica set: " + ex.message);
                                }
                            }
                        }

                        sleep(1000);

                        try {
                            var result = rs.status();

                            print("Replica set status: " + tojson(result.ok));
                        } catch (e) {
                            print("Error getting replica set status: " + e.message);
                        }
                        """;
}