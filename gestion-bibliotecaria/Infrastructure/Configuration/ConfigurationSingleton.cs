using System;
using System.Data;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace gestion_bibliotecaria.Infrastructure.Configuration;


public class ConfigurationSingleton
{
    private static ConfigurationSingleton? _instancia;
    private static readonly object _lock = new object();
    
    private readonly string _connectionString;

    private ConfigurationSingleton(IConfiguration configuration)
    {
        
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");
    }

    
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

  
    public IDbConnection GetConnection()
    {
        
        return new MySqlConnection(_connectionString);
    }
}