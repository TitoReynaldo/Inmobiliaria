# 🏦 Inmobiliaria API & Simulador Hipotecario

Sistema Fullstack de simulación financiera hipotecaria basado en el Método de Amortización Francés. Permite proyectar cronogramas de pago exactos, gestionar subsidios estatales y mantener un historial de auditoría inmutable.

##  Stack Tecnológico

* **Frontend:** Vue.js 3, Vite, Pinia (Gestor de Estado), TailwindCSS.
* **Backend:** C# .NET 10, ASP.NET Core Web API, Entity Framework Core.
* **Base de Datos:** MySQL 8.0+ (Estructura Relacional Normalizada).

##  Características Core

* **Motor Matemático:** Cálculo preciso de Tasa de Coste Efectivo Anual (TCEA), Valor Actual Neto (VAN) y Tasa Interna de Retorno (TIR).
* **Sistema Multidivisa:** Motor reactivo para cálculos y persistencia bidireccional en Soles (PEN) y Dólares (USD).
* **Estructura de Cuotas:** Integración algorítmica de Seguro de Desgravamen, Seguro de Inmueble, Gastos Notariales, Registrales, de Activación y Portes.
* **Periodos de Gracia:** Soporte paramétrico para configuración de Gracia Total o Gracia Parcial.
* **Gestión de Subsidios:** Evaluación y aplicación automática de Bono Verde y Bono del Buen Pagador basados en rangos de tasación.
* **Auditoría Criptográfica:** Trazabilidad de cada simulación vinculada a la sesión del usuario.
* **Amortizaciones Extraordinarias y Pagos Anticipados:** * El sistema permite la inyección de capital extraordinario en cualquier periodo del cronograma, reduciendo el saldo deudor de forma inmediata.
* **Reducción de Cuota:** El motor ejecuta un recálculo dinámico de la anualidad (R) manteniendo el plazo original, lo que disminuye la carga financiera mensual del cliente.
* **Reducción de Plazo:** El algoritmo mantiene la cuota constante pero liquida el capital más rápido, recalculando la fecha de término del crédito y generando un ahorro significativo en intereses totales.
* **Cierre Automático:** Validación lógica que detecta si el pago extraordinario cubre la totalidad de la deuda, forzando un cierre en cero exacto y finalizando el cronograma de forma automática.

##  Guía de Despliegue Local

### 1. Motor de Base de Datos (MySQL)
1.  Localizar el script de aprovisionamiento en: `database/InmobiliariaDB.sql`.
2.  Ejecutar el script en su servidor MySQL local para generar el esquema `InmobiliariaDB` e inyectar los datos semilla de configuración financiera TRAS la instalacion.

### 2. Capa Backend (.NET 10)

El núcleo del sistema expone la lógica de negocio mediante una API REST protegida por tokens e interactúa con la base de datos mediante Entity Framework Core.

**Dependencias Core (NuGet):**
* `Pomelo.EntityFrameworkCore.MySql` (v9.0.0): Abstracción ORM nativa y de alto rendimiento.
* `Microsoft.AspNetCore.Authentication.JwtBearer` & `System.IdentityModel.Tokens.Jwt`: Infraestructura de seguridad, autorización y validación de sesiones.
* `Microsoft.AspNetCore.OpenApi`: Especificación estandarizada de endpoints.

**Instrucciones de Despliegue:**
1. Navegar al directorio del núcleo: `cd Inmobiliaria.API/Inmobiliaria.API`
2. Inyectar credenciales de base de datos modificando el nodo `DefaultConnection` dentro del archivo `appsettings.json`.
3. Restaurar los paquetes descritos y desplegar el servidor Kestrel:
   ```bash
   dotnet restore
   dotnet run

### 3. Capa Frontend (Vue 3)

Aplicación de Página Única (SPA) optimizada con Vite, diseñada para la reactividad de cálculos financieros, visualización gráfica y exportación documental.

**Dependencias Core (NPM):**
* `vue` (Beta) & `vue-router` (v5.0.2): Núcleo reactivo y enrutamiento estructural.
* `pinia` (v3.0.4): Gestión de estado global (Store) para la persistencia del simulador.
* `axios` (v1.13.5): Cliente HTTP asíncrono para el consumo de la API REST.
* `chart.js` (v4.5.1) & `vue-chartjs` (v5.3.3): Motor de renderizado dinámico para dashboards y gráficos financieros.
* `jspdf` (v4.2.0) & `jspdf-autotable` (v5.0.7): Sistema de exportación de cronogramas y reportes de auditoría en formato PDF.

**Instrucciones de Despliegue:**
1. Navegar al directorio del cliente: `cd inmobiliaria-frontend`
2. Instalar el árbol de dependencias e inicializar el servidor Vite:
   ```bash
   npm install
   npm run dev

##  Arquitectura y Estándares
* Diseño API RESTful estricto (Controladores, DTOs y Servicios aislados).
* Inyección de Dependencias (DI) nativa de ASP.NET Core.
* Gestión de estado centralizada para flujos de datos asíncronos en UI.
