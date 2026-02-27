
DROP DATABASE IF EXISTS InmobiliariaDB;
CREATE DATABASE InmobiliariaDB;
USE InmobiliariaDB;

CREATE TABLE Usuarios (
    UsuarioID INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(50) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Rol ENUM('Admin', 'Cliente') DEFAULT 'Cliente',
    FechaRegistro DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE Clientes (
    ClienteID INT AUTO_INCREMENT PRIMARY KEY,
    UsuarioID INT,
    DNI VARCHAR(8) NOT NULL UNIQUE,
    Nombres VARCHAR(100) NOT NULL,
    Apellidos VARCHAR(100) NOT NULL,
    FechaNacimiento DATE NOT NULL,
    SueldoMensual DECIMAL(30, 15) NOT NULL,
    Email VARCHAR(100),
    FOREIGN KEY (UsuarioID) REFERENCES Usuarios(UsuarioID) ON DELETE CASCADE
);

CREATE TABLE Propiedades (
    PropiedadID INT AUTO_INCREMENT PRIMARY KEY,
    Direccion VARCHAR(255) NOT NULL,
    PrecioVenta DECIMAL(30, 15) NOT NULL,
    EsBonoVerde BOOLEAN DEFAULT FALSE,
    Estado ENUM('Disponible', 'Vendido') DEFAULT 'Disponible'
);

CREATE TABLE ConfigFinanciera (
    ConfigID INT AUTO_INCREMENT PRIMARY KEY,
    NombreBanco VARCHAR(50) NOT NULL,
    TipoTasa ENUM('Nominal', 'Efectiva') DEFAULT 'Efectiva',
    TasaInteres DECIMAL(30, 15) NOT NULL,
    PorcentajeDesgravamen DECIMAL(30, 15) NOT NULL,
    PorcentajeSeguroInmueble DECIMAL(30, 15) NOT NULL,
    PlazoMinimoMeses INT DEFAULT 60,
    PlazoMaximoMeses INT DEFAULT 240
);

CREATE TABLE MaestroBonos (
    BonoID INT AUTO_INCREMENT PRIMARY KEY,
    PrecioViviendaDesde DECIMAL(30, 15) NOT NULL,
    PrecioViviendaHasta DECIMAL(30, 15) NOT NULL,
    MontoBono DECIMAL(30, 15) NOT NULL,
    EsSostenible BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE TABLE Simulaciones (
    SimulacionID INT AUTO_INCREMENT PRIMARY KEY,
    ClienteID INT NOT NULL,
    PropiedadID INT NOT NULL,
    ConfigID INT NOT NULL,
    FechaSimulacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    PrecioVenta DECIMAL(30, 15) NOT NULL,
    CuotaInicial DECIMAL(30, 15) NOT NULL,
    MontoPrestamo DECIMAL(30, 15) NOT NULL,
    PlazoMeses INT NOT NULL,
    TipoGracia ENUM('Sin Gracia', 'Parcial', 'Total') DEFAULT 'Sin Gracia',
    MesesGracia INT DEFAULT 0,
    TasaEfectivaAnual DECIMAL(30, 15) NOT NULL,
    TCEA DECIMAL(30, 15) NOT NULL,
    VAN DECIMAL(30, 15) NOT NULL,
    TIR DECIMAL(30, 15) NOT NULL,
    FOREIGN KEY (ClienteID) REFERENCES Clientes(ClienteID) ON DELETE CASCADE,
    FOREIGN KEY (PropiedadID) REFERENCES Propiedades(PropiedadID) ON DELETE CASCADE,
    FOREIGN KEY (ConfigID) REFERENCES ConfigFinanciera(ConfigID) ON DELETE CASCADE
);

CREATE TABLE DetalleCronograma (
    DetalleID INT AUTO_INCREMENT PRIMARY KEY,
    SimulacionID INT NOT NULL,
    NroCuota INT NOT NULL,
    TasaEfectivaPeriodo DECIMAL(30, 15) NOT NULL,
    SaldoInicial DECIMAL(30, 15) NOT NULL,
    Interes DECIMAL(30, 15) NOT NULL,
    Amortizacion DECIMAL(30, 15) NOT NULL,
    Cuota DECIMAL(30, 15) NOT NULL,
    SeguroDesgravamen DECIMAL(30, 15) NOT NULL,
    SeguroInmueble DECIMAL(30, 15) NOT NULL,
    CuotaTotal DECIMAL(30, 15) NOT NULL,
    SaldoFinal DECIMAL(30, 15) NOT NULL,
    FOREIGN KEY (SimulacionID) REFERENCES Simulaciones(SimulacionID) ON DELETE CASCADE
);

INSERT INTO Usuarios (Username, PasswordHash, Rol) 
VALUES ('admin', 'admin123', 'Admin');

INSERT INTO ConfigFinanciera (NombreBanco, TipoTasa, TasaInteres, PorcentajeDesgravamen, PorcentajeSeguroInmueble) 
VALUES ('Banco Continental', 'Efectiva', 0.125000000000000, 0.000500000000000, 0.000200000000000);

INSERT INTO Propiedades (Direccion, PrecioVenta, EsBonoVerde) 
VALUES ('Av. Primavera 123, Surco', 250000.000000000000000, TRUE),
       ('Calle Las Magnolias 456, San Isidro', 480000.000000000000000, FALSE);

INSERT INTO MaestroBonos (PrecioViviendaDesde, PrecioViviendaHasta, MontoBono, EsSostenible)
VALUES (67000.000000000000000, 93100.000000000000000, 25700.000000000000000, FALSE),
       (93100.000000000000000, 139400.000000000000000, 21400.000000000000000, FALSE),
       (139400.000000000000000, 232200.000000000000000, 19600.000000000000000, TRUE);
       
ALTER TABLE Simulaciones ADD COLUMN Moneda VARCHAR(3) NOT NULL DEFAULT 'PEN';