namespace MVFC.Aspire.Helpers.WireMock.Settings;

/// <summary>
/// Customizable settings for object serialization and deserialization in WireMock endpoints.
/// Allows defining delegates for serialization and deserialization, as well as generic methods for convenience.
/// </summary>
public sealed class EndpointSettings
{
    /// <summary>
    /// Delegate responsible for serializing an object to a string.
    /// Can be overridden to use any desired serializer.
    /// </summary>
    public Func<object, string> Serialize { get; set; } =
        obj => JsonSerializer.Serialize(obj);

    /// <summary>
    /// Delegate responsible for deserializing a string to an object of the specified type.
    /// Can be overridden to use any desired deserializer.
    /// </summary>
    public Func<string, Type, object?> Deserialize { get; set; } =
        (json, type) => JsonSerializer.Deserialize(json, type);

    /// <summary>
    /// Serializes an object of type <typeparamref name="T"/> to a string using the <see cref="Serialize"/> delegate.
    /// </summary>
    /// <typeparam name="T">Type of the object to be serialized.</typeparam>
    /// <param name="obj">Object to be serialized.</param>
    /// <returns>String representation of the serialized object.</returns>
    public string SerializeGeneric<T>(T obj) =>
        Serialize(obj!);

    /// <summary>
    /// Deserializes a string to an object of type <typeparamref name="T"/> using the <see cref="Deserialize"/> delegate.
    /// </summary>
    /// <typeparam name="T">Target deserialization type.</typeparam>
    /// <param name="json">String containing the serialized object.</param>
    /// <returns>Deserialized object of type <typeparamref name="T"/>.</returns>
    public T? DeserializeGeneric<T>(string json) =>
        (T?)Deserialize(json, typeof(T));
}
