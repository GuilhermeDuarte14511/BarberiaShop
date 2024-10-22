using BarberShop.Infrastructure.Data;
using BarberShop.Infrastructure.Services;
using BarberShop.Infrastructure.Repositories;
using BarberShop.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using BarberShop.Domain.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using BarberShop.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Configurar a string de conexão usando o appsettings.json
builder.Services.AddDbContext<BarbeariaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BarberShopDb")));

// Registrar serviços no contêiner de injeção de dependências
builder.Services.AddScoped<IEmailService, EmailService>(provider =>
    new EmailService(builder.Configuration["SendGridApiKey"])); // SendGrid API Key do appsettings.json

builder.Services.AddScoped<IMessageQueueService, RabbitMQService>(provider =>
    new RabbitMQService("localhost", provider.GetRequiredService<IEmailService>())); // Ajuste o host do RabbitMQ se necessário

// Registrar repositórios
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IBarbeiroRepository, BarbeiroRepository>();
builder.Services.AddScoped<IServicoRepository, ServicoRepository>();
builder.Services.AddScoped<IAgendamentoRepository, AgendamentoRepository>();
builder.Services.AddScoped<IRepository<AgendamentoServico>, AgendamentoServicoRepository>();

// Registrar serviços da camada de aplicação
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IAgendamentoService, AgendamentoService>();
builder.Services.AddScoped<IBarbeiroService, BarbeiroService>();

// Configurar autenticação com cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Login";   // Define o caminho da página de login
        options.LogoutPath = "/Login/Logout"; // Define o caminho da página de logout
    });

// Adicionar serviços MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configurar pipeline de processamento de requisições HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Middleware de autenticação
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

app.Run();
