using System;
using MySql.Data.MySqlClient;

namespace GestionBibliotecaria.Helpers
{
    public class ConfigurationSingleton
    {
        private static ConfigurationSingleton _instancia;
        private static readonly object _lock = new object();
        
        private MySqlConnection _conexion; 
        private readonly string _connectionString;
        
        private ConfigurationSingleton()
        {
            try
            {
                string rutaArchivo = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
                string jsonString = System.IO.File.ReadAllText(rutaArchivo);

                using (System.Text.Json.JsonDocument doc = System.Text.Json.JsonDocument.Parse(jsonString))
                {
                    System.Text.Json.JsonElement root = doc.RootElement;
                    
                    _connectionString = root.GetProperty("ConnectionStrings")
                                            .GetProperty("DefaultConnection")
                                            .GetString();
                }

                _conexion = new MySqlConnection(_connectionString);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al leer appsettings.json o al inicializar la conexión: " + ex.Message);
                throw; 
            }
        }

        public static ConfigurationSingleton Instancia
        {
            get
            {
                if (_instancia == null)
                {
                    lock (_lock)
                    {
                        if (_instancia == null)
                        {
                            _instancia = new ConfigurationSingleton();
                        }
                    }
                }
                return _instancia;
            }
        }

        public MySqlConnection GetConnection()
        {
            try
            {
                if (_conexion.State == System.Data.ConnectionState.Closed || 
                    _conexion.State == System.Data.ConnectionState.Broken)
                {
                    _conexion.Open();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al conectar a la base de datos MySQL: " + ex.Message);
            }

            return _conexion;
        }

        public void CloseConnection()
        {
            if (_conexion != null && _conexion.State == System.Data.ConnectionState.Open)
            {
                _conexion.Close();
            }
        }
    }
}