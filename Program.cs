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
    var html = @"<!DOCTYPE html>
<html lang=""pt-BR"">
<head>
  <meta charset=""UTF-8"">
  <title>EmailMicroService - Documentação da API</title>
  <style>
    body {
      font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
      background-color: #f3f4f6;
      margin: 0;
      padding: 0;
    }

    header {
      background-color: #0f172a;
      color: #fff;
      padding: 20px 40px;
      border-bottom: 4px solid #38bdf8;
    }

    header h1 {
      margin: 0;
      font-size: 28px;
    }

    .container {
      padding: 40px;
      max-width: 1000px;
      margin: auto;
    }

    h2 {
      color: #0f172a;
      border-bottom: 2px solid #38bdf8;
      padding-bottom: 5px;
      margin-top: 40px;
    }

    code {
      background-color: #e2e8f0;
      padding: 2px 6px;
      border-radius: 4px;
      font-family: monospace;
    }

    pre {
      background-color: #e2e8f0;
      padding: 15px;
      border-radius: 6px;
      overflow-x: auto;
    }

    ul {
      line-height: 1.8;
    }

    .note {
      background-color: #fff8c5;
      border-left: 4px solid #facc15;
      padding: 10px 15px;
      margin-top: 10px;
      border-radius: 4px;
    }
  </style>
</head>
<body>
  <header>
    <h1>EmailMicroService - Documentação da API v1.0</h1>
  </header>

  <div class=""container"">
    <h2>Descrição</h2>
    <p>Este endpoint permite o envio de emails via SMTP utilizando o Gmail. Suporta corpo HTML e anexos codificados em Base64.</p>

    <h2>Endpoint</h2>
    <p><strong>Método:</strong> <code>POST</code></p>
    <p><strong>URL:</strong> <code>/send</code></p>
    <p><strong>Content-Type:</strong> <code>application/json</code></p>

    <h2>Exemplo de Requisição</h2>
    <pre>{
  ""Server"": ""emailmicroservicedash@gmail.com"",
  ""AppPassWord"": ""vgkg tdxs ffqu mjgr"",
  ""Sender"": ""Administrador"",
  ""To"": ""destinatario@email.com"",
  ""Subject"": ""Assunto do email"",
  ""Body"": ""<h1>Conteúdo HTML</h1>"",
  ""Attachments"": [
    {
      ""FileName"": ""documento.pdf"",
      ""Base64"": ""base64string..."",
      ""ContentType"": ""application/pdf""
    }
  ]
}</pre>

    <h2>Modelo de Dados</h2>
    <h3><code>EmailRequest</code></h3>
    <ul>
      <li><strong>Server</strong> (<code>string</code>): Email usado para autenticação SMTP.  
        <div class=""note"">Se não for enviado, será usado o padrão: <code>emailmicroservicedash@gmail.com</code></div>
      </li>
      <li><strong>AppPassWord</strong> (<code>string</code>): Senha de aplicativo do Gmail.  
        <div class=""note"">Se não for enviado, será usada a senha padrão: <code>vgkg tdxs ffqu mjgr</code></div>
      </li>
      <li><strong>Sender</strong> (<code>string</code>): Nome do remetente. Padrão: <code>Administrador</code></li>
      <li><strong>To</strong> (<code>string</code>): Email do destinatário.</li>
      <li><strong>Subject</strong> (<code>string</code>): Assunto do email.</li>
      <li><strong>Body</strong> (<code>string</code>): Corpo do email em HTML.</li>
      <li><strong>Attachments</strong> (<code>List&lt;AttachmentRequest&gt;</code>): Lista de anexos.</li>
    </ul>

    <h3><code>AttachmentRequest</code></h3>
    <ul>
      <li><strong>FileName</strong> (<code>string</code>): Nome do arquivo.</li>
      <li><strong>Base64</strong> (<code>string</code>): Conteúdo do arquivo codificado em Base64.</li>
      <li><strong>ContentType</strong> (<code>string</code>): Tipo MIME do arquivo (ex: <code>application/pdf</code>).</li>
    </ul>

    <h2>Respostas</h2>
    <ul>
      <li><strong>200 OK:</strong> <code>""Email enviado com sucesso""</code></li>
      <li><strong>400 Bad Request:</strong> Erro ao processar anexos</li>
      <li><strong>500 Internal Server Error:</strong> Erro ao enviar email</li>
    </ul>

    <h2>Notas</h2>
    <ul>
      <li>Utiliza autenticação com senha de aplicativo do Gmail.</li>
      <li>Os anexos devem ser enviados como strings Base64 válidas.</li>
      <li>O campo <code>ContentType</code> deve corresponder ao tipo MIME do arquivo.</li>
      <li>O corpo do email pode conter HTML para formatação.</li>
      <li>Campos <code>Server</code> e <code>AppPassWord</code> são opcionais se os valores padrão forem suficientes.</li>
    </ul>
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


