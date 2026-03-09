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
    }
}
