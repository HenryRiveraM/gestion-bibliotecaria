using System.Security.Cryptography;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using gestion_bibliotecaria.Aplicacion.Interfaces;
using gestion_bibliotecaria.Domain.Common;
using gestion_bibliotecaria.Domain.Entities;
using gestion_bibliotecaria.Domain.Errors;
using gestion_bibliotecaria.Domain.Ports;
using gestion_bibliotecaria.Domain.Validations;
using MySql.Data.MySqlClient;

namespace gestion_bibliotecaria.Aplicacion.Servicios;

public class UsuarioServicio : IUsuarioServicio
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly IUsuarioRepositorio _usuarioRepositorio;
    private readonly IUserCredentialProvisioningService _credentialProvisioningService;

    public UsuarioServicio(
        IUsuarioRepositorio usuarioRepositorio,
        IUserCredentialProvisioningService credentialProvisioningService)
    {
        _usuarioRepositorio = usuarioRepositorio;
        _credentialProvisioningService = credentialProvisioningService;
    }

    public string JoinCiComp(string ci, string complemento)
    {
        if (string.IsNullOrWhiteSpace(ci)) return string.Empty;
        if (string.IsNullOrWhiteSpace(complemento)) return ci.Trim();
        return $"{ci.Trim()}-{complemento.Trim()}";
    }

    public DataTable Select()
    {
        var usuarios = _usuarioRepositorio.GetAll();
        var dt = new DataTable();
        dt.Columns.Add("UsuarioId", typeof(int));
        dt.Columns.Add("UsuarioSesionId", typeof(int));
        dt.Columns.Add("Nombres", typeof(string));
        dt.Columns.Add("PrimerApellido", typeof(string));
        dt.Columns.Add("SegundoApellido", typeof(string));
        dt.Columns.Add("Email", typeof(string));
        dt.Columns.Add("NombreUsuario", typeof(string));
        dt.Columns.Add("PasswordHash", typeof(string));
        dt.Columns.Add("Salt", typeof(string));
        dt.Columns.Add("Rol", typeof(string));
        dt.Columns.Add("Estado", typeof(bool));
        dt.Columns.Add("FechaRegistro", typeof(DateTime));
        dt.Columns.Add("UltimaActualizacion", typeof(DateTime));
        dt.Columns.Add("CI", typeof(string));

        foreach (var u in usuarios)
        {
            dt.Rows.Add(
                u.UsuarioId,
                u.UsuarioSesionId.HasValue ? (object)u.UsuarioSesionId.Value : DBNull.Value,
                u.Nombres,
                u.PrimerApellido,
                u.SegundoApellido ?? (object)DBNull.Value,
                u.Email,
                u.NombreUsuario,
                u.PasswordHash,
                u.Salt ?? (object)DBNull.Value,
                u.Rol,
                u.Estado,
                u.FechaRegistro,
                u.UltimaActualizacion.HasValue ? (object)u.UltimaActualizacion.Value : DBNull.Value,
                u.CI ?? (object)DBNull.Value
            );
        }
        return dt;
    }

    public Result<Usuario> Login(string nombreUsuario, string passwordPlano)
    {
        nombreUsuario = ValidadorEntrada.NormalizarEspacios(nombreUsuario);

        if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(passwordPlano))
        {
            return Result<Usuario>.Failure(UsuarioErrors.CredencialesInvalidas);
        }

        var usuario = _usuarioRepositorio.GetByNombreUsuario(nombreUsuario);

        if (usuario is null || !usuario.Estado)
        {
            return Result<Usuario>.Failure(UsuarioErrors.CredencialesInvalidas);
        }

        var passwordHash = ComputeSha256(passwordPlano);

        if (!string.Equals(usuario.PasswordHash, passwordHash, StringComparison.Ordinal))
        {
            return Result<Usuario>.Failure(UsuarioErrors.CredencialesInvalidas);
        }

        return Result<Usuario>.Success(usuario);
    }

    public async Task<Result> CrearUsuarioAsync(Usuario usuario, int usuarioSesionId, CancellationToken cancellationToken = default)
    {
        if (usuario is null)
        {
            return Result.Failure(UsuarioErrors.DatosObligatorios);
        }

        usuario.Nombres = ValidadorEntrada.NormalizarAMayusculas(usuario.Nombres);
        usuario.PrimerApellido = ValidadorEntrada.NormalizarAMayusculas(usuario.PrimerApellido);
        usuario.SegundoApellido = ValidadorEntrada.NormalizarAMayusculas(usuario.SegundoApellido);
        usuario.Email = ValidadorEntrada.NormalizarEspacios(usuario.Email).ToLowerInvariant();

        // SegundoApellido ya no es obligatorio; validar solo los campos requeridos
        if (string.IsNullOrWhiteSpace(usuario.Nombres)
            || string.IsNullOrWhiteSpace(usuario.PrimerApellido)
            || string.IsNullOrWhiteSpace(usuario.Email))
        {
            return Result.Failure(UsuarioErrors.DatosObligatorios);
        }

        if (!EmailRegex.IsMatch(usuario.Email))
        {
            return Result.Failure(UsuarioErrors.EmailInvalido);
        }

        if (_usuarioRepositorio.ExisteEmail(usuario.Email))
        {
            return Result.Failure(UsuarioErrors.EmailDuplicado);
        }

        if (!EsRolValido(usuario.Rol))
        {
            return Result.Failure(UsuarioErrors.RolInvalido);
        }

        usuario.UsuarioSesionId = usuarioSesionId;
        usuario.Estado = true;
        usuario.FechaRegistro = DateTime.Now;

        try
        {
            await _credentialProvisioningService.PrepareAndNotifyAsync(usuario, cancellationToken);
            _usuarioRepositorio.Insert(usuario);
        }
        catch (InvalidOperationException)
        {
            return Result.Failure(UsuarioErrors.NombreUsuarioDuplicado);
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            return Result.Failure(UsuarioErrors.NombreUsuarioDuplicado);
        }

        return Result.Success();
    }

    public Result DarDeBaja(int usuarioId, int usuarioSesionId)
    {
        if (usuarioId <= 0)
        {
            return Result.Failure(UsuarioErrors.UsuarioNoEncontrado);
        }

        var usuario = _usuarioRepositorio.GetById(usuarioId);

        if (usuario is null)
        {
            return Result.Failure(UsuarioErrors.UsuarioNoEncontrado);
        }

        if (!usuario.Estado)
        {
            return Result.Failure(UsuarioErrors.UsuarioYaInactivo);
        }

        usuario.UsuarioSesionId = usuarioSesionId;
        _usuarioRepositorio.Delete(usuario);

        return Result.Success();
    }

    private static bool EsRolValido(string rol)
    {
        return string.Equals(rol, Usuario.RolAdmin, StringComparison.Ordinal)
               || string.Equals(rol, Usuario.RolBibliotecario, StringComparison.Ordinal);
    }

    private static string ComputeSha256(string password)
    {
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}
