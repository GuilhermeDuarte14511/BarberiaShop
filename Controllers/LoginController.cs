using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BarberShopMVC.Controllers
{
    public class LoginController : Controller
    {
        private readonly IClienteRepository _clienteRepository;

        public LoginController(IClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository;
        }

        // Exibe a tela de login
        [HttpGet]
        public IActionResult Login()
        {
            return View(); // Retorna a view de login
        }

        // Lida com o processo de login e autenticação do usuário
        [HttpPost]
        public async Task<IActionResult> Login(string phoneInput, string emailInput)
        {
            string userInput = string.IsNullOrEmpty(phoneInput) ? emailInput : phoneInput;

            if (string.IsNullOrEmpty(userInput))
            {
                ModelState.AddModelError("", "Por favor, insira um telefone ou email válido.");
                return View(); // Retorna à view de login em caso de erro
            }

            // Busca o cliente pelo email ou telefone
            var cliente = await _clienteRepository.GetByEmailOrPhoneAsync(userInput);

            if (cliente != null)
            {
                // Criando claims para armazenar informações do cliente
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, cliente.ClienteId.ToString()),
                    new Claim(ClaimTypes.Name, cliente.Nome),
                    new Claim(ClaimTypes.Email, cliente.Email ?? cliente.Telefone)
                };

                // Criar a identidade e o principal
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true // Define o cookie de autenticação como persistente
                };

                // Autenticar o usuário
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                // Redireciona para a tela de escolha (histórico ou novo agendamento)
                return RedirectToAction("MenuPrincipal", "Cliente");
            }
            else
            {
                // Cliente não encontrado
                ModelState.AddModelError("", "Cliente não encontrado.");
                return View(); // Retorna à view de login em caso de erro
            }
        }

        // Realiza o logout e remove a sessão de autenticação
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
