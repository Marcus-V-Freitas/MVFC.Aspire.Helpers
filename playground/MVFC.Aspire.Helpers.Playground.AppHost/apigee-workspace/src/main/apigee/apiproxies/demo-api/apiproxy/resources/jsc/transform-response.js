// transform-response.js
// Envelopa o response do backend em um formato padronizado
var responsePayload = context.getVariable("response.content");
var statusCode = context.getVariable("response.status.code");
var verb = context.getVariable("request.verb");
var path = context.getVariable("proxy.pathsuffix");

var data = null;
try {
    data = JSON.parse(responsePayload);
} catch (e) {
    data = responsePayload;
}

var envelope = {
    status: (statusCode >= 200 && statusCode < 300) ? "ok" : "error",
    code: parseInt(statusCode, 10),
    data: data,
    metadata: {
        proxy: "demo-api",
        method: verb,
        path: path,
        timestamp: new Date().toISOString(),
        transformedBy: "JS-TransformResponse"
    }
};

context.setVariable("response.content", JSON.stringify(envelope));
context.setVariable("response.header.Content-Type", "application/json");
