<script setup>
import { computed, ref } from 'vue'
import { useSimulacionStore } from '../stores/simulacionStore'
import { Chart as ChartJS, ArcElement, Tooltip, Legend } from 'chart.js'
import { Doughnut } from 'vue-chartjs'

ChartJS.register(ArcElement, Tooltip, Legend)

const simulacionStore = useSimulacionStore()

if (!simulacionStore.input.precioVivienda) simulacionStore.input.precioVivienda = 250000
if (!simulacionStore.input.cuotaInicialPorcentaje) simulacionStore.input.cuotaInicialPorcentaje = 20
if (!simulacionStore.input.tasaInteres) simulacionStore.input.tasaInteres = 10.8
if (!simulacionStore.input.tipoTasa) simulacionStore.input.tipoTasa = 'Efectiva'
if (!simulacionStore.input.plazoMeses) simulacionStore.input.plazoMeses = 240
if (!simulacionStore.input.tipoPrepago) simulacionStore.input.tipoPrepago = 'ReducirCuota'
if (!simulacionStore.input.pagosAnticipados) simulacionStore.input.pagosAnticipados = {}
if (!simulacionStore.input.mesesPorCuota) simulacionStore.input.mesesPorCuota = 1
if (simulacionStore.input.incrementoTasaFutura === undefined) simulacionStore.input.incrementoTasaFutura = 0
if (simulacionStore.input.cuotaInicioAjuste === undefined) simulacionStore.input.cuotaInicioAjuste = 0

const plazoAnios = computed({
  get: () => Number((simulacionStore.input.plazoMeses / 12).toFixed(2)),
  set: (newVal) => {
    if (newVal >= 0) {
      simulacionStore.input.plazoMeses = Math.round(newVal * 12)
    }
  }
})

const totalCuotasCalculadas = computed(() => {
  const mesesTotales = simulacionStore.input.plazoMeses || 0
  const mesesPorCuota = simulacionStore.input.mesesPorCuota || 1
  return Math.ceil(mesesTotales / mesesPorCuota)
})

const alertaGraciaRiesgo = computed(() => {
  const periodos = simulacionStore.input.periodosGracia
  if (!periodos || periodos <= 0) return null

  const mesesPorCuota = simulacionStore.input.mesesPorCuota || 1
  const mesesReales = periodos * mesesPorCuota

  const bonoBBP = simulacionStore.input.aplicaBonoBuenPagador
  const bonoVerde = simulacionStore.input.aplicaBonoVerde
  const bonoTechoPropio = simulacionStore.input.aplicaTechoPropio

  const limiteMeses = (bonoBBP || bonoVerde || bonoTechoPropio) ? 12 : 6

  if (mesesReales > limiteMeses) {
    return `⚠️ Equivale a ${mesesReales} meses. Excede el límite bancario (${limiteMeses} meses).`
  }

  return null
})

const nuevoPrepagoCuota = ref(null)
const nuevoPrepagoMonto = ref(null)

const agregarPrepago = () => {
  if (nuevoPrepagoCuota.value && nuevoPrepagoMonto.value > 0) {
    simulacionStore.input.pagosAnticipados[nuevoPrepagoCuota.value] = nuevoPrepagoMonto.value
    nuevoPrepagoCuota.value = null
    nuevoPrepagoMonto.value = null
  }
}

const eliminarPrepago = (cuota) => {
  delete simulacionStore.input.pagosAnticipados[cuota]
}

const calcularSimulacion = async () => {
  await simulacionStore.calcular()
}

