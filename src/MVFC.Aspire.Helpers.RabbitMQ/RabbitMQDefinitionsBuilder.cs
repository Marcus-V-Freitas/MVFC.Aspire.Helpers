namespace MVFC.Aspire.Helpers.RabbitMQ;

/// <summary>
/// Responsible for building the RabbitMQ definitions.json file
/// from the exchange, queue, and credential configurations of the resource.
/// </summary>
internal static class RabbitMQDefinitionsBuilder
{
    private static readonly string[] _roleAdmin = ["administrator"];
    internal static readonly JsonSerializerOptions JsonOptions =  new()
    {
        WriteIndented = true
    };

    /// <summary>
    /// Builds the RabbitMQ definitions object from the resource state.
    /// </summary>
    internal static RabbitMQDefinitions Build(RabbitMQResource resource)
    {
        var exchanges = BuildExchanges(resource.Exchanges);
        var (queues, bindings) = BuildQueuesAndBindings(resource.Queues);

        return new RabbitMQDefinitions(
            Users:
            [
                new RabbitMQUser(
                    Name: resource.Username,
                    PasswordHash: HashPassword(resource.Password),
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
                    User: resource.Username,
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
    /// Builds the list of exchanges from the provided configurations.
    /// </summary>
    private static List<RabbitMQExchange> BuildExchanges(List<ExchangeConfig>? exchangeConfigs)
    {
        return exchangeConfigs is null or { Count: 0 }
            ? []
            : [.. exchangeConfigs.Select(e => new RabbitMQExchange(
            Name: e.Name,
            Vhost: "/",
            Type: e.Type,
            Durable: e.Durable,
            AutoDelete: e.AutoDelete,
            Internal: false,
            Arguments: []
        ))];
    }

    /// <summary>
    /// Builds the queue and binding lists from the provided configurations.
    /// Returns a tuple with empty lists if no configurations exist.
    /// </summary>
    private static (List<RabbitMQQueue> Queues, List<RabbitMQBinding> Bindings) BuildQueuesAndBindings(
        List<QueueConfig>? queueConfigs)
    {
        if (queueConfigs is null or { Count: 0 })
            return ([], []);

        var queues = new List<RabbitMQQueue>(queueConfigs.Count);
        var bindings = new List<RabbitMQBinding>();

        foreach (var queue in queueConfigs)
        {
            queues.Add(new RabbitMQQueue(
                Name: queue.Name,
                Vhost: "/",
                Durable: queue.Durable,
                AutoDelete: queue.AutoDelete,
                Arguments: BuildQueueArguments(queue)
            ));

            var binding = BuildQueueBinding(queue);
            if (binding is not null)
                bindings.Add(binding);
        }

        return (queues, bindings);
    }

    /// <summary>
    /// Builds the extra arguments dictionary for a queue (dead-letter exchange, message TTL).
    /// </summary>
    private static Dictionary<string, object> BuildQueueArguments(QueueConfig queue)
    {
        var arguments = new Dictionary<string, object>();

        if (!string.IsNullOrWhiteSpace(queue.DeadLetterExchange))
            arguments["x-dead-letter-exchange"] = queue.DeadLetterExchange;

        if (queue.MessageTTL.HasValue)
            arguments["x-message-ttl"] = queue.MessageTTL.Value;

        return arguments;
    }

    /// <summary>
    /// Builds the binding between the queue and the exchange, if an exchange is configured.
    /// Returns <see langword="null"/> if the queue is not associated with any exchange.
    /// </summary>
    private static RabbitMQBinding? BuildQueueBinding(QueueConfig queue)
    {
        if (string.IsNullOrWhiteSpace(queue.ExchangeName))
            return null;

        return new RabbitMQBinding(
            Source: queue.ExchangeName,
            Vhost: "/",
            Destination: queue.Name,
            DestinationType: "queue",
            RoutingKey: queue.RoutingKey ?? queue.Name,
            Arguments: []
        );
    }

    /// <summary>
    /// Generates the SHA256 password hash in the format expected by RabbitMQ (salt + hash, Base64 encoded).
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
}
