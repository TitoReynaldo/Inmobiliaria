using System;
using System.Collections.Generic;
using System.Linq;
using Excel.FinancialFunctions;
using Inmobiliaria.API.DTOs.Simulacion;

namespace Inmobiliaria.API.Services
{
    public class FinancialService : IFinancialService
    {
        public SimulacionResultDto CalcularSimulacion(SimulacionInputDto input)
        {
            var resultado = new SimulacionResultDto();

            decimal montoPrestamo = input.PrecioVivienda - (input.PrecioVivienda * (input.CuotaInicialPorcentaje / 100));

            if (input.AplicaBonoBuenPagador)
            {
                montoPrestamo -= 25700m;
            }
            if (input.AplicaBonoVerde)
            {
                montoPrestamo -= 5400m;
            }

            decimal gastosIniciales = input.CostesNotariales + input.CostesRegistrales + input.Tasacion + input.ComisionActivacion;

            if (input.DiasPorPeriodo <= 0) input.DiasPorPeriodo = 30;
            if (input.DiasPorAnio <= 0) input.DiasPorAnio = 360;
            decimal periodosPorAnio = (decimal)input.DiasPorAnio / input.DiasPorPeriodo;
            decimal tea = input.TipoTasa == "Nominal"
                ? ConvertirTasaNominalAEfectiva(input.TasaInteres / 100, periodosPorAnio)
                : input.TasaInteres / 100;

            resultado.TEA = tea;

            decimal tem = (decimal)Math.Pow((double)(1 + tea), (double)input.DiasPorPeriodo / input.DiasPorAnio) - 1;

            decimal tasaDesgravamen = input.SeguroDesgravamenMensual / 100m;
            decimal montoSeguroRiesgo = input.ValorTasacion * (input.SeguroRiesgoMensual / 100m);

            resultado.Cronograma = GenerarCronogramaFrances(
                montoPrestamo,
                tem,
                input.PlazoMeses > 0 ? input.PlazoMeses : input.PlazoAnios * 12,
                input.TipoGracia,
                input.MesesGracia,
                tasaDesgravamen,
                montoSeguroRiesgo,
                input.Portes,
                input.GastosAdministracion
            );

            resultado.TotalAmortizacion = resultado.Cronograma.Sum(x => x.Amortizacion);
            resultado.TotalIntereses = resultado.Cronograma.Sum(x => x.Interes);
            resultado.TotalSeguros = resultado.Cronograma.Sum(x => x.SegDesgravamen + x.SeguroRiesgo);
            resultado.TotalPortes = resultado.Cronograma.Sum(x => x.Portes);
            resultado.TotalGastosAdmin = resultado.Cronograma.Sum(x => x.GastosAdministracion);

            var flujoCaja = new List<double>();
            flujoCaja.Add((double)-(montoPrestamo - gastosIniciales));
            flujoCaja.AddRange(resultado.Cronograma.Select(x => (double)x.CuotaTotal));

            double tirMensual = Financial.Irr(flujoCaja, 0.01);
            resultado.TIR = (decimal)tirMensual;

            resultado.TCEA = (decimal)(Math.Pow(1 + tirMensual, (double)input.DiasPorAnio / input.DiasPorPeriodo) - 1);

            double cokAnual = (double)(input.TasaDescuento / 100m);
            double cokMensual = Math.Pow(1 + cokAnual, (double)input.DiasPorPeriodo / input.DiasPorAnio) - 1;

            resultado.VAN = (decimal)(Financial.Npv(cokMensual, flujoCaja.Skip(1)) + flujoCaja[0]);

            return resultado;
        }

        private decimal ConvertirTasaNominalAEfectiva(decimal tna, decimal periodosPorAnio)
        {
            return (decimal)(Math.Pow((double)(1 + tna / periodosPorAnio), (double)periodosPorAnio) - 1);
        }

        private List<DetalleCronogramaDto> GenerarCronogramaFrances(
            decimal saldoCapital,
            decimal tasaMensual,
            int totalCuotas,
            string tipoGracia,
            int mesesGracia,
            decimal tasaDesgravamen,
            decimal montoSeguroRiesgo,
            decimal portes,
            decimal gastosAdmin)
        {
            var cronograma = new List<DetalleCronogramaDto>();
            int cuotaActual = 1;

            for (int i = 0; i < mesesGracia; i++)
            {
                decimal interes = saldoCapital * tasaMensual;
                decimal segDesgravamen = saldoCapital * tasaDesgravamen;

                if (tipoGracia == "Total")
                {
                    saldoCapital += interes;

                    cronograma.Add(new DetalleCronogramaDto
                    {
                        NroCuota = cuotaActual++,
                        SaldoInicial = saldoCapital - interes,
                        Interes = interes,
                        Amortizacion = 0m,
                        SegDesgravamen = segDesgravamen,
                        SeguroRiesgo = montoSeguroRiesgo,
                        SegInmueble = montoSeguroRiesgo,
                        Portes = portes,
                        GastosAdministracion = gastosAdmin,
                        CuotaTotal = 0m,
                        SaldoFinal = saldoCapital,
                        TasaPeriodo = tasaMensual
                    });
                }
                else if (tipoGracia == "Parcial")
                {
                    decimal cuotaPagar = interes + segDesgravamen + montoSeguroRiesgo + portes + gastosAdmin;

                    cronograma.Add(new DetalleCronogramaDto
                    {
                        NroCuota = cuotaActual++,
                        SaldoInicial = saldoCapital,
                        Interes = interes,
                        Amortizacion = 0m,
                        SegDesgravamen = segDesgravamen,
                        SeguroRiesgo = montoSeguroRiesgo,
                        SegInmueble = montoSeguroRiesgo,
                        Portes = portes,
                        GastosAdministracion = gastosAdmin,
                        CuotaTotal = cuotaPagar,
                        SaldoFinal = saldoCapital,
                        TasaPeriodo = tasaMensual
                    });
                }
            }

            int cuotasRestantes = totalCuotas - mesesGracia;
            if (cuotasRestantes > 0)
            {
                double i = (double)(tasaMensual + tasaDesgravamen);
                double factor = Math.Pow(1 + i, cuotasRestantes);
                decimal cuotaFija = saldoCapital * (decimal)((i * factor) / (factor - 1));

                for (int j = 0; j < cuotasRestantes; j++)
                {
                    decimal interes = saldoCapital * tasaMensual;
                    decimal segDesgravamen = saldoCapital * tasaDesgravamen;

                    decimal amortizacion = cuotaFija - interes - segDesgravamen;

                    decimal cuotaTotal = cuotaFija + montoSeguroRiesgo + portes + gastosAdmin;
                    decimal saldoFinal = saldoCapital - amortizacion;

                    cronograma.Add(new DetalleCronogramaDto
                    {
                        NroCuota = cuotaActual++,
                        SaldoInicial = saldoCapital,
                        Interes = interes,
                        Amortizacion = amortizacion,
                        SegDesgravamen = segDesgravamen,
                        SeguroRiesgo = montoSeguroRiesgo,
                        SegInmueble = montoSeguroRiesgo,
                        Portes = portes,
                        GastosAdministracion = gastosAdmin,
                        CuotaTotal = cuotaTotal,
                        SaldoFinal = saldoFinal < 0 ? 0 : saldoFinal,
                        TasaPeriodo = tasaMensual
                    });

                    saldoCapital = saldoFinal;
                }
            }

            return cronograma;
        }
    }
}