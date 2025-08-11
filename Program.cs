using EmailMicroService.Src;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Builder;
using MimeKit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTudo", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("PermitirTudo"); // aplica a política

app.MapGet("/", () =>
{
    var html = @"
    <html>
    <head>
        <title>API Documentation</title>
        <style>
            body { font-family: Arial, sans-serif; max-width: 800px; margin: auto; line-height: 1.6; }
            h1 { color: #0a516d; }
            code { background: #f4f4f4; padding: 2px 4px; border-radius: 4px; }
            .endpoint { border-left: 4px solid #0a516d; padding-left: 10px; margin-bottom: 20px; }
        </style>
    </head>
    <body>
        <h1>📚 API Documentation</h1>
        <p>Bem-vindo à documentação da API. Aqui estão os endpoints disponíveis:</p>

        <div class='endpoint'>
            <h2>POST /login</h2>
            <p>Realiza autenticação do usuário.</p>
            <strong>Body (JSON):</strong>
            <pre>{ ""email"": ""string"", ""password"": ""string"" }</pre>
            <strong>Retorno:</strong> 200 OK com token JWT.
        </div>

        <div class='endpoint'>
            <h2>POST /send-recovery-email</h2>
            <p>Envia o e-mail com token de recuperação de senha.</p>
            <strong>Body (JSON):</strong>
            <pre>{ ""email"": ""string"" }</pre>
        </div>

        <div class='endpoint'>
            <h2>POST /reset-password</h2>
            <p>Redefine a senha usando o token recebido por e-mail.</p>
            <strong>Body (JSON):</strong>
            <pre>{ ""token"": ""string"", ""novaSenha"": ""string"" }</pre>
        </div>
    </body>
    </html>";

    return Results.Content(html, "text/html");
});


app.MapPost("/send", async (EmailRequest emailRequest) =>
{
    var email = new MimeMessage();
    email.From.Add(new MailboxAddress(emailRequest.Sender, emailRequest.Server));
    email.To.Add(MailboxAddress.Parse(emailRequest.To));
    email.Subject = emailRequest.Subject;

    var builder = new BodyBuilder
    {
        HtmlBody = emailRequest.Body
    };

    // Adiciona anexos
    foreach (var att in emailRequest.Attachments)
    {
        try
        {
            byte[] fileBytes = Convert.FromBase64String(att.Base64);
            builder.Attachments.Add(att.FileName, fileBytes, ContentType.Parse(att.ContentType));
        }
        catch (Exception ex)
        {
            return Results.BadRequest($"Erro no anexo '{att.FileName}': {ex.Message}");
        }
    }

    email.Body = builder.ToMessageBody();

    try
    {
        using var client = new SmtpClient();
        await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(emailRequest.Server, emailRequest.AppPassWord);
        await client.SendAsync(email);
        await client.DisconnectAsync(true);

        return Results.Ok("Email enviado com sucesso");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Erro ao enviar email: {ex.Message}");
    }
});



app.Urls.Add("http://+:80");

app.Run();