const limpiarFormulario = () => {
  simulacionStore.input.precioVivienda = null;
  simulacionStore.input.valorTasacion = null;
  simulacionStore.input.cuotaInicialPorcentaje = null;
  simulacionStore.input.tasaInteres = null;
  simulacionStore.input.tipoTasa = 'Efectiva';
  simulacionStore.input.plazoMeses = null;
  simulacionStore.input.mesesPorCuota = 1;
  simulacionStore.input.diasPorPeriodo = 30;
  simulacionStore.input.diasPorAnio = 360;
  simulacionStore.input.tipoGracia = 'Sin Gracia';
  simulacionStore.input.periodosGracia = 0;
  simulacionStore.input.costesNotariales = 0;
  simulacionStore.input.costesRegistrales = 0;
  simulacionStore.input.tasacion = 0;
  simulacionStore.input.portes = 0;
  simulacionStore.input.gastosAdministracion = 0;
  simulacionStore.input.seguroDesgravamenMensual = 0;
  simulacionStore.input.seguroRiesgoMensual = 0;
  simulacionStore.input.incrementoTasaFutura = 0;
  simulacionStore.input.cuotaInicioAjuste = 0;
  simulacionStore.input.tipoPrepago = 'ReducirCuota';
  simulacionStore.input.pagosAnticipados = {};
  simulacionStore.input.tasaDescuento = 0;
  simulacionStore.input.aplicaBonoBuenPagador = false;
  simulacionStore.input.aplicaBonoVerde = false;
  simulacionStore.input.aplicaTechoPropio = false;
  simulacionStore.input.moneda = 'PEN';
  simulacionStore.resultado = null;
  simulacionStore.errorMsg = '';
}

const chartData = computed(() => {
  const res = simulacionStore.resultado
  if (!res) return null

  return {
    labels: ['Capital (Amortización)', 'Intereses', 'Seguros y Gastos'],
    datasets: [
      {
        backgroundColor: ['#10b981', '#f59e0b', '#3b82f6'],
        data: [
          res.totalAmortizacion || 0,
          res.totalIntereses || 0,
          (res.totalSeguros || 0) + (res.totalPortes || 0) + (res.totalGastosAdmin || 0),
        ],
        borderWidth: 0,
      },
    ],
  }
})

const chartOptions = computed(() => {
  return {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: false,
      },
    },
  }
})
//TRAS
const formatMonedaNormal = (value, currencyCode = simulacionStore.input.moneda) => {
  if (value === undefined || value === null) {
    return currencyCode === 'USD' ? '$ 0.00' : 'S/ 0.00'
  }
  return `${currencyCode === 'USD' ? '$' : 'S/'} ${Number(value).toFixed(2)}`
}

const formatMonedaInteres = (value, currencyCode = simulacionStore.input.moneda) => {
  if (value === undefined || value === null) {
    return currencyCode === 'USD' ? '$ 0.000000000000000' : 'S/ 0.000000000000000'
  }
  return `${currencyCode === 'USD' ? '$' : 'S/'} ${Number(value).toFixed(15)}`
}

const formatTasa = (value) => {
  if (value === undefined || value === null) return '0.000000000000000%'
  return `${Number(value * 100).toFixed(15)}%`
}
</script>

