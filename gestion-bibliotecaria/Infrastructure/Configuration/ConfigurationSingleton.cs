using System;
using System.Data;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace gestion_bibliotecaria.Infrastructure.Configuration;

// PATRÓN SINGLETON: GESTOR DE CONEXIONES (Connection Factory)
public class ConfigurationSingleton
{
    private static ConfigurationSingleton? _instancia;
    private static readonly object _lock = new object();
    
    private readonly string _connectionString;

    private ConfigurationSingleton(IConfiguration configuration)
    {
        // ASP.NET Core ya sabe buscar en el appsettings, no hace falta leer el archivo a mano.
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");
    }

    // Inicializamos el Singleton desde Program.cs una sola vez
    public static void Initialize(IConfiguration configuration)
    {
        if (_instancia == null)
        {
            lock (_lock)
            {
                if (_instancia == null)
                {
                    _instancia = new ConfigurationSingleton(configuration);
                }
            }
        }
    }

    public static ConfigurationSingleton Instancia
    {
        get
        {
            if (_instancia == null)
                throw new InvalidOperationException("El Singleton no fue inicializado. Llamá a Initialize() en Program.cs");
                
            return _instancia;
        }
    }

    // ACA ESTA LA MAGIA: Cada vez que alguien pide conexión, le armamos una NUEVA.
    // Usamos IDbConnection (la interfaz pura) para no acoplar la arquitectura a MySQL en todos lados.
    public IDbConnection GetConnection()
    {
        // Entregamos una conexión nueva. El que la pide es responsable de abrirla y cerrarla.
        return new MySqlConnection(_connectionString);
    }
}