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
    //return Results.Ok("v1");

    Console.WriteLine("aqui");

    var html = File.ReadAllText("docs.html"); // arquivo de documentação
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