<template>
  <div class="simulador-container">
    <section class="panel input-panel">
      <h2 class="panel-title">Configuración del Crédito (Modelo BCP)</h2>

      <form @submit.prevent="calcularSimulacion" class="simulador-form">
        <div class="fieldsets-grid">
          <fieldset class="form-fieldset">
            <legend>Datos del Préstamo</legend>
            <div class="form-row">
              <div class="form-group">
                <label title="Monto total acordado para la compra del inmueble."
                  >Precio de Venta {{ simulacionStore.input.moneda === 'USD' ? '($)' : '(S/)' }}</label
                >
                <input
                  v-model.number="simulacionStore.input.precioVivienda"
                  type="number"
                  step="0.01"
                  required
                  min="1"
                />
              </div>
              <div class="form-group">
                <label title="Valor asignado al inmueble por un perito tasador oficial."
                  >Valor de Tasación {{ simulacionStore.input.moneda === 'USD' ? '($)' : '(S/)' }}</label
                >
                <input
                  v-model.number="simulacionStore.input.valorTasacion"
                  type="number"
                  step="0.01"
                  required
                  min="1"
                />
              </div>
            </div>

            <div class="form-row">
              <div class="form-group">
                <label title="Porcentaje del valor del inmueble que se paga al inicio."
                  >Cuota Inicial (%)</label
                >
                <input
                  v-model.number="simulacionStore.input.cuotaInicialPorcentaje"
                  type="number"
                  step="0.01"
                  required
                  min="0"
                  max="100"
                />
              </div>
              <div class="form-group">
                <label title="Tipo de Tasa">Tipo de Tasa</label>
                <select v-model="simulacionStore.input.tipoTasa" class="form-control" style="width: 100%; padding: 0.75rem; border: 1px solid var(--border-color); border-radius: 6px; background: var(--input-bg); color: var(--text-color);">
                  <option value="Efectiva">Efectiva</option>
                  <option value="Nominal">Nominal</option>
                </select>
              </div>
            </div>

            <div class="form-row">
              <div class="form-group">
                <label title="Tasa Anual (Costo del préstamo).">Tasa de Interés (%)</label>
                <input
                  v-model.number="simulacionStore.input.tasaInteres"
                  type="number"
                  step="0.000000000000001"
                  required
                />
              </div>
              <div class="form-group">
                <label title="Frecuencia de Pago">Frecuencia de Pago</label>
                <select v-model.number="simulacionStore.input.mesesPorCuota" class="form-control" style="width: 100%; padding: 0.75rem; border: 1px solid var(--border-color); border-radius: 6px; background: var(--input-bg); color: var(--text-color);">
                  <option :value="1">Mensual</option>
                  <option :value="2">Bimestral</option>
                  <option :value="3">Trimestral</option>
                  <option :value="4">Cuatrimestral</option>
                  <option :value="6">Semestral</option>
                  <option :value="12">Anual</option>
                </select>
                <span class="helper-text" style="color: var(--text-muted);">
                  Total de cuotas proyectadas: <strong>{{ totalCuotasCalculadas }}</strong>
                </span>
              </div>
            </div>

            <div class="form-row">
              <div class="form-group">
                <label title="Cantidad de días considerados para un periodo de facturación (usualmente 30)."
                  >Días x Periodo Base</label>
                <input
                  v-model.number="simulacionStore.input.diasPorPeriodo"
                  type="number"
                  required
                  min="1"
                />
              </div>
              <div class="form-group">
                <label title="Base de días en un año contable (usualmente 360).">Días x Año</label>
                <input
                  v-model.number="simulacionStore.input.diasPorAnio"
                  type="number"
                  required
                  min="1"
                />
              </div>
            </div>

            <div class="form-row">
              <div class="form-group">
                <label title="Duración total del préstamo expresada en meses.">Plazo (Meses)</label>
                <input
                  v-model.number="simulacionStore.input.plazoMeses"
                  type="number"
                  min="12"
                  max="360"
                  required
                />
              </div>
              <div class="form-group">
                <label title="Duración total del préstamo expresada en años.">Plazo (Años)</label>
                <input
                  v-model.number="plazoAnios"
                  type="number"
                  min="1"
                  max="30"
                  step="0.01"
                  required
                />
              </div>
            </div>

            <div class="form-row">
              <div class="form-group">
                <label title="Tiempo en el que no se paga capital o intereses."
                  >Periodo de Gracia</label
                >
                <select v-model="simulacionStore.input.tipoGracia" class="form-control" style="width: 100%; padding: 0.75rem; border: 1px solid var(--border-color); border-radius: 6px; background: var(--input-bg); color: var(--text-color);">
                  <option value="Sin Gracia">Sin Gracia</option>
                  <option value="Parcial">Gracia Parcial (Paga Interés)</option>
                  <option value="Total">Gracia Total (Capitaliza)</option>
                </select>
              </div>
              <div class="form-group" v-if="simulacionStore.input.tipoGracia !== 'Sin Gracia'">
                <label title="Cantidad de periodos que dura el periodo de gracia."
                  >Periodos de Gracia</label
                >
                <input
                  v-model.number="simulacionStore.input.periodosGracia"
                  type="number"
                  min="1"
                  max="360"
                  required
                />
                <small v-if="alertaGraciaRiesgo" class="helper-text" style="color: #d97706; font-weight: 600;">
                  {{ alertaGraciaRiesgo }}
                </small>
              </div>
            </div>
          </fieldset>

          <div class="fieldset-column">
            <fieldset class="form-fieldset">
              <legend>Costes/Gastos Iniciales</legend>
              <div class="form-row">
                <div class="form-group">
                  <label title="Gastos pagados al notario por las escrituras públicas."
                    >Costes Notariales {{ simulacionStore.input.moneda === 'USD' ? '($)' : '(S/)' }}</label
                  >
                  <input
                    v-model.number="simulacionStore.input.costesNotariales"
                    type="number"
                    step="0.01"
                    min="0"
                  />
                </div>
                <div class="form-group">
                  <label title="Gastos cobrados por Registros Públicos."
                    >Costes Registrales {{ simulacionStore.input.moneda === 'USD' ? '($)' : '(S/)' }}</label
                  >
                  <input
                    v-model.number="simulacionStore.input.costesRegistrales"
                    type="number"
                    step="0.01"
                    min="0"
                  />
                </div>
              </div>
              <div class="form-row">
                <div class="form-group">
                  <label title="Costo pagado al tasador del inmueble.">Tasación {{ simulacionStore.input.moneda === 'USD' ? '($)' : '(S/)' }}</label>
                  <input
                    v-model.number="simulacionStore.input.tasacion"
                    type="number"
                    step="0.01"
                    min="0"
                  />
                </div>
              </div>
            </fieldset>

            <fieldset class="form-fieldset">
              <legend>Costes/Gastos Periódicos</legend>
              <div class="form-row">
                <div class="form-group">
                  <label title="Costo de envío de estados de cuenta o información física."
                    >Portes {{ simulacionStore.input.moneda === 'USD' ? '($)' : '(S/)' }}</label
                  >
                  <input
                    v-model.number="simulacionStore.input.portes"
                    type="number"
                    step="0.01"
                    min="0"
                  />
                </div>
                <div class="form-group">
                  <label title="Gastos mensuales administrativos del banco."
                    >Gastos Administración {{ simulacionStore.input.moneda === 'USD' ? '($)' : '(S/)' }}</label
                  >
                  <input
                    v-model.number="simulacionStore.input.gastosAdministracion"
                    type="number"
                    step="0.01"
                    min="0"
                  />
                </div>
              </div>
              <div class="form-row">
                <div class="form-group">
                  <label
                    title="Porcentaje mensual que asegura el saldo deudor en caso de fallecimiento."
                    >Seg. Desgravamen Mensual (%)</label
                  >
                  <input
                    v-model.number="simulacionStore.input.seguroDesgravamenMensual"
                    type="number"
                    step="0.000000000000001"
                    min="0"
                  />
                </div>
                <div class="form-group">
                  <label title="Porcentaje mensual que protege al inmueble contra siniestros."
                    >Seg. Riesgo Mensual (%)</label
                  >
                  <input
                    v-model.number="simulacionStore.input.seguroRiesgoMensual"
                    type="number"
                    step="0.000000000000001"
                    min="0"
                  />
                </div>
              </div>
            </fieldset>
          </div>

          <div class="fieldset-column">
            <fieldset class="form-fieldset">
              <legend>Escenario de Riesgo (Tasa Flexible)</legend>
              <div class="form-row">
                <div class="form-group">
                  <label title="Incremento en la tasa de interés anual a aplicar en el futuro">Incremento de Tasa (%)</label>
                  <input
                    v-model.number="simulacionStore.input.incrementoTasaFutura"
                    type="number"
                    step="0.000000000000001"
                  />
                </div>
                <div class="form-group">
                  <label title="Número de cuota a partir del cual se aplicará el incremento de tasa">Aplicar desde Cuota N°</label>
                  <input
                    v-model.number="simulacionStore.input.cuotaInicioAjuste"
                    type="number"
                    min="0"
                  />
                </div>
              </div>
            </fieldset>

            <fieldset class="form-fieldset">
              <legend>Amortizaciones Extraordinarias</legend>
              <div class="form-group">
                <label title="Estrategia al realizar un pago anticipado">Tipo de Prepago</label>
                <select v-model="simulacionStore.input.tipoPrepago" class="form-control" style="width: 100%; padding: 0.75rem; border: 1px solid var(--border-color); border-radius: 6px; background: var(--input-bg); color: var(--text-color);">
                  <option value="ReducirCuota">Reducir Cuota (Mismo Plazo)</option>
                  <option value="ReducirPlazo">Reducir Plazo (Misma Cuota)</option>
                </select>
              </div>

              <div class="form-row">
                <div class="form-group">
                  <label title="Número de la cuota en la que se hará el prepago">N° Cuota</label>
                  <input v-model.number="nuevoPrepagoCuota" type="number" min="1" />
                </div>
                <div class="form-group">
                  <label title="Monto adicional a amortizar">Monto {{ simulacionStore.input.moneda === 'USD' ? '($)' : '(S/)' }}</label>
                  <input v-model.number="nuevoPrepagoMonto" type="number" step="0.01" min="0" />
                </div>
              </div>
              <button type="button" @click="agregarPrepago" class="btn-secondary" style="margin-bottom: 1rem; padding: 0.5rem; border-radius: 4px; border: 1px solid var(--border-color); cursor:pointer; background: transparent; color: var(--text-color);">
                Añadir Prepago
              </button>

              <div class="prepagos-list" v-if="Object.keys(simulacionStore.input.pagosAnticipados).length > 0">
                <div v-for="(monto, cuota) in simulacionStore.input.pagosAnticipados" :key="cuota" class="prepago-item" style="display:flex; justify-content:space-between; margin-bottom: 0.5rem; font-size: 0.9rem; border-bottom: 1px solid var(--border-color); padding-bottom: 0.3rem;">
                  <span>Cuota {{ cuota }}: {{ formatMonedaNormal(monto) }}</span>
                  <button type="button" @click="eliminarPrepago(cuota)" style="color:red; background:none; border:none; cursor:pointer;">Eliminar</button>
                </div>
              </div>
            </fieldset>
          </div>
          <fieldset class="form-fieldset">
            <legend>Costo de Oportunidad y Acciones</legend>
            <div class="form-group">
              <label
                title="Tasa utilizada para descontar y traer al valor presente los flujos futuros."
                >Tasa de Descuento (%)</label
              >
              <input
                v-model.number="simulacionStore.input.tasaDescuento"
                type="number"
                step="0.000000000000001"
                min="0"
              />
            </div>

            <div class="subsidies">
              <label
                class="checkbox-label"
                title="Beneficio del Estado para complementar la cuota inicial."
              >
                <input v-model="simulacionStore.input.aplicaBonoBuenPagador" type="checkbox" />
                Bono Buen Pagador (BBP)
              </label>
              <label class="checkbox-label" title="Beneficio adicional para viviendas sostenibles.">
                <input v-model="simulacionStore.input.aplicaBonoVerde" type="checkbox" />
                Bono Mivivienda Sostenible
              </label>
            </div>

            <div class="form-group currency-pill-container">
              <label title="Moneda de la Operación">Moneda de la Operación</label>
              <div class="currency-pill-group">
                <button
                  type="button"
                  class="pill-btn"
                  :class="{ active: simulacionStore.input.moneda === 'PEN' }"
                  @click="simulacionStore.input.moneda = 'PEN'"
                >
                  Soles (S/)
                </button>
                <button
                  type="button"
                  class="pill-btn"
                  :class="{ active: simulacionStore.input.moneda === 'USD' }"
                  @click="simulacionStore.input.moneda = 'USD'"
                >
                  Dólares ($)
                </button>
              </div>
            </div>

            <div class="action-container">
              <button type="submit" class="btn-submit" :disabled="simulacionStore.loading">
                {{ simulacionStore.loading ? 'Procesando...' : 'Generar Simulación' }}
              </button>
              <button type="button" @click="limpiarFormulario" title="Restablecer todos los campos" style="margin-top: 0.75rem; width: 100%; padding: 0.5rem; background: transparent; border: 1px solid var(--border-color); color: var(--text-muted, #9ca3af); border-radius: 6px; font-size: 0.85rem; cursor: pointer; transition: all 0.2s;">
                Limpiar Datos
              </button>
            </div>

            <div v-if="simulacionStore.errorMsg" class="error-alert">
              {{ simulacionStore.errorMsg }}
            </div>
          </fieldset>
        </div>
      </form>
    </section>

    <section class="panel results-panel" v-if="simulacionStore.resultado">
      <h2 class="panel-title">Resultados de la Operación</h2>

      <div v-if="simulacionStore.resultado.advertenciaRiesgo" class="alert-warning" style="background-color: #fffbeb; color: #b45309; border: 1px solid #f59e0b; padding: 1rem; border-radius: 8px; margin-bottom: 1.5rem; font-weight: bold; text-align: center;">
        ⚠️ {{ simulacionStore.resultado.advertenciaRiesgo }}
      </div>

      <div class="results-grid">
        <div class="kpis-and-summary">
          <div class="kpi-container">
            <div class="kpi-box">
              <span class="kpi-label">TCEA</span>
              <span class="kpi-value highlight">{{
                formatTasa(simulacionStore.resultado.tcea)
              }}</span>
            </div>
            <div class="kpi-box kpi-box-van">
              <span class="kpi-label kpi-label-van">COSTO FINANCIERO ACTUALIZADO (VAN)</span>
              <span class="kpi-value kpi-value-van">{{
                formatMonedaNormal(Math.abs(simulacionStore.resultado.van || 0))
              }}</span>
            </div>
            <div class="kpi-box">
              <span class="kpi-label">TIR Mensual</span>
              <span class="kpi-value">{{ formatTasa(simulacionStore.resultado.tir) }}</span>
            </div>
          </div>

          <div class="summary-box mt-3">
            <p>
              <strong>Total Intereses:</strong><br />{{
                formatMonedaInteres(simulacionStore.resultado.totalIntereses)
              }}
            </p>
            <p>
              <strong>Total Seguros:</strong><br />{{
                formatMonedaNormal(simulacionStore.resultado.totalSeguros)
              }}
            </p>
            <p>
              <strong>Total a Pagar:</strong><br />{{
                formatMonedaNormal(
                  (simulacionStore.resultado.totalAmortizacion || 0) +
                    (simulacionStore.resultado.totalIntereses || 0) +
                    (simulacionStore.resultado.totalSeguros || 0),
                )
              }}
            </p>
          </div>
        </div>

        <div class="chart-container">
          <div class="chart-wrapper">
            <Doughnut v-if="chartData" :data="chartData" :options="chartOptions" />
          </div>
          <div class="custom-legend" v-if="chartData">
            <div v-for="(label, index) in chartData.labels" :key="index" class="legend-item">
              <span class="legend-color" :style="{ backgroundColor: chartData.datasets[0].backgroundColor[index] }"></span>
              <span class="legend-label">{{ label }}</span>
            </div>
          </div>
        </div>
      </div>
    </section>

    <section class="panel empty-panel" v-else>
      <p>
        Configure los parámetros y ejecute la simulación para visualizar los resultados analíticos.
      </p>
    </section>

    <section class="panel table-panel" v-if="simulacionStore.resultado">
      <h2 class="panel-title">Cronograma de Pagos (Método Francés)</h2>
      <div class="table-responsive">
        <table class="financial-table">
          <thead>
            <tr>
              <th>N° Cuota</th>
              <th>Tasa Período</th>
              <th>Saldo Inicial</th>
              <th>Amortización</th>
              <th>Interés</th>
              <th>Seg. Desgravamen</th>
              <th>Seg. Riesgo</th>
              <th>Gastos</th>
              <th>Cuota Total</th>
              <th>Saldo Final</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="fila in simulacionStore.resultado.cronograma" :key="fila.nroCuota">
              <td>{{ fila.nroCuota }}</td>
              <td>{{ formatTasa(fila.tasaPeriodo) }}</td>
              <td>{{ formatMonedaNormal(fila.saldoInicial) }}</td>
              <td>{{ formatMonedaNormal(fila.amortizacion) }}</td>
              <td>{{ formatMonedaInteres(fila.interes) }}</td>
              <td>{{ formatMonedaNormal(fila.segDesgravamen) }}</td>
              <td>{{ formatMonedaNormal(fila.seguroRiesgo || fila.segInmueble) }}</td>
              <td>{{ formatMonedaNormal((fila.portes || 0) + (fila.gastosAdministracion || 0)) }}</td>
              <td class="col-cuota">{{ formatMonedaNormal(fila.cuotaTotal) }}</td>
              <td>{{ formatMonedaNormal(fila.saldoFinal) }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>
  </div>
</template>

<style scoped>
.simulador-container {
  display: flex;
  flex-direction: column;
  gap: 2rem;
  max-width: 1400px;
  margin: 0 auto;
  width: 100%;
}

.panel {
  background: var(--panel-bg);
  border-radius: 12px;
  padding: 2rem;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
  border: 1px solid var(--border-color);
  transition:
    background 0.3s,
    border-color 0.3s;
}

.panel-title {
  font-size: 1.5rem;
  color: var(--text-title);
  margin-top: 0;
  margin-bottom: 2rem;
  border-bottom: 2px solid var(--border-color);
  padding-bottom: 0.75rem;
}

.fieldsets-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 1.5rem;
  align-items: start;
}

