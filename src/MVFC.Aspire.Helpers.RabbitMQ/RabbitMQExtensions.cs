namespace MVFC.Aspire.Helpers.RabbitMQ;

/// <summary>
/// Fornece métodos de extensão para facilitar a configuração e integração do recurso RabbitMQ
/// em aplicações distribuídas utilizando o Aspire.
/// </summary>
public static class RabbitMQExtensions
{
    private static readonly string[] _roleAdmin = ["administrator"];
    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true
    };

    /// <summary>
    /// Adiciona um recurso RabbitMQ à aplicação distribuída.
    /// Permite configurar opções como imagem, portas, credenciais, exchanges, queues e persistência de dados.
    /// </summary>
    /// <param name="builder">O construtor da aplicação distribuída.</param>
    /// <param name="name">O nome do recurso RabbitMQ.</param>
    /// <param name="rabbitMQConfig">Configurações opcionais para o RabbitMQ. Se nulo, utiliza valores padrão.</param>
    /// <returns>Um <see cref="IResourceBuilder{RabbitMQResource}"/> configurado.</returns>
    public static IResourceBuilder<RabbitMQResource> AddRabbitMQ(
        this IDistributedApplicationBuilder builder,
        string name,
        RabbitMQConfig? rabbitMQConfig = null)
    {

        rabbitMQConfig ??= new RabbitMQConfig();

        var resource = new RabbitMQResource(name);

        var resourceBuilder = builder.AddResource(resource)
            .WithImage(rabbitMQConfig.ImageName)
            .WithImageTag(rabbitMQConfig.ImageTag)
            .WithEndpoint(
                port: rabbitMQConfig.Port,
                targetPort: RabbitMQDefaults.DefaultAmqpPort,
                name: RabbitMQResource.AmqpEndpointName)
            .WithHttpEndpoint(
                port: rabbitMQConfig.ManagementPort,
                targetPort: RabbitMQDefaults.DefaultManagementPort,
                name: RabbitMQResource.ManagementEndpointName);

        // Configurar credenciais
        resourceBuilder
            .WithEnvironment("RABBITMQ_DEFAULT_USER", rabbitMQConfig.Username)
            .WithEnvironment("RABBITMQ_DEFAULT_PASS", rabbitMQConfig.Password);

        // Configurar persistência se volume especificado
        if (!string.IsNullOrWhiteSpace(rabbitMQConfig.VolumeName))
        {
            resourceBuilder.WithVolume(rabbitMQConfig.VolumeName, "/var/lib/rabbitmq");
        }

        // Criar exchanges e queues via definitions.json importado automaticamente pelo RabbitMQ
        if (rabbitMQConfig.Exchanges?.Count > 0 || rabbitMQConfig.Queues?.Count > 0)
        {
            ConfigureDefinitions(resourceBuilder, rabbitMQConfig);
        }

        return resourceBuilder;
    }

    /// <summary>
    /// Gera o arquivo definitions.json com exchanges, queues e bindings,
    /// e configura o RabbitMQ para importá-lo automaticamente na inicialização.
    /// </summary>
    private static void ConfigureDefinitions(
        IResourceBuilder<RabbitMQResource> resourceBuilder,
        RabbitMQConfig config)
    {
        var definitions = BuildDefinitionsJson(config);
        var definitionsJson = JsonSerializer.Serialize(definitions, _options);

        // Gerar o arquivo definitions.json em um diretório temporário
        var tempDir = Path.Combine(Path.GetTempPath(), "aspire-rabbitmq", resourceBuilder.Resource.Name);
        Directory.CreateDirectory(tempDir);

        var definitionsPath = Path.Combine(tempDir, "definitions.json");
        File.WriteAllText(definitionsPath, definitionsJson);

        // Montar definitions.json e configurar carregamento via variável de ambiente
        resourceBuilder
            .WithBindMount(definitionsPath, "/etc/rabbitmq/definitions.json")
            .WithEnvironment("RABBITMQ_SERVER_ADDITIONAL_ERL_ARGS",
                "-rabbitmq_management load_definitions \"/etc/rabbitmq/definitions.json\"");
    }

    /// <summary>
    /// Gera hash de senha no formato esperado pelo RabbitMQ: Base64(salt + SHA256(salt + password)).
    /// </summary>
    private static string HashPassword(string password)
    {
        var salt = new byte[4];
        System.Security.Cryptography.RandomNumberGenerator.Fill(salt);
        var passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);

        var toHash = new byte[salt.Length + passwordBytes.Length];
        Buffer.BlockCopy(salt, 0, toHash, 0, salt.Length);
        Buffer.BlockCopy(passwordBytes, 0, toHash, salt.Length, passwordBytes.Length);

        var hash = System.Security.Cryptography.SHA256.HashData(toHash);

        var result = new byte[salt.Length + hash.Length];
        Buffer.BlockCopy(salt, 0, result, 0, salt.Length);
        Buffer.BlockCopy(hash, 0, result, salt.Length, hash.Length);

        return Convert.ToBase64String(result);
    }

    /// <summary>
    /// Constrói o objeto de definições do RabbitMQ com usuário, vhosts, permissões, exchanges, queues e bindings.
    /// </summary>
    private static RabbitMQDefinitions BuildDefinitionsJson(RabbitMQConfig config)
    {

        var exchanges = new List<RabbitMQExchange>();
        var queues = new List<RabbitMQQueue>();
        var bindings = new List<RabbitMQBinding>();

        // Exchanges
        if (config.Exchanges is not null)
        {
            foreach (var exchange in config.Exchanges)
            {
                exchanges.Add(new RabbitMQExchange(
                    Name: exchange.Name,
                    Vhost: "/",
                    Type: exchange.Type,
                    Durable: exchange.Durable,
                    AutoDelete: exchange.AutoDelete,
                    Internal: false,
                    Arguments: []
                ));
            }
        }

        // Queues e Bindings
        if (config.Queues is not null)
        {
            foreach (var queue in config.Queues)
            {
                var arguments = new Dictionary<string, object>();

                if (!string.IsNullOrWhiteSpace(queue.DeadLetterExchange))
                    arguments["x-dead-letter-exchange"] = queue.DeadLetterExchange;

                if (queue.MessageTTL.HasValue)
                    arguments["x-message-ttl"] = queue.MessageTTL.Value;

                queues.Add(new RabbitMQQueue(
                    Name: queue.Name,
                    Vhost: "/",
                    Durable: queue.Durable,
                    AutoDelete: queue.AutoDelete,
                    Arguments: arguments
                ));

                // Criar binding se ExchangeName fornecido
                if (!string.IsNullOrWhiteSpace(queue.ExchangeName))
                {
                    bindings.Add(new RabbitMQBinding(
                        Source: queue.ExchangeName,
                        Vhost: "/",
                        Destination: queue.Name,
                        DestinationType: "queue",
                        RoutingKey: queue.RoutingKey ?? queue.Name,
                        Arguments: []
                    ));
                }
            }
        }

        return new RabbitMQDefinitions(
            Users:
            [
                new RabbitMQUser(
                    Name: config.Username,
                    PasswordHash: HashPassword(config.Password),
                    HashingAlgorithm: "rabbit_password_hashing_sha256",
                    Tags: _roleAdmin)
            ],
            Vhosts:
            [
                new RabbitMQVhost(Name: "/")
            ],
            Permissions:
            [
                new RabbitMQPermission(
                    User: config.Username,
                    Vhost: "/",
                    Configure: ".*",
                    Write: ".*",
                    Read: ".*")
            ],
            Exchanges: exchanges,
            Queues: queues,
            Bindings: bindings
        );
    }

    /// <summary>
    /// Aguarda até que o recurso RabbitMQ esteja disponível antes de iniciar o projeto Aspire.
    /// Adiciona uma referência ao recurso RabbitMQ, garantindo que o projeto só será iniciado após o RabbitMQ estar pronto.
    /// </summary>
    /// <param name="project">O builder do recurso do projeto Aspire.</param>
    /// <param name="rabbitMQ">O builder do recurso RabbitMQ.</param>
    /// <returns>O builder do recurso do projeto Aspire com dependência do RabbitMQ.</returns>
    public static IResourceBuilder<ProjectResource> WaitForRabbitMQ(
        this IResourceBuilder<ProjectResource> project,
        IResourceBuilder<RabbitMQResource> rabbitMQ) =>

        project.WaitFor(rabbitMQ)
               .WithReference(rabbitMQ);

    /// <summary>
    /// Adiciona uma referência ao recurso RabbitMQ em um projeto Aspire.
    /// </summary>
    /// <param name="project">O builder do recurso do projeto.</param>
    /// <param name="builder">O construtor da aplicação distribuída.</param>
    /// <param name="name">O nome do recurso RabbitMQ.</param>
    /// <param name="rabbitMQConfig">Configurações opcionais para o RabbitMQ.</param>
    /// <param name="connectionStringSection">Seção da connection string na configuração da aplicação.</param>
    /// <returns>O builder do recurso do projeto com a referência ao RabbitMQ.</returns>
    public static IResourceBuilder<ProjectResource> WithRabbitMQ(
        this IResourceBuilder<ProjectResource> project,
        IDistributedApplicationBuilder builder,
        string name,
        RabbitMQConfig? rabbitMQConfig = null,
        string connectionStringSection = RabbitMQDefaults.DefaultConnectionStringSection)
    {

        IResourceBuilder<RabbitMQResource> rabbitMQ;

        if (!builder.TryCreateResourceBuilder(name, out rabbitMQ!))
        {
            rabbitMQ = builder.AddRabbitMQ(name, rabbitMQConfig);
        }

        return project.WaitForRabbitMQ(rabbitMQ)
                      .WithEnvironment(connectionStringSection, rabbitMQ.Resource.ConnectionStringExpression);
    }
}
