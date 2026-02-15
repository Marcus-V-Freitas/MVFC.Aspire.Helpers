namespace MVFC.Aspire.Helpers.WireMock.Builders;

/// <summary>
/// Builder para configuração de endpoints WireMock integrados ao Aspire.
/// Permite definir autenticação, tipos de corpo, headers e handlers para métodos HTTP.
/// </summary>
/// <remarks>
/// Inicializa uma nova instância de <see cref="EndpointBuilder"/>.
/// </remarks>
/// <param name="server">Instância do servidor WireMock.</param>
/// <param name="path">Caminho do endpoint.</param>
/// <param name="settings">Configurações opcionais de serialização/deserialização. Se nulo, utiliza valores padrão.</param>
public sealed class EndpointBuilder(WireMockServer server, string path, EndpointSettings? settings = null)
{
    private readonly WireMockServer _server = server;
    private readonly string _path = path;
    private Encoding _encoding = Encoding.UTF8;
    private BodyType _requestBodyType = BodyType.Json;
    private BodyType _responseBodyType = BodyType.Json;
    private Func<IRequestMessage, (bool, object?, BodyType)>? _authValidator;
    private readonly Dictionary<string, WireMockList<string>> _customHeaders = [];
    private HttpStatusCode _defaultErrorStatusCode = HttpStatusCode.Unauthorized;
    private readonly EndpointSettings _settings = settings ?? new EndpointSettings();

    /// <summary>
    /// Define o tipo de body esperado na requisição.
    /// </summary>
    /// <param name="bodyType">Tipo do corpo da requisição.</param>
    /// <returns>O próprio <see cref="EndpointBuilder"/> para encadeamento.</returns>
    public EndpointBuilder WithRequestBodyType(BodyType bodyType) {
        _requestBodyType = bodyType;
        return this;
    }

    /// <summary>
    /// Define a codificação utilizada para serialização e desserialização dos corpos das requisições e respostas.
    /// </summary>
    /// <param name="encoding">Instância de <see cref="Encoding"/> a ser utilizada.</param>
    /// <returns>O próprio <see cref="EndpointBuilder"/> para encadeamento.</returns>
    public EndpointBuilder SetEncoding(Encoding encoding) {
        _encoding = encoding;
        return this;
    }

    /// <summary>
    /// Define o tipo de body esperado na resposta.
    /// </summary>
    /// <param name="bodyType">Tipo do corpo da resposta.</param>
    /// <returns>O próprio <see cref="EndpointBuilder"/> para encadeamento.</returns>
    public EndpointBuilder WithResponseBodyType(BodyType bodyType) {
        _responseBodyType = bodyType;
        return this;
    }

    /// <summary>
    /// Define o tipo de body para requisição e resposta.
    /// </summary>
    /// <param name="bodyType">Tipo do corpo para requisição e resposta.</param>
    /// <returns>O próprio <see cref="EndpointBuilder"/> para encadeamento.</returns>
    public EndpointBuilder WithDefaultBodyType(BodyType bodyType) {
        _requestBodyType = bodyType;
        _responseBodyType = bodyType;
        return this;
    }

    /// <summary>
    /// Define o status code padrão para erros de autenticação.
    /// </summary>
    /// <param name="statusCode">Status code de erro padrão.</param>
    /// <returns>O próprio <see cref="EndpointBuilder"/> para encadeamento.</returns>
    public EndpointBuilder WithDefaultErrorStatusCode(HttpStatusCode statusCode) {
        _defaultErrorStatusCode = statusCode;
        return this;
    }

    /// <summary>
    /// Requer autenticação Bearer.
    /// </summary>
    /// <param name="token">Token Bearer esperado.</param>
    /// <param name="error">Objeto de erro retornado em caso de falha.</param>
    /// <param name="contentType">Tipo do corpo do erro.</param>
    /// <returns>O próprio <see cref="EndpointBuilder"/> para encadeamento.</returns>
    public EndpointBuilder RequireBearer(string token, object? error = null, BodyType contentType = BodyType.Json) {
        _authValidator = request => {
            var auth = request.Headers != null && request.Headers.TryGetValue("Authorization", out var authList)
                ? authList.FirstOrDefault()
                : null;

            if (auth?.StartsWith("Bearer ") != true || auth[7..] != token)
                return (false, error, contentType);

            return (true, null, contentType);
        };
        return this;
    }