@media (min-width: 1200px) {
  .fieldsets-grid {
    grid-template-columns: repeat(4, 1fr);
  }
}

.fieldset-column {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  height: 100%;
}

.form-fieldset {
  border: 1px solid var(--fieldset-border);
  border-radius: 8px;
  padding: 1.25rem;
  transition: border-color 0.3s;
  display: flex;
  flex-direction: column;
  justify-content: flex-start;
}

.form-fieldset legend {
  font-weight: bold;
  font-size: 1.1rem;
  color: var(--text-title);
  padding: 0 0.5rem;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.25rem;
  align-items: flex-end;
}

.form-group {
  position: relative;
  display: flex;
  flex-direction: column;
  justify-content: flex-end;
  margin-bottom: 1.75rem;
}

.form-group input, .form-group select {
  margin-top: auto;
}

.helper-text {
  position: absolute;
  top: 100%;
  left: 0;
  width: 100%;
  margin-top: 0.3rem;
  line-height: 1.2;
  font-size: 0.8rem;
}

label {
  display: inline-block;
  font-size: 0.95rem;
  font-weight: 600;
  color: var(--text-color);
  margin-bottom: 0.5rem;
  border-bottom: 1px dotted var(--text-muted);
  cursor: help;
}

.subsidies {
  background: var(--kpi-bg);
  padding: 1rem;
  border-radius: 6px;
  border: 1px dashed var(--border-color);
  margin-bottom: 1.25rem;
}

