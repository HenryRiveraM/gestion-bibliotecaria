namespace gestion_bibliotecaria.Validaciones
{
    public static class ValidadorEntrada
    {
        public static bool EstaVacio(string valor)
        {
            return string.IsNullOrWhiteSpace(valor);
        }

        public static bool ExcedeLongitud(string valor, int maximo)
        {
            if (valor == null)
            {
                return false;
            }

            return valor.Length > maximo;
        }

        public static bool SoloLetras(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return false;
            }

            foreach (char c in valor)
            {
                if (!char.IsLetter(c) && c != ' ')
                {
                    return false;
                }
            }

            return true;
        }

        public static bool SoloLetrasYNumeros(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return false;
            }

            foreach (char c in valor)
            {
                if (!char.IsLetterOrDigit(c) && c != ' ')
                {
                    return false;
                }
            }

            return true;
        }

        public static bool CodigoInventarioValido(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return false;
            }

            foreach (char c in valor)
            {
                if (!char.IsLetterOrDigit(c) && c != '-')
                {
                    return false;
                }
            }

            return true;
        }

        public static bool FechaNoFutura(DateTime? fecha)
        {
            if (!fecha.HasValue)
            {
                return true;
            }

            return fecha.Value.Date <= DateTime.Today;
        }

        public static string NormalizarEspacios(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return string.Empty;
            }

            var trimmed = valor.Trim();
            return System.Text.RegularExpressions.Regex.Replace(trimmed, "\\s+", " ");
        }

        public static bool ValidYear(int? year, int minyear = 1000)
        {
            if (!year.HasValue)
            {
                return true;
            }

            int current = DateTime.Now.Year;
            return year.Value >= minyear && year.Value <= current;
        }
    }
}