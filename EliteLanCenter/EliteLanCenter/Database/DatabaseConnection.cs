// =============================================
// ELITE LAN CENTER - CONEXIÓN A BASE DE DATOS
// =============================================

using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace EliteLanCenter.Database
{
    public class DatabaseConnection
    {
        // Ruta de la base de datos
        private static string GetDatabasePath()
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // En desarrollo — carpeta del proyecto
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "elite.db");
            }
            else
            {
                // En producción — carpeta del ejecutable
                return Path.Combine(
                    Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!,
                    "elite.db"
                );
            }
        }

        public static readonly string DatabasePath = GetDatabasePath();

        // Retorna una conexión activa a la base de datos
        public static SqliteConnection GetConnection()
        {
            var connection = new SqliteConnection($"Data Source={DatabasePath}");
            connection.Open();
            return connection;
        }

        // Inicializa la base de datos creando todas las tablas
        public static void Initialize()
        {
            var logFile = Path.Combine(Path.GetTempPath(), "elite_init.log");
            try
            {
                File.AppendAllText(logFile, $"[{DateTime.Now}] Starting DB init. Path: {DatabasePath}\r\n");
                
                // Eliminar archivo existente si está vacío o corrupto
                if (File.Exists(DatabasePath))
                {
                    var info = new FileInfo(DatabasePath);
                    File.AppendAllText(logFile, $"[{DateTime.Now}] DB file exists, size: {info.Length}\r\n");
                    if (info.Length == 0)
                    {
                        File.Delete(DatabasePath);
                        File.AppendAllText(logFile, $"[{DateTime.Now}] Deleted empty DB file\r\n");
                    }
                }

                using var connection = GetConnection();
                File.AppendAllText(logFile, $"[{DateTime.Now}] Connection opened\r\n");

                // Crear tabla de usuarios primero (la más importante)
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Usuarios (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Nombre TEXT NOT NULL,
                            Usuario TEXT NOT NULL UNIQUE,
                            Contrasena TEXT NOT NULL,
                            Rol TEXT NOT NULL,
                            Activo INTEGER DEFAULT 1,
                            CreadoEn TEXT DEFAULT (datetime('now', 'localtime'))
                        )";
                    command.ExecuteNonQuery();
                    File.AppendAllText(logFile, $"[{DateTime.Now}] Usuarios table created\r\n");
                }

                // Crear las demás tablas
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Productos (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Nombre TEXT NOT NULL UNIQUE,
                            PrecioUnidad REAL NOT NULL DEFAULT 0,
                            PrecioPaquete REAL NOT NULL DEFAULT 0,
                            UnidadesPaquete INTEGER NOT NULL DEFAULT 1,
                            Activo INTEGER DEFAULT 1,
                            CreadoEn TEXT DEFAULT (datetime('now', 'localtime'))
                        );
                        CREATE TABLE IF NOT EXISTS StockMostrador (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            ProductoId INTEGER NOT NULL REFERENCES Productos(Id),
                            Cantidad INTEGER NOT NULL DEFAULT 0,
                            ActualizadoEn TEXT DEFAULT (datetime('now', 'localtime'))
                        );
                        CREATE TABLE IF NOT EXISTS StockAlmacen (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            ProductoId INTEGER NOT NULL REFERENCES Productos(Id),
                            Paquetes INTEGER NOT NULL DEFAULT 0,
                            UnidadesSueltas INTEGER NOT NULL DEFAULT 0,
                            ActualizadoEn TEXT DEFAULT (datetime('now', 'localtime'))
                        );
                        CREATE TABLE IF NOT EXISTS Turnos (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            UsuarioId INTEGER NOT NULL REFERENCES Usuarios(Id),
                            Tipo TEXT NOT NULL,
                            Fecha TEXT NOT NULL,
                            Abierto INTEGER DEFAULT 1,
                            AbiertaEn TEXT DEFAULT (datetime('now', 'localtime')),
                            CerradaEn TEXT
                        );
                        CREATE TABLE IF NOT EXISTS Ventas (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            TurnoId INTEGER NOT NULL REFERENCES Turnos(Id),
                            ProductoId INTEGER NOT NULL REFERENCES Productos(Id),
                            Cantidad INTEGER NOT NULL,
                            PrecioUnit REAL NOT NULL,
                            Total REAL NOT NULL,
                            Fiado INTEGER DEFAULT 0,
                            FiadoPagado INTEGER DEFAULT 0,
                            NumeroPc TEXT,
                            CreadoEn TEXT DEFAULT (datetime('now', 'localtime'))
                        );
                        CREATE TABLE IF NOT EXISTS Caseros (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Nombre TEXT NOT NULL,
                            Apodo TEXT,
                            Telefono TEXT,
                            CodigoRFID TEXT UNIQUE,
                            Saldo REAL DEFAULT 0,
                            Activo INTEGER DEFAULT 1,
                            UltimaVisita TEXT,
                            CreadoEn TEXT DEFAULT (datetime('now', 'localtime'))
                        );
                        CREATE TABLE IF NOT EXISTS Recargas (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            CaseroId INTEGER NOT NULL REFERENCES Caseros(Id),
                            Monto REAL NOT NULL,
                            UsuarioId INTEGER REFERENCES Usuarios(Id),
                            CreadoEn TEXT DEFAULT (datetime('now', 'localtime'))
                        );
                        CREATE TABLE IF NOT EXISTS Consumos (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            CaseroId INTEGER NOT NULL REFERENCES Caseros(Id),
                            Monto REAL NOT NULL,
                            SaldoAnterior REAL NOT NULL,
                            SaldoNuevo REAL NOT NULL,
                            UsuarioId INTEGER REFERENCES Usuarios(Id),
                            CreadoEn TEXT DEFAULT (datetime('now', 'localtime'))
                        );
                        CREATE TABLE IF NOT EXISTS IngresosMercaderia (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            ProductoId INTEGER NOT NULL REFERENCES Productos(Id),
                            Cantidad INTEGER NOT NULL,
                            EsPaquete INTEGER DEFAULT 1,
                            CostoTotal REAL NOT NULL,
                            RegistradoPor INTEGER REFERENCES Usuarios(Id),
                            CreadoEn TEXT DEFAULT (datetime('now', 'localtime'))
                        );
                        CREATE TABLE IF NOT EXISTS Reportes (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            TurnoId INTEGER NOT NULL REFERENCES Turnos(Id),
                            IngresoMaquinas REAL DEFAULT 0,
                            TotalProductos REAL DEFAULT 0,
                            TotalBruto REAL DEFAULT 0,
                            Descuentos REAL DEFAULT 0,
                            TotalLiquido REAL DEFAULT 0,
                            MontoYape REAL DEFAULT 0,
                            Efectivo REAL DEFAULT 0,
                            ImagenPanCafe1 TEXT,
                            ImagenPanCafe2 TEXT,
                            ImagenYape TEXT,
                            PdfPath TEXT,
                            CreadoEn TEXT DEFAULT (datetime('now', 'localtime'))
                        )";
                    command.ExecuteNonQuery();
                    File.AppendAllText(logFile, $"[{DateTime.Now}] Other tables created\r\n");
                }

                // Migraciones para tablas existentes
                using (var migrar = connection.CreateCommand())
                {
                    migrar.CommandText = "ALTER TABLE Ventas ADD COLUMN NumeroPc TEXT";
                    try { migrar.ExecuteNonQuery(); File.AppendAllText(logFile, $"[{DateTime.Now}] Migrated Ventas: added NumeroPc\r\n"); } catch { }
                }

                File.AppendAllText(logFile, $"[{DateTime.Now}] DB init complete\r\n");
            }
            catch (Exception ex)
            {
                File.AppendAllText(logFile, $"[{DateTime.Now}] ERROR: {ex.Message}\r\n{ex.StackTrace}\r\n");
                throw;
            }
        }

        // Esquema completo de la base de datos
        private static string GetSchema()
        {
            return @"
                -- Usuarios del sistema
                CREATE TABLE IF NOT EXISTS Usuarios (
                    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nombre      TEXT    NOT NULL,
                    Usuario     TEXT    NOT NULL UNIQUE,
                    Contrasena  TEXT    NOT NULL,
                    Rol         TEXT    NOT NULL CHECK(Rol IN ('AdminDueno', 'AdminEncargado', 'Operador')),
                    Activo      INTEGER DEFAULT 1,
                    CreadoEn    TEXT    DEFAULT (datetime('now', 'localtime'))
                );

                -- Productos
                CREATE TABLE IF NOT EXISTS Productos (
                    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nombre          TEXT    NOT NULL UNIQUE,
                    PrecioUnidad    REAL    NOT NULL DEFAULT 0,
                    PrecioPaquete   REAL    NOT NULL DEFAULT 0,
                    UnidadesPaquete INTEGER NOT NULL DEFAULT 1,
                    Activo          INTEGER DEFAULT 1,
                    CreadoEn        TEXT    DEFAULT (datetime('now', 'localtime'))
                );

                -- Stock en mostrador
                CREATE TABLE IF NOT EXISTS StockMostrador (
                    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    ProductoId  INTEGER NOT NULL REFERENCES Productos(Id),
                    Cantidad    INTEGER NOT NULL DEFAULT 0,
                    ActualizadoEn TEXT DEFAULT (datetime('now', 'localtime'))
                );

                -- Stock en almacen
                CREATE TABLE IF NOT EXISTS StockAlmacen (
                    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                    ProductoId      INTEGER NOT NULL REFERENCES Productos(Id),
                    Paquetes        INTEGER NOT NULL DEFAULT 0,
                    UnidadesSueltas INTEGER NOT NULL DEFAULT 0,
                    ActualizadoEn   TEXT DEFAULT (datetime('now', 'localtime'))
                );

                -- Turnos
                CREATE TABLE IF NOT EXISTS Turnos (
                    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    UsuarioId   INTEGER NOT NULL REFERENCES Usuarios(Id),
                    Tipo        TEXT    NOT NULL CHECK(Tipo IN ('Mañana', 'Tarde', 'Noche')),
                    Fecha       TEXT    NOT NULL,
                    Abierto     INTEGER DEFAULT 1,
                    AbiertaEn   TEXT    DEFAULT (datetime('now', 'localtime')),
                    CerradaEn   TEXT
                );

                -- Ventas
                CREATE TABLE IF NOT EXISTS Ventas (
                    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    TurnoId     INTEGER NOT NULL REFERENCES Turnos(Id),
                    ProductoId  INTEGER NOT NULL REFERENCES Productos(Id),
                    Cantidad    INTEGER NOT NULL,
                    PrecioUnit  REAL    NOT NULL,
                    Total       REAL    NOT NULL,
                    Fiado       INTEGER DEFAULT 0,
                    FiadoPagado INTEGER DEFAULT 0,
                    CreadoEn    TEXT    DEFAULT (datetime('now', 'localtime'))
                );

                -- Movimientos de almacen a mostrador
                CREATE TABLE IF NOT EXISTS Movimientos (
                    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    TurnoId     INTEGER NOT NULL REFERENCES Turnos(Id),
                    ProductoId  INTEGER NOT NULL REFERENCES Productos(Id),
                    Paquetes    INTEGER DEFAULT 0,
                    Unidades    INTEGER DEFAULT 0,
                    CreadoEn    TEXT    DEFAULT (datetime('now', 'localtime'))
                );

                -- Ingresos de mercaderia
                CREATE TABLE IF NOT EXISTS IngresosMercaderia (
                    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    ProductoId  INTEGER NOT NULL REFERENCES Productos(Id),
                    Cantidad    INTEGER NOT NULL,
                    EsPaquete   INTEGER DEFAULT 1,
                    CostoTotal  REAL    NOT NULL,
                    RegistradoPor INTEGER REFERENCES Usuarios(Id),
                    CreadoEn    TEXT    DEFAULT (datetime('now', 'localtime'))
                );

                -- Reportes de cierre
                CREATE TABLE IF NOT EXISTS Reportes (
                    Id                  INTEGER PRIMARY KEY AUTOINCREMENT,
                    TurnoId             INTEGER NOT NULL REFERENCES Turnos(Id),
                    IngresoMaquinas     REAL    DEFAULT 0,
                    TotalProductos      REAL    DEFAULT 0,
                    TotalBruto          REAL    DEFAULT 0,
                    Descuentos          REAL    DEFAULT 0,
                    TotalLiquido        REAL    DEFAULT 0,
                    MontoYape           REAL    DEFAULT 0,
                    Efectivo            REAL    DEFAULT 0,
                    ImagenPanCafe1      TEXT,
                    ImagenPanCafe2      TEXT,
                    ImagenYape          TEXT,
                    PdfPath             TEXT,
                    CreadoEn            TEXT    DEFAULT (datetime('now', 'localtime'))
                );

                -- Conteo de stock al cerrar turno
                CREATE TABLE IF NOT EXISTS ConteoTurno (
                    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    TurnoId     INTEGER NOT NULL REFERENCES Turnos(Id),
                    ProductoId  INTEGER NOT NULL REFERENCES Productos(Id),
                    Cantidad    INTEGER NOT NULL DEFAULT 0,
                    CreadoEn    TEXT    DEFAULT (datetime('now', 'localtime'))
                );

                -- Caseros (clientes frecuentes)
                CREATE TABLE IF NOT EXISTS Caseros (
                    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nombre          TEXT    NOT NULL,
                    Apodo           TEXT,
                    Telefono        TEXT,
                    CodigoRFID      TEXT    UNIQUE,
                    Saldo           REAL    DEFAULT 0,
                    Activo          INTEGER DEFAULT 1,
                    UltimaVisita    TEXT,
                    CreadoEn        TEXT    DEFAULT (datetime('now', 'localtime'))
                );

                -- Recargas de caseros
                CREATE TABLE IF NOT EXISTS Recargas (
                    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    CaseroId    INTEGER NOT NULL REFERENCES Caseros(Id),
                    Monto       REAL    NOT NULL,
                    UsuarioId   INTEGER REFERENCES Usuarios(Id),
                    CreadoEn    TEXT    DEFAULT (datetime('now', 'localtime'))
                );

                -- Consumos de caseros
                CREATE TABLE IF NOT EXISTS Consumos (
                    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                    CaseroId        INTEGER NOT NULL REFERENCES Caseros(Id),
                    Monto           REAL    NOT NULL,
                    SaldoAnterior   REAL    NOT NULL,
                    SaldoNuevo      REAL    NOT NULL,
                    UsuarioId       INTEGER REFERENCES Usuarios(Id),
                    CreadoEn        TEXT    DEFAULT (datetime('now', 'localtime'))
                );
            ";
        }

        // Insertar datos iniciales
        public static void InsertarDatosIniciales()
        {
            var logFile = Path.Combine(Path.GetTempPath(), "elite_init.log");
            try
            {
                using var connection = GetConnection();
                using var command = connection.CreateCommand();

                command.CommandText = "SELECT COUNT(*) FROM Usuarios";
                var count = (long)command.ExecuteScalar()!;

                if (count == 0)
                {
                    command.CommandText = @"
                        INSERT INTO Usuarios (Nombre, Usuario, Contrasena, Rol)
                        VALUES 
                        ('Administrador', 'admin', 'Admin@Elite2024', 'AdminDueno'),
                        ('Encargado', 'encargado', 'Encargado@Elite2024', 'AdminEncargado'),
                        ('Operador', 'operador', 'Operador@Elite2024', 'Operador')
                    ";
                    command.ExecuteNonQuery();
                    File.AppendAllText(logFile, $"[{DateTime.Now}] Usuarios iniciales creados\r\n");
                }
                else
                {
                    File.AppendAllText(logFile, $"[{DateTime.Now}] Ya hay {count} usuarios en la DB\r\n");
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(logFile, $"[{DateTime.Now}] ERROR insertando datos: {ex.Message}\r\n{ex.StackTrace}\r\n");
            }
        }
    }
}