.checkbox-label {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-weight: normal;
  cursor: pointer;
  border-bottom: none;
}

input[type='number'],
select {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid var(--border-color);
  border-radius: 6px;
  box-sizing: border-box;
  font-size: 1rem;
  background: var(--input-bg);
  color: var(--text-color);
  transition:
    background 0.3s,
    color 0.3s,
    border-color 0.3s;
}

/* Nuevos estilos para el toggle de moneda */
.currency-pill-container {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  margin-bottom: 1rem;
}

.currency-pill-group {
  display: flex;
  background: var(--input-bg);
  border: 1px solid var(--border-color);
  border-radius: 6px;
  overflow: hidden;
  width: 100%;
}

.pill-btn {
  padding: 0.75rem 1rem;
  border: none;
  background: transparent;
  color: var(--text-muted);
  font-size: 0.95rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
  flex: 1;
  text-align: center;
}

.pill-btn:hover {
  background: rgba(0,0,0,0.05);
  color: var(--text-color);
}

.pill-btn.active {
  background: var(--primary);
  color: white;
}

.action-container {
  margin-top: auto;
  padding-top: 1rem;
}

.btn-submit {
  width: 100%;
  background: var(--primary);
  color: #ffffff;
  border: none;
  padding: 1.1rem;
  font-weight: 700;
  font-size: 1.1rem;
  border-radius: 6px;
  cursor: pointer;
  transition: background 0.3s;
}

