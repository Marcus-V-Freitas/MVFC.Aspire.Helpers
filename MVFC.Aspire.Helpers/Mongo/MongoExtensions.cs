namespace MVFC.Aspire.Helpers.Mongo;

/// <summary>
/// Fornece métodos de extensão para facilitar a configuração e integração de um serviço MongoDB (com Replica Set)
/// em aplicações distribuídas, utilizando containers baseados na imagem oficial do MongoDB.
/// </summary>
public static class MongoExtensions
{
    private const string DEFAULT_MONGO_IMAGE = "mongo";

    /// <summary>
    /// Adiciona um recurso de MongoDB configurado como Replica Set à aplicação distribuída, utilizando um container baseado na imagem oficial do MongoDB.
    /// </summary>
    /// <param name="builder">O construtor da aplicação distribuída (<see cref="IDistributedApplicationBuilder"/>).</param>
    /// <param name="name">Nome do recurso de MongoDB a ser criado.</param>
    /// <param name="tag">(Opcional) Tag da imagem do MongoDB a ser utilizada. Padrão: "latest".</param>
    /// <returns>
    /// Um <see cref="IResourceBuilder{ContainerResource}"/> representando o recurso de MongoDB configurado como Replica Set.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Lançada se o parâmetro <paramref name="name"/> ou <paramref name="tag"/> for nulo, vazio ou composto apenas por espaços em branco.
    /// </exception>
    public static IResourceBuilder<ContainerResource> AddMongoReplicaSet(this IDistributedApplicationBuilder builder, string name, string tag = "latest")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        ArgumentException.ThrowIfNullOrWhiteSpace(tag, nameof(tag));

        return builder.AddContainer(name, DEFAULT_MONGO_IMAGE, tag)
                      .WithArgs("--replSet", "rs0", "--bind_ip_all")
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
    /// <returns>
    /// O <see cref="IResourceBuilder{ProjectResource}"/> do projeto, configurado para aguardar o MongoDB.
    /// </returns>
    public static IResourceBuilder<ProjectResource> WaitForMongoReplicaSet(this IResourceBuilder<ProjectResource> project, IResourceBuilder<ContainerResource> mongoDbResource, string connectionStringSection = "ConnectionStrings:mongo")
    {
        project.WaitFor(mongoDbResource)
               .WithEnvironment(connectionStringSection, "mongodb://localhost:27017");

        return project;
    }

    /// <summary>
    /// Adiciona e integra o recurso de MongoDB (Replica Set) ao projeto, configurando dependências e conexão.
    /// </summary>
    /// <param name="project">O recurso do projeto que irá utilizar o MongoDB.</param>
    /// <param name="builder">O construtor da aplicação distribuída.</param>
    /// <param name="name">Nome do recurso de MongoDB a ser criado.</param>
    /// <param name="tag">(Opcional) Tag da imagem do MongoDB a ser utilizada. Padrão: "latest".</param>
    /// <returns>
    /// O <see cref="IResourceBuilder{ProjectResource}"/> do projeto, configurado para utilizar o MongoDB.
    /// </returns>
    public static IResourceBuilder<ProjectResource> WithMongoReplicaSet(this IResourceBuilder<ProjectResource> project, IDistributedApplicationBuilder builder, string name, string tag = "latest")
    {
        var mongo = builder.AddMongoReplicaSet(name, tag);

        return project.WaitForMongoReplicaSet(mongo);
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