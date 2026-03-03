using Microsoft.AspNetCore.Mvc.RazorPages;
using gestion_bibliotecaria.Services;
using MySql.Data.MySqlClient;
using System.Data;

namespace gestion_bibliotecaria.Pages;

public class AutorModel : PageModel
{
    private readonly DatabaseService _databaseService;

    public DataTable AutorDataTable { get; set; } = new DataTable();

    public AutorModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public void OnGet()
    {
        Select();
    }

    void Select()
    {
        using (var connection = _databaseService.CreateConnection())
        {
            connection.Open();
            string query = @"SELECT AutorId, Nombres, Apellidos, Nacionalidad, FechaNacimiento, Estado 
                            FROM Autor 
                            WHERE Estado = 1 
                            ORDER BY Apellidos, Nombres";

            using (var command = new MySqlCommand(query, connection))
            {
                using (var adapter = new MySqlDataAdapter(command))
                {
                    adapter.Fill(AutorDataTable);
                }
            }
        }
    }
}