.btn-submit:hover:not(:disabled) {
  background: var(--primary-hover);
}
.btn-submit:disabled {
  background: var(--text-muted);
  cursor: wait;
}

.error-alert {
  margin-top: 1rem;
  color: #b91c1c;
  background: #fef2f2;
  padding: 0.75rem;
  border-radius: 6px;
  font-size: 0.85rem;
}

.results-grid {
  display: grid;
  grid-template-columns: 1.8fr 1fr;
  gap: 2rem;
  align-items: center;
}

@media (max-width: 992px) {
  .results-grid {
    grid-template-columns: 1fr;
  }
}

.kpi-container {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1.25rem;
  width: 100%;
}

.kpi-box {
  background: var(--kpi-bg);
  padding: 1.5rem 1rem;
  border-radius: 8px;
  text-align: center;
  border: 1px solid var(--border-color);
  transition:
    background 0.3s,
    border-color 0.3s;
  overflow: hidden;
  flex: 1;
  min-width: 0;
}

.kpi-label {
  display: block;
  font-size: 0.8rem;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  white-space: nowrap;
}

.kpi-label-van {
  font-size: clamp(0.7rem, 2vw, 0.85rem);
  line-height: 1.2;
  white-space: normal;
  text-align: center;
  margin-bottom: 0.2rem;
}