    /// <summary>
    /// Requer autenticação customizada.
    /// </summary>
    /// <param name="validator">Função de validação customizada.</param>
    /// <returns>O próprio <see cref="EndpointBuilder"/> para encadeamento.</returns>
    public EndpointBuilder RequireCustomAuth(Func<IRequestMessage, (bool, object?, BodyType)> validator) {
        _authValidator = validator;
        return this;
    }

    /// <summary>
    /// Adiciona um header de resposta.
    /// </summary>
    /// <param name="key">Nome do header.</param>
    /// <param name="value">Valor do header.</param>
    /// <returns>O próprio <see cref="EndpointBuilder"/> para encadeamento.</returns>
    public EndpointBuilder WithResponseHeader(string key, string value) {
        if (_customHeaders.TryGetValue(key, out var list))
            list.Add(value);
        else
            _customHeaders[key] = [value];
        return this;
    }

    /// <summary>
    /// Adiciona múltiplos headers de resposta.
    /// </summary>
    /// <param name="headers">Dicionário de headers e seus valores.</param>
    /// <returns>O próprio <see cref="EndpointBuilder"/> para encadeamento.</returns>
    public EndpointBuilder WithResponseHeaders(Dictionary<string, IEnumerable<string>> headers) {
        foreach (var kvp in headers) {
            if (_customHeaders.TryGetValue(kvp.Key, out var list))
                list.AddRange(kvp.Value);
            else
                _customHeaders[kvp.Key] = [.. kvp.Value];
        }

        return this;
    }

    /// <summary>
    /// Define o handler para POST.
    /// </summary>
    /// <typeparam name="TRequest">Tipo do corpo da requisição.</typeparam>
    /// <typeparam name="TResponse">Tipo do corpo da resposta.</typeparam>
    /// <param name="handler">Função que processa a requisição e retorna resposta, status e tipo de body.</param>
    /// <returns>O próprio <see cref="EndpointBuilder"/> para encadeamento.</returns>
    public EndpointBuilder OnPost<TRequest, TResponse>(Func<TRequest, (TResponse, HttpStatusCode, BodyType?)> handler) {
        _server.Given(
            Request.Create()
                .UsingPost()
                .WithPath(_path)
        )
        .RespondWith(
            Response.Create()
                .WithCallback(request =>
                    HandleRequest(
                        request,
                        () => ProcessResponse(request, handler)
                    )
                )
        );

        return this;
    }

    /// <summary>
    /// Define o handler para GET.
    /// </summary>
    /// <typeparam name="TResponse">Tipo do corpo da resposta.</typeparam>
    /// <param name="handler">Função que retorna resposta, status e tipo de body.</param>
    /// <returns>O próprio <see cref="EndpointBuilder"/> para encadeamento.</returns>
    public EndpointBuilder OnGet<TResponse>(Func<(TResponse, HttpStatusCode, BodyType?)> handler) {
        _server.Given(
            Request.Create()
                .UsingGet()
                .WithPath(_path)
        )
        .RespondWith(
            Response.Create()
                .WithCallback(request =>
                    HandleRequest(request, () => {
                        var (resp, status, respType) = handler();
                        return (resp, status, respType ?? _responseBodyType);
                    })
                )
        );

        return this;
    }

    /// <summary>
    /// Define o handler para PUT.
    /// </summary>
    /// <typeparam name="TRequest">Tipo do corpo da requisição.</typeparam>
    /// <typeparam name="TResponse">Tipo do corpo da resposta.</typeparam>
    /// <param name="handler">Função que processa a requisição e retorna resposta, status e tipo de body.</param>
    /// <returns>O próprio <see cref="EndpointBuilder"/> para encadeamento.</returns>
    public EndpointBuilder OnPut<TRequest, TResponse>(Func<TRequest, (TResponse, HttpStatusCode, BodyType?)> handler) {
        _server.Given(
            Request.Create()
                .UsingPut()
                .WithPath(_path)
        )
        .RespondWith(
            Response.Create()
                .WithCallback(request =>
                    HandleRequest(
                        request,
                        () => ProcessResponse(request, handler)
                    )
                )
        );

        return this;
    }

