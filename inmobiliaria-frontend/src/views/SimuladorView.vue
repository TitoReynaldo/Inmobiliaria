<script setup>
import { computed } from 'vue'
import { useSimulacionStore } from '../stores/simulacionStore'
import { Chart as ChartJS, ArcElement, Tooltip, Legend } from 'chart.js'
import { Doughnut } from 'vue-chartjs'

ChartJS.register(ArcElement, Tooltip, Legend)

const simulacionStore = useSimulacionStore()

if (!simulacionStore.input.precioVivienda) simulacionStore.input.precioVivienda = 250000
if (!simulacionStore.input.cuotaInicialPorcentaje) simulacionStore.input.cuotaInicialPorcentaje = 20
if (!simulacionStore.input.tasaInteres) simulacionStore.input.tasaInteres = 10.8
if (!simulacionStore.input.plazoMeses) simulacionStore.input.plazoMeses = 240

const plazoAnios = computed({
  get: () => Number((simulacionStore.input.plazoMeses / 12).toFixed(2)),
  set: (newVal) => {
    if (newVal >= 0) {
      simulacionStore.input.plazoMeses = Math.round(newVal * 12)
    }
  }
})

const calcularSimulacion = async () => {
  await simulacionStore.calcular()
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

const formatCurrency = (value, currencyCode = simulacionStore.input.moneda) => {
  if (value === undefined || value === null) {
    return currencyCode === 'USD' ? '$ 0.00' : 'S/ 0.00'
  }
  return new Intl.NumberFormat('es-PE', {
    style: 'currency',
    currency: currencyCode === 'USD' ? 'USD' : 'PEN',
    minimumFractionDigits: 2,
  }).format(value)
}

const formatRate = (value) => {
  if (value === undefined || value === null) return '0.000000000000000%'
  return new Intl.NumberFormat('es-PE', {
    style: 'percent',
    minimumFractionDigits: 15,
    maximumFractionDigits: 15,
  }).format(value)
}
</script>

<template>
  <div class="simulador-container">
    <section class="panel input-panel">
      <h2 class="panel-title">Configuración del Crédito (Modelo BCP)</h2>

      <div class="currency-toggle-container">
        <label class="toggle-main-label">Moneda de la Operación</label>
        <div class="currency-toggle-group">
          <button 
            type="button"
            class="toggle-btn" 
            :class="{ active: simulacionStore.input.moneda === 'PEN' }"
            @click="simulacionStore.input.moneda = 'PEN'"
          >
            <span class="flag">🇵🇪</span> Soles
          </button>
          <button 
            type="button"
            class="toggle-btn" 
            :class="{ active: simulacionStore.input.moneda === 'USD' }"
            @click="simulacionStore.input.moneda = 'USD'"
          >
            <span class="flag">🇺🇸</span> Dólares
          </button>
        </div>
      </div>

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
                <label title="Tasa Efectiva Anual (Costo del préstamo).">TEA (%)</label>
                <input
                  v-model.number="simulacionStore.input.tasaInteres"
                  type="number"
                  step="0.000000000000001"
                  required
                />
              </div>
            </div>

            <div class="form-row">
              <div class="form-group">
                <label
                  title="Cantidad de días considerados para un periodo de facturación (usualmente 30)."
                  >Días x Periodo</label
                >
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
                <select v-model="simulacionStore.input.tipoGracia" class="form-control">
                  <option value="Sin Gracia">Sin Gracia</option>
                  <option value="Parcial">Gracia Parcial (Paga Interés)</option>
                  <option value="Total">Gracia Total (Capitaliza)</option>
                </select>
              </div>
              <div class="form-group" v-if="simulacionStore.input.tipoGracia !== 'Sin Gracia'">
                <label title="Cantidad de meses que dura el periodo de gracia."
                  >Meses de Gracia</label
                >
                <input
                  v-model.number="simulacionStore.input.mesesGracia"
                  type="number"
                  min="1"
                  max="24"
                  required
                />
              </div>
            </div>
          </fieldset>

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
              <div class="form-group">
                <label title="Comisión que cobra el banco por desembolsar el préstamo."
                  >Comisión Activación {{ simulacionStore.input.moneda === 'USD' ? '($)' : '(S/)' }}</label
                >
                <input
                  v-model.number="simulacionStore.input.comisionActivacion"
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

          <fieldset class="form-fieldset">
            <legend>Costo de Oportunidad</legend>
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

            <div class="action-container">
              <button type="submit" class="btn-submit" :disabled="simulacionStore.loading">
                {{ simulacionStore.loading ? 'Procesando...' : 'Generar Simulación' }}
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

      <div class="results-grid">
        <div class="kpis-and-summary">
          <div class="kpi-container">
            <div class="kpi-box">
              <span class="kpi-label">TCEA</span>
              <span class="kpi-value highlight">{{
                formatRate(simulacionStore.resultado.tcea)
              }}</span>
            </div>
            <div class="kpi-box">
              <span class="kpi-label">Costo Financiero Actualizado (VAN)</span>
              <span class="kpi-value">{{
                formatCurrency(Math.abs(simulacionStore.resultado.van || 0))
              }}</span>
            </div>
            <div class="kpi-box">
              <span class="kpi-label">TIR Mensual</span>
              <span class="kpi-value">{{ formatRate(simulacionStore.resultado.tir) }}</span>
            </div>
          </div>

          <div class="summary-box mt-3">
            <p>
              <strong>Total Intereses:</strong><br />{{
                formatCurrency(simulacionStore.resultado.totalIntereses)
              }}
            </p>
            <p>
              <strong>Total Seguros:</strong><br />{{
                formatCurrency(simulacionStore.resultado.totalSeguros)
              }}
            </p>
            <p>
              <strong>Total a Pagar:</strong><br />{{
                formatCurrency(
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
              <th>Gastos y Comis.</th>
              <th>Cuota Total</th>
              <th>Saldo Final</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="fila in simulacionStore.resultado.cronograma" :key="fila.nroCuota">
              <td>{{ fila.nroCuota }}</td>
              <td>{{ formatRate(fila.tasaPeriodo) }}</td>
              <td>{{ formatCurrency(fila.saldoInicial) }}</td>
              <td>{{ formatCurrency(fila.amortizacion) }}</td>
              <td>{{ formatCurrency(fila.interes) }}</td>
              <td>{{ formatCurrency(fila.segDesgravamen) }}</td>
              <td>{{ formatCurrency(fila.seguroRiesgo || fila.segInmueble) }}</td>
              <td>{{ formatCurrency((fila.portes || 0) + (fila.gastosAdministracion || 0)) }}</td>
              <td class="col-cuota">{{ formatCurrency(fila.cuotaTotal) }}</td>
              <td>{{ formatCurrency(fila.saldoFinal) }}</td>
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
}

.currency-toggle-container {
  margin-bottom: 2rem;
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.toggle-main-label {
  font-weight: 700;
  font-size: 1.1rem;
  color: var(--text-title);
  border-bottom: none !important;
  cursor: default !important;
}

.currency-toggle-group {
  display: flex;
  background: var(--kpi-bg);
  padding: 0.4rem;
  border-radius: 12px;
  border: 1px solid var(--border-color);
  width: fit-content;
}

.toggle-btn {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.6rem 1.5rem;
  border: none;
  background: transparent;
  color: var(--text-muted);
  font-weight: 600;
  font-size: 1rem;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.3s ease;
}

.toggle-btn .flag {
  font-size: 1.25rem;
}

.toggle-btn:hover {
  color: var(--text-color);
}

.toggle-btn.active {
  background: var(--primary);
  color: white;
  box-shadow: 0 4px 12px rgba(var(--primary-rgb), 0.3);
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
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 1.5rem;
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
  margin-bottom: 1.25rem;
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

.helper-text {
  font-size: 0.8rem;
  color: var(--text-muted);
  display: block;
  margin-top: 0.3rem;
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