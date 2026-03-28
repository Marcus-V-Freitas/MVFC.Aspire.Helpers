// log-request-response.js
// SharedFlow: injeta informações de debug como response headers
// NOTA: print() no Apigee JS só aparece no Trace tool, não no Docker stdout.
// Para visibilidade imediata, usamos headers + variáveis de contexto.

var verb = context.getVariable("request.verb") || "?";
var path = context.getVariable("proxy.pathsuffix") || "/";
var clientIp = context.getVariable("client.ip") || "unknown";
var statusCode = context.getVariable("response.status.code") || "?";
var proxyName = context.getVariable("apiproxy.name") || "unknown";
var userAgent = context.getVariable("request.header.User-Agent") || "unknown";

// Monta um resumo compacto do request para o header de debug
var logSummary = verb + " " + path + " → " + statusCode + " | IP=" + clientIp;

// Seta headers de debug na resposta
context.setVariable("response.header.X-Debug-Log", logSummary);
context.setVariable("response.header.X-Debug-Proxy", proxyName);
context.setVariable("response.header.X-Debug-Client-IP", clientIp);
context.setVariable("response.header.X-Debug-User-Agent", userAgent);
context.setVariable("response.header.X-Debug-Timestamp", new Date().toISOString());

// print() ainda fica — aparece no Trace tool quando habilitado
print("[APIGEE-LOG] " + logSummary + " | UA=" + userAgent);
