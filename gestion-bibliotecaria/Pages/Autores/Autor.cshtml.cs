using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System.Data;

namespace gestion_bibliotecaria.Pages;

public class AutorModel : PageModel
{
    public DataTable AutorDataTable { get; set; } = new DataTable();

    private readonly IConfiguration configuration;

    public AutorModel(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public void OnGet()
    {
        Select();
    }

    void Select()
    {
        string connectionString = configuration.GetConnectionString("DefaultConnection")!;

        string query = @"SELECT AutorId, Nombres, Apellidos, Nacionalidad, FechaNacimiento, Estado
                         FROM autor
                         ORDER BY AutorId DESC";

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            MySqlCommand command = new MySqlCommand(query, connection);

            MySqlDataAdapter adapter = new MySqlDataAdapter(command);

            adapter.Fill(AutorDataTable);
        }
    }
}