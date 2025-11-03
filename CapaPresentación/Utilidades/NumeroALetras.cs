using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaPresentación.Utilidades
{
    public static class NumeroALetras
    {
        private static string[] UNIDADES = { "", "UNO", "DOS", "TRES", "CUATRO", "CINCO", "SEIS", "SIETE", "OCHO", "NUEVE" };
        private static string[] DECENAS = { "", "", "VEINTE", "TREINTA", "CUARENTA", "CINCUENTA", "SESENTA", "SETENTA", "OCHENTA", "NOVENTA" };
        private static string[] ESPECIALES = { "DIEZ", "ONCE", "DOCE", "TRECE", "CATORCE", "QUINCE", "DIECISEIS", "DIECISIETE", "DIECIOCHO", "DIECINUEVE" };
        private static string[] CENTENAS = { "", "CIENTO", "DOSCIENTOS", "TRESCIENTOS", "CUATROCIENTOS", "QUINIENTOS", "SEISCIENTOS", "SETECIENTOS", "OCHOCIENTOS", "NOVECIENTOS" };

        public static string Convertir(decimal numero, string moneda = "SOLES")
        {
            int parteEntera = (int)Math.Floor(numero);
            int centavos = (int)Math.Round((numero - parteEntera) * 100);

            if (parteEntera == 0)
                return $"CERO {moneda} CON {centavos:00}/100";

            string resultado = ConvertirParteEntera(parteEntera);
            return $"{resultado} {moneda} CON {centavos:00}/100";
        }

        private static string ConvertirParteEntera(int numero)
        {
            if (numero == 0) return "CERO";
            if (numero == 100) return "CIEN";

            string resultado = "";

            // Millones
            if (numero >= 1000000)
            {
                int millones = numero / 1000000;
                if (millones == 1)
                    resultado += "UN MILLON ";
                else
                    resultado += ConvertirMenorMil(millones) + " MILLONES ";
                numero %= 1000000;
            }

            // Miles
            if (numero >= 1000)
            {
                int miles = numero / 1000;
                if (miles == 1)
                    resultado += "MIL ";
                else
                    resultado += ConvertirMenorMil(miles) + " MIL ";
                numero %= 1000;
            }

            // Centenas, decenas y unidades
            if (numero > 0)
            {
                resultado += ConvertirMenorMil(numero);
            }

            return resultado.Trim();
        }

        private static string ConvertirMenorMil(int numero)
        {
            if (numero == 0) return "";
            if (numero == 100) return "CIEN";

            string resultado = "";

            // Centenas
            if (numero >= 100)
            {
                resultado += CENTENAS[numero / 100] + " ";
                numero %= 100;
            }

            // Decenas y unidades
            if (numero >= 20)
            {
                resultado += DECENAS[numero / 10];
                if (numero % 10 != 0)
                    resultado += " Y " + UNIDADES[numero % 10];
            }
            else if (numero >= 10)
            {
                resultado += ESPECIALES[numero - 10];
            }
            else if (numero > 0)
            {
                resultado += UNIDADES[numero];
            }

            return resultado.Trim();
        }
    }
}