using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Inmobiliaria.API.Models;
using Inmobiliaria.API.DTOs.Simulacion;
using Inmobiliaria.API.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Inmobiliaria.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SimulacionesController : ControllerBase
    {
        private readonly InmobiliariaContext _context;
        private readonly IFinancialService _financialService;
        private readonly ILogger<SimulacionesController> _logger;

        public SimulacionesController(InmobiliariaContext context, IFinancialService financialService, ILogger<SimulacionesController> logger)
        {
            _context = context;
            _financialService = financialService;
            _logger = logger;
        }

        [HttpPost("calcular")]
        public async Task<IActionResult> CalcularYGuardarSimulacion([FromBody] SimulacionInputDto input)
        {
            if (!ModelState.IsValid)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("\n[ERROR DE VALIDACIÓN DTO] " + JsonSerializer.Serialize(ModelState));
                Console.ResetColor();
                return BadRequest(ModelState);
            }

            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int usuarioId))
                {
                    return Unauthorized("Token inválido o expirado.");
                }

                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

                if (cliente == null)
                {
                    cliente = new Cliente
                    {
                        UsuarioId = usuarioId,
                        Dni = usuarioId.ToString().PadLeft(8, '0'),
                        Nombres = User.FindFirst(ClaimTypes.Name)?.Value ?? "Cliente Demo",
                        Apellidos = "Demo",
                        SueldoMensual = 5000m,
                        FechaNacimiento = new DateOnly(1990, 1, 1)
                    };
                    _context.Clientes.Add(cliente);
                    await _context.SaveChangesAsync();
                }

                var resultado = await _financialService.CalcularSimulacion(input);
                resultado.Moneda = input.Moneda;

                var propiedadDefecto = await _context.Propiedades.FirstOrDefaultAsync()
                                       ?? throw new Exception("No hay propiedades registradas en BD.");
                var configDefecto = await _context.Configfinancieras.FirstOrDefaultAsync()
                                    ?? throw new Exception("No hay configuraciones financieras en BD.");

                using var transaction = await _context.Database.BeginTransactionAsync();

                var simulacionDb = new Simulacione
                {
                    ClienteId = cliente.ClienteId,
                    PropiedadId = propiedadDefecto.PropiedadId,
                    ConfigId = configDefecto.ConfigId,
                    PrecioVenta = input.PrecioVivienda,
                    CuotaInicial = input.CuotaInicial,
                    MontoPrestamo = input.PrecioVivienda - input.CuotaInicial,
                    PlazoMeses = input.PlazoMeses > 0 ? input.PlazoMeses : input.PlazoAnios * 12,
                    TipoGracia = input.TipoGracia,
                    MesesGracia = input.MesesGracia,
                    TasaEfectivaAnual = resultado.TEA,
                    Tcea = resultado.TCEA,
                    Van = resultado.VAN,
                    Tir = resultado.TIR,
                    FechaSimulacion = DateTime.Now,
                    Moneda = input.Moneda
                };

                _context.Simulaciones.Add(simulacionDb);
                await _context.SaveChangesAsync();

                var detallesDb = resultado.Cronograma.Select(d => new Detallecronograma
                {
                    SimulacionId = simulacionDb.SimulacionId,
                    NroCuota = d.NroCuota,
                    TasaEfectivaPeriodo = d.TasaPeriodo,
                    SaldoInicial = d.SaldoInicial,
                    Interes = d.Interes,
                    Amortizacion = d.Amortizacion,
                    Cuota = d.Amortizacion + d.Interes,
                    SeguroDesgravamen = d.SegDesgravamen,
                    SeguroInmueble = d.SeguroRiesgo, // Corrected mapping
                    CuotaTotal = d.CuotaTotal,
                    SaldoFinal = d.SaldoFinal
                }).ToList();

                _context.Detallecronogramas.AddRange(detallesDb);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(resultado);
            }
            catch (InvalidOperationException ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n[ADVERTENCIA DE NEGOCIO] {ex.Message}\nPayload: {JsonSerializer.Serialize(input)}\n");
                Console.ResetColor();
                return BadRequest(new { message = "Inconsistencia matemática en la simulación", detalle = ex.Message });
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[ERROR CRÍTICO] {ex.Message}\nStackTrace: {ex.StackTrace}\nPayload: {JsonSerializer.Serialize(input)}\n");
                Console.ResetColor();
                return StatusCode(500, new { message = "Error interno del servidor.", detalle = ex.Message });
            }
        }

        [HttpGet("historial")]
        public async Task<IActionResult> ObtenerHistorial()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int usuarioId))
            {
                return Unauthorized("Token inválido.");
            }

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
            if (cliente == null)
            {
                return Ok(new List<object>());
            }

            var historial = await _context.Simulaciones
                .Where(s => s.ClienteId == cliente.ClienteId)
                .OrderByDescending(s => s.FechaSimulacion)
                .Select(s => new
                {
                    s.SimulacionId,
                    s.FechaSimulacion,
                    s.PrecioVenta,
                    s.MontoPrestamo,
                    s.Tcea,
                    s.Van,
                    s.Tir,
                    Plazo = s.PlazoMeses,
                    Moneda = s.Moneda ?? "PEN"
                })
                .ToListAsync();

            return Ok(historial);
        }
    }
}
