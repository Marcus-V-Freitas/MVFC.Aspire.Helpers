namespace MVFC.Aspire.Helpers.WireMock.Settings;

/// <summary>
/// Configurações customizáveis para serialização e deserialização de objetos em endpoints WireMock.
/// Permite definir delegates para serialização e deserialização, além de métodos genéricos para facilitar o uso.
/// </summary>
public sealed class EndpointSettings {
    /// <summary>
    /// Delegate responsável por serializar um objeto para string.
    /// Pode ser sobrescrito para utilizar qualquer serializador desejado.
    /// </summary>
    public Func<object, string> Serializar { get; set; } =
        obj => JsonSerializer.Serialize(obj);

    /// <summary>
    /// Delegate responsável por deserializar uma string para um objeto do tipo especificado.
    /// Pode ser sobrescrito para utilizar qualquer desserializador desejado.
    /// </summary>
    public Func<string, Type, object?> Desserializar { get; set; } =
        (json, type) => JsonSerializer.Deserialize(json, type);

    /// <summary>
    /// Serializa um objeto do tipo <typeparamref name="T"/> para string usando o delegate <see cref="Serializar"/>.
    /// </summary>
    /// <typeparam name="T">Tipo do objeto a ser serializado.</typeparam>
    /// <param name="obj">Objeto a ser serializado.</param>
    /// <returns>Representação em string do objeto serializado.</returns>
    public string SerializarGenerico<T>(T obj) =>
        Serializar(obj!);

    /// <summary>
    /// Desserializa uma string para um objeto do tipo <typeparamref name="T"/> usando o delegate <see cref="Desserializar"/>.
    /// </summary>
    /// <typeparam name="T">Tipo de destino da desserialização.</typeparam>
    /// <param name="json">String contendo o objeto serializado.</param>
    /// <returns>Objeto desserializado do tipo <typeparamref name="T"/>.</returns>
    public T? DesserializarGenerico<T>(string json) =>
        (T?)Desserializar(json, typeof(T));
}