    /// <summary>
    /// Define o handler para DELETE.
    /// </summary>
    /// <typeparam name="TResponse">Tipo do corpo da resposta.</typeparam>
    /// <param name="handler">Função que retorna resposta, status e tipo de body.</param>
    /// <returns>O próprio <see cref="EndpointBuilder"/> para encadeamento.</returns>
    public EndpointBuilder OnDelete<TResponse>(Func<(TResponse, HttpStatusCode, BodyType?)> handler) {
        _server.Given(
            Request.Create()
                .UsingDelete()
                .WithPath(_path)
        )
        .RespondWith(
            Response.Create()
                .WithCallback(request =>
                    HandleRequest(request, () => {
                        var (resp, status, respType) = handler();
                        return (resp, status, respType ?? _responseBodyType);
                    })
                )
        );

        return this;
    }

    /// <summary>
    /// Define o handler para PATCH.
    /// </summary>
    /// <typeparam name="TRequest">Tipo do corpo da requisição.</typeparam>
    /// <typeparam name="TResponse">Tipo do corpo da resposta.</typeparam>
    /// <param name="handler">Função que processa a requisição e retorna resposta, status e tipo de body.</param>
    /// <returns>O próprio <see cref="EndpointBuilder"/> para encadeamento.</returns>
    public EndpointBuilder OnPatch<TRequest, TResponse>(Func<TRequest, (TResponse, HttpStatusCode, BodyType?)> handler) {
        _server.Given(
            Request.Create()
                .UsingPatch()
                .WithPath(_path)
        )
        .RespondWith(
            Response.Create()
                .WithCallback(request =>
                    HandleRequest(
                        request,
                        () => ProcessResponse(request, handler)
                    )
                )
        );

        return this;
    }

    /// <summary>
    /// Processa o corpo da requisição e executa o handler fornecido, retornando resposta, status e tipo de body.
    /// </summary>
    /// <typeparam name="TRequest">Tipo do corpo da requisição.</typeparam>
    /// <typeparam name="TResponse">Tipo do corpo da resposta.</typeparam>
    /// <param name="request">Mensagem da requisição recebida.</param>
    /// <param name="handler">Função que processa o objeto da requisição e retorna resposta, status e tipo de body.</param>
    /// <returns>Tupla contendo resposta, status HTTP e tipo de body.</returns>
    private (TResponse, HttpStatusCode, BodyType) ProcessResponse<TRequest, TResponse>(IRequestMessage request, Func<TRequest, (TResponse, HttpStatusCode, BodyType?)> handler) {
        if (string.IsNullOrWhiteSpace(request.Body))
            return (default!, HttpStatusCode.BadRequest, _responseBodyType);

        var reqObj = DeserializeBody<TRequest>(request.Body, _requestBodyType);
        var (resp, status, respType) = handler(reqObj!);
        return (resp, status, respType ?? _responseBodyType);
    }

    /// <summary>
    /// Desserializa o corpo da requisição para o tipo especificado, conforme o tipo de body.
    /// </summary>
    /// <typeparam name="TRequest">Tipo de destino da desserialização.</typeparam>
    /// <param name="body">Corpo da requisição em string.</param>
    /// <param name="bodyType">Tipo do corpo da requisição.</param>
    /// <returns>Objeto desserializado ou null.</returns>
    private TRequest? DeserializeBody<TRequest>(string body, BodyType bodyType)
        => bodyType switch {
            BodyType.Json => _settings.DeserializeGeneric<TRequest>(body),
            BodyType.String => (TRequest)(object)body,
            BodyType.Bytes => (TRequest)(object)_encoding.GetBytes(body),
            BodyType.FormUrlEncoded => ParseFormUrlEncoded<TRequest>(body),
            _ => throw new NotSupportedException($"BodyType '{bodyType}' não suportado para deserialização.")
        };

