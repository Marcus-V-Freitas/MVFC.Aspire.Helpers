// validate-credentials.js
// Valida credenciais decodificadas pela BasicAuthentication policy
// Demo: aceita user=admin, password=secret123

var username = context.getVariable("credentials.username");
var password = context.getVariable("credentials.password");

// Credenciais de demonstração (em produção, usar KVM ou serviço externo)
var validUser = "admin";
var validPass = "secret123";

if (!username || !password) {
    context.setVariable("credentials.isValid", "false");
    context.setVariable("credentials.errorMessage", "Credenciais não fornecidas. Use Authorization: Basic base64(user:pass)");
} else if (username === validUser && password === validPass) {
    context.setVariable("credentials.isValid", "true");
    context.setVariable("credentials.authenticatedUser", username);
} else {
    context.setVariable("credentials.isValid", "false");
    context.setVariable("credentials.errorMessage", "Credenciais inválidas para o usuario: " + username);
}
