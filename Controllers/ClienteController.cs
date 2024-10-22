using BarberShop.Application.Services;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BarberShopMVC.Controllers
{
    public class ClienteController : Controller
    {
        private readonly IClienteService _clienteService;
        private readonly IServicoRepository _servicoRepository;
        private readonly IAgendamentoService _agendamentoService; // Adicionando o serviço de agendamento
        private readonly IBarbeiroService _barbeiroService; // Adicionando o serviço de barbeiros

        public ClienteController(IClienteService clienteService, IServicoRepository servicoRepository, IAgendamentoService agendamentoService, IBarbeiroService barbeiroService)
        {
            _clienteService = clienteService;
            _servicoRepository = servicoRepository;
            _agendamentoService = agendamentoService;
            _barbeiroService = barbeiroService;
        }

        // Exibe o Menu Principal com as opções de ver histórico e solicitar novo serviço
        public IActionResult MenuPrincipal()
        {
            return View();
        }

        // Exibe o histórico de agendamentos do cliente
        public async Task<IActionResult> Historico()
        {
            var clienteId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var agendamentos = await _clienteService.ObterHistoricoAgendamentosAsync(clienteId);
            return View("HistoricoAgendamentos", agendamentos);
        }

        // Exibe a tela de solicitação de serviços com os serviços disponíveis
        public async Task<IActionResult> SolicitarServico()
        {
            var servicos = await _servicoRepository.GetAllAsync(); // Obtendo todos os serviços
            return View("SolicitarServico", servicos); // Retorna a view com os serviços
        }

        // Método para listar barbeiros após o cliente escolher os serviços
        public async Task<IActionResult> EscolherBarbeiro()
        {
            var barbeiros = await _barbeiroService.ObterTodosBarbeirosAsync(); // Obtém todos os barbeiros
            return View("EscolherBarbeiro", barbeiros); // Retorna a view para escolha de barbeiros
        }

        // Retorna os horários disponíveis de um barbeiro específico
        public async Task<IActionResult> ObterHorariosDisponiveis(int barbeiroId, int duracaoTotal)
        {
            var horariosDisponiveis = await _agendamentoService.ObterHorariosDisponiveisAsync(barbeiroId, DateTime.Now, duracaoTotal);
            return Json(horariosDisponiveis); // Retorna os horários disponíveis em formato JSON
        }

        // Exibe todos os clientes
        public async Task<IActionResult> Index()
        {
            var clientes = await _clienteService.ObterTodosClientesAsync();
            return View(clientes);
        }

        // Exibe detalhes de um cliente específico
        public async Task<IActionResult> Details(int id)
        {
            var cliente = await _clienteService.ObterClientePorIdAsync(id);
            if (cliente == null) return NotFound();
            return View(cliente);
        }

        // Exibe a view de criação de cliente
        public IActionResult Create()
        {
            return View();
        }

        // Cria um novo cliente
        [HttpPost]
        public async Task<IActionResult> Create(Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                await _clienteService.AdicionarClienteAsync(cliente);
                return RedirectToAction(nameof(Index));
            }
            return View(cliente);
        }

        // Exibe a view de edição de cliente
        public async Task<IActionResult> Edit(int id)
        {
            var cliente = await _clienteService.ObterClientePorIdAsync(id);
            if (cliente == null) return NotFound();
            return View(cliente);
        }

        // Atualiza um cliente existente
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Cliente cliente)
        {
            if (id != cliente.ClienteId) return BadRequest();

            if (ModelState.IsValid)
            {
                await _clienteService.AtualizarClienteAsync(cliente);
                return RedirectToAction(nameof(Index));
            }
            return View(cliente);
        }

        // Exibe a view de exclusão de cliente
        public async Task<IActionResult> Delete(int id)
        {
            var cliente = await _clienteService.ObterClientePorIdAsync(id);
            if (cliente == null) return NotFound();
            return View(cliente);
        }

        // Exclui um cliente
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _clienteService.DeletarClienteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
