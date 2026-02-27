using Inmobiliaria.API.DTOs.Simulacion;

namespace Inmobiliaria.API.Services
{
    public interface IFinancialService
    {
        SimulacionResultDto CalcularSimulacion(SimulacionInputDto input);
    }
}