    /// <summary>
    /// Converte um corpo FormUrlEncoded em um dicionário de chave/valor.
    /// </summary>
    /// <typeparam name="TRequest">Tipo de destino (deve ser Dictionary&lt;string, string&gt;).</typeparam>
    /// <param name="body">Corpo da requisição em formato FormUrlEncoded.</param>
    /// <returns>Dicionário de chave/valor ou lança exceção se tipo não suportado.</returns>
    private static TRequest? ParseFormUrlEncoded<TRequest>(string body) {
        if (typeof(TRequest) == typeof(Dictionary<string, string>)) {
            var dict = new Dictionary<string, string>();
            var pairs = body.Split('&', StringSplitOptions.RemoveEmptyEntries);
            foreach (var pair in pairs) {
                var kv = pair.Split('=', 2);
                var key = Uri.UnescapeDataString(kv[0]);
                var value = kv.Length > 1 ? Uri.UnescapeDataString(kv[1]) : "";
                dict[key] = value;
            }
            return (TRequest)(object)dict;
        }
        throw new NotSupportedException("FormUrlEncoded só suporta Dictionary<string, string> por padrão.");
    }

    /// <summary>
    /// Serializa um dicionário de chave/valor para o formato FormUrlEncoded.
    /// </summary>
    /// <param name="dict">Dicionário de chave/valor.</param>
    /// <returns>String no formato FormUrlEncoded.</returns>
    private static string SerializeFormUrlEncoded(IDictionary<string, string> dict)
        => string.Join("&", dict.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));

    /// <summary>
    /// Executa a validação de autenticação (se definida) e processa o handler, retornando a resposta HTTP.
    /// </summary>
    /// <typeparam name="T">Tipo do corpo da resposta.</typeparam>
    /// <param name="request">Mensagem da requisição recebida.</param>
    /// <param name="handler">Função que retorna resposta, status e tipo de body.</param>
    /// <returns>Objeto <see cref="ResponseMessage"/> representando a resposta HTTP.</returns>
    private ResponseMessage HandleRequest<T>(IRequestMessage request, Func<(T, HttpStatusCode, BodyType)> handler) {
        if (_authValidator != null) {
            var (valido, error, errorContentType) = _authValidator(request);
            if (!valido) return ResponseMessageBuilder(error, _defaultErrorStatusCode, errorContentType);
        }
        var (respObj, statusCode, contentType) = handler();
        return ResponseMessageBuilder(respObj, statusCode, contentType);
    }

    /// <summary>
    /// Constrói o objeto <see cref="ResponseMessage"/> com base nos parâmetros fornecidos.
    /// </summary>
    /// <typeparam name="T">Tipo do corpo da resposta.</typeparam>
    /// <param name="obj">Objeto de resposta.</param>
    /// <param name="statusCode">Status HTTP da resposta.</param>
    /// <param name="contentType">Tipo do corpo da resposta.</param>
    /// <returns>Objeto <see cref="ResponseMessage"/> configurado.</returns>
    private ResponseMessage ResponseMessageBuilder<T>(T obj, HttpStatusCode statusCode, BodyType contentType) {
        var response = new ResponseMessage {
            StatusCode = statusCode,
            Headers = new Dictionary<string, WireMockList<string>>(_customHeaders),
            BodyData = new BodyData {
                Encoding = _encoding,
                DetectedBodyType = contentType
            }
        };

        if (obj is null) {
            response.BodyData = null;
            return response;
        }

        switch (contentType) {
            case BodyType.Json:
                response.BodyData.BodyAsString = _settings.SerializeGeneric(obj);
                response.Headers["Content-Type"] = ["application/json"];
                response.BodyData.DetectedBodyType = BodyType.String;
                break;
            case BodyType.Bytes:
                response.BodyData.BodyAsBytes = obj is byte[] bytes ? bytes : response.BodyData.Encoding!.GetBytes(obj?.ToString() ?? "");
                response.Headers["Content-Type"] = ["application/octet-stream"];
                break;
            case BodyType.FormUrlEncoded:
                response.BodyData.BodyAsString = SerializeFormUrlEncoded(obj as IDictionary<string, string> ?? new Dictionary<string, string>());
                response.Headers["Content-Type"] = ["application/x-www-form-urlencoded"];
                break;
            default:
                response.BodyData.BodyAsString = obj?.ToString();
                response.Headers["Content-Type"] = ["text/plain"];
                break;
        }

        return response;
    }
}
