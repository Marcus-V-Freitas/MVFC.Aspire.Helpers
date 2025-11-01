namespace MVFC.Aspire.Helpers.Mailpit.Models;

/// <summary>
/// Configurações para o recurso Mailpit
/// </summary>
/// <param name="HttpPort">Porta HTTP para a interface web (padrão: null)</param>
/// <param name="SmtpPort">Porta SMTP para receber emails (padrão: null)</param>
/// <param name="MaxMessages">Número máximo de mensagens a armazenar (padrão: 500)</param>
/// <param name="DataFilePath">Caminho para arquivo de dados persistente</param>
/// <param name="SmtpAuthAcceptAny">Aceitar qualquer credencial SMTP</param>
/// <param name="SmtpAuthAllowInsecure">Permitir autenticação SMTP insegura</param>
/// <param name="EnableWebAuth">Habilitar autenticação básica na UI web</param>
/// <param name="WebAuthUsername">Usuário para autenticação web</param>
/// <param name="WebAuthPassword">Senha para autenticação web</param>
/// <param name="ImageName">Nome da imagem Docker do Mailpit</param>
/// <param name="ImageTag">Tag da imagem Docker do Mailpit</param>
/// <param name="VerboseLogging">Habilitar modo verbose de logs</param>
/// <param name="MaxMessageSize">Limite de tamanho para mensagens em MB</param>
/// <param name="SmtpHostname">Nome do hostname do servidor SMTP</param>
public sealed record MailpitConfig(
    int? HttpPort = null,
    int? SmtpPort = null,
    int MaxMessages = 500,
    string? DataFilePath = null,
    bool SmtpAuthAcceptAny = true,
    bool SmtpAuthAllowInsecure = true,
    bool EnableWebAuth = false,
    string? WebAuthUsername = null,
    string? WebAuthPassword = null,
    string ImageName = "axllent/mailpit",
    string ImageTag = "latest",
    bool VerboseLogging = false,
    int MaxMessageSize = 50,
    string? SmtpHostname = null);