.kpi-value {
  display: block;
  font-size: clamp(0.85rem, 1.2vw, 1.1rem);
  font-weight: 700;
  color: var(--text-title);
  margin-top: 0.5rem;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.kpi-box-van {
  min-width: 180px;
}

.kpi-value-van {
  white-space: normal;
  word-break: break-all;
  overflow-wrap: anywhere;
  font-size: clamp(0.7rem, 1vw, 1rem);
}

.kpi-value.highlight {
  color: var(--primary);
}

.mt-3 {
  margin-top: 1.5rem;
}

.summary-box {
  background: var(--summary-bg);
  border: 1px solid var(--border-color);
  padding: 1.5rem;
  border-radius: 8px;
  display: flex;
  justify-content: space-around;
  font-size: 1.05rem;
  color: var(--text-title);
  transition:
    background 0.3s,
    border-color 0.3s;
}

.summary-box p {
  margin: 0;
  text-align: center;
}
.summary-box strong {
  color: var(--text-title);
  font-size: 0.95rem;
  font-weight: 800;
  text-transform: uppercase;
}

.chart-wrapper {
  height: 300px;
  display: flex;
  justify-content: center;
}

.custom-legend {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: 1rem;
  margin-top: 1.5rem;
}

.legend-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.legend-color {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  display: inline-block;
}

.legend-label {
  color: var(--text-color);
  font-size: 0.9rem;
  font-weight: 600;
}

.empty-panel {
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--text-muted);
  text-align: center;
  min-height: 150px;
  font-size: 1.1rem;
}

.table-responsive {
  overflow-x: auto;
  max-height: 600px;
}

.financial-table {
  width: 100%;
  border-collapse: collapse;
  font-family: monospace;
  font-size: 0.9rem;
}

.financial-table th {
  background: var(--table-head);
  color: var(--table-head-text);
  padding: 1rem 0.75rem;
  text-align: right;
  position: sticky;
  top: 0;
  z-index: 10;
  font-weight: 600;
}

.financial-table td {
  padding: 0.8rem 0.75rem;
  text-align: right;
  border-bottom: 1px solid var(--border-color);
  color: var(--text-color);
  white-space: nowrap;
  font-variant-numeric: tabular-nums;
}

.financial-table tbody tr:hover {
  background-color: var(--table-hover);
}

.col-cuota {
  font-weight: bold;
  color: var(--cuota-color);
  background-color: var(--cuota-bg);
}
</style>
