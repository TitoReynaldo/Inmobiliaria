import { ref } from 'vue'
import { defineStore } from 'pinia'
import { SimulacionService } from '../services/simulacionService'

export const useSimulacionStore = defineStore('simulacion', () => {
  const input = ref({
    moneda: 'PEN',
    precioVivienda: 250000,
    valorTasacion: 250000,
    cuotaInicialPorcentaje: 20,
    diasPorPeriodo: 30,
    diasPorAnio: 360,
    tasaInteres: 10.8,
    tipoTasa: 'Efectiva',
    plazoMeses: 240,
    tipoGracia: 'Sin Gracia',
    mesesGracia: 0,
    aplicaBonoBuenPagador: false,
    aplicaBonoVerde: false,

    costesNotariales: 150,
    costesRegistrales: 250,
    tasacion: 300,
    comisionActivacion: 0,

    portes: 3.5,
    gastosAdministracion: 10,
    seguroDesgravamenMensual: 0.049,
    seguroRiesgoMensual: 0.029,

    tasaDescuento: 12.5,
  })

  const resultado = ref(null)
  const loading = ref(false)
  const errorMsg = ref('')

  const calcular = async () => {
    loading.value = true
    errorMsg.value = ''
    resultado.value = null

    try {
      const payload = {
        ...input.value,
        moneda: input.value.moneda,
        precioVivienda: parseFloat(input.value.precioVivienda),
        valorTasacion: parseFloat(input.value.valorTasacion),
        cuotaInicialPorcentaje: parseFloat(input.value.cuotaInicialPorcentaje),
        cuotaInicial:
          parseFloat(input.value.precioVivienda) *
          (parseFloat(input.value.cuotaInicialPorcentaje) / 100),
        diasPorPeriodo: parseInt(input.value.diasPorPeriodo, 10),
        diasPorAnio: parseInt(input.value.diasPorAnio, 10),
        tasaInteres: parseFloat(input.value.tasaInteres),
        plazoMeses: parseInt(input.value.plazoMeses, 10),
        mesesGracia: parseInt(input.value.mesesGracia, 10),
        costesNotariales: parseFloat(input.value.costesNotariales),
        costesRegistrales: parseFloat(input.value.costesRegistrales),
        tasacion: parseFloat(input.value.tasacion),
        comisionActivacion: parseFloat(input.value.comisionActivacion),
        portes: parseFloat(input.value.portes),
        gastosAdministracion: parseFloat(input.value.gastosAdministracion),
        seguroDesgravamenMensual: parseFloat(input.value.seguroDesgravamenMensual),
        seguroRiesgoMensual: parseFloat(input.value.seguroRiesgoMensual),
        tasaDescuento: parseFloat(input.value.tasaDescuento),
      }

      console.log('Payload enviado:', payload)

      const response = await SimulacionService.calcular(payload)
      resultado.value = response.data
      return true
    } catch (error) {
      console.error('Error al calcular en Store:', error)
      const data = error.response?.data
      if (data) {
        errorMsg.value = [data.message, data.detalle].filter(Boolean).join(' - ')
      } else {
        errorMsg.value = 'Error al procesar la simulación.'
      }
      return false
    } finally {
      loading.value = false
    }
  }

  const cargarDesdeHistorial = async (resumen) => {
    console.log('Iniciando reconstrucción con datos del historial:', resumen)

    const precio = resumen.precioVenta || resumen.PrecioVenta || 0
    const prestamo = resumen.montoPrestamo || resumen.MontoPrestamo || 0
    const plazoMesesHist = resumen.plazo || resumen.Plazo || resumen.plazoMeses || 0

    input.value.moneda = resumen.moneda || resumen.Moneda || 'PEN'
    input.value.precioVivienda = precio
    if (precio > 0) {
      input.value.cuotaInicialPorcentaje = ((precio - prestamo) / precio) * 100
    }
    input.value.plazoMeses = plazoMesesHist > 0 ? plazoMesesHist : 240

    console.log('Input reconstruido, ejecutando cálculo motor...')
    await calcular()
  }

  const resetearResultados = () => {
    resultado.value = null
    errorMsg.value = ''
  }

  return {
    input,
    resultado,
    loading,
    errorMsg,
    calcular,
    cargarDesdeHistorial,
    resetearResultados,
  }
})