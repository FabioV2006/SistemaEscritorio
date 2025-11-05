using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CapaDatos;
using CapaNegocio;

namespace CapaPresentación.Utilidades
{
    public static class ImpresionHelper
    {
        private static string RutaLogo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "logo.png");

        public static void ImprimirVenta(VENTAS venta, List<DETALLE_VENTAS> detalles)
        {
            if (venta == null) return;

            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    FixedDocument document = CrearDocumentoVentaFijo(venta, detalles, printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
                    printDialog.PrintDocument(document.DocumentPaginator, $"Venta {venta.NumeroDocumento}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al imprimir: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void ImprimirCompra(COMPRAS compra, List<DETALLE_COMPRAS> detalles)
        {
            if (compra == null) return;

            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    FixedDocument document = CrearDocumentoCompraFijo(compra, detalles, printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
                    printDialog.PrintDocument(document.DocumentPaginator, $"Compra {compra.NumeroDocumento}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al imprimir: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static FixedDocument CrearDocumentoVentaFijo(VENTAS venta, List<DETALLE_VENTAS> detalles, double anchoImpresion, double altoImpresion)
        {
            FixedDocument document = new FixedDocument();
            PageContent pageContent = new PageContent();
            FixedPage fixedPage = new FixedPage();
            fixedPage.Width = 793.7; // Ancho A4 en puntos
            fixedPage.Height = 1122.5; // Alto A4 en puntos

            // Obtener datos del negocio
            CN_Negocio cnNegocio = new CN_Negocio();
            Negocio datosNegocio = cnNegocio.ObtenerDatos();

            Canvas canvas = new Canvas();
            canvas.Width = fixedPage.Width;
            canvas.Height = fixedPage.Height;

            double margenIzq = 30;
            double margenSup = 50;
            double yPos = margenSup;

            // ENCABEZADO
            // Logo
            if (File.Exists(RutaLogo))
            {
                try
                {
                    BitmapImage logo = new BitmapImage(new Uri(RutaLogo));
                    Image imgLogo = new Image
                    {
                        Source = logo,
                        Width = 120,
                        Height = 60,
                        Stretch = Stretch.Uniform
                    };
                    Canvas.SetLeft(imgLogo, margenIzq);
                    Canvas.SetTop(imgLogo, yPos);
                    canvas.Children.Add(imgLogo);
                }
                catch { /* Si falla, continúa sin logo */ }
            }

            // Datos del Negocio (Centro)
            Border borderNegocio = new Border
            {
                Width = 320,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0.5),
                Background = Brushes.White
            };

            StackPanel stackNegocio = new StackPanel
            {
                Margin = new Thickness(10, 5, 10, 5)
            };

            stackNegocio.Children.Add(CrearTexto($"RUC: {datosNegocio.RUC}", 9, FontWeights.Normal, TextAlignment.Center));
            stackNegocio.Children.Add(CrearTexto(datosNegocio.Direccion, 9, FontWeights.Normal, TextAlignment.Center));
            stackNegocio.Children.Add(CrearTexto($"Email: {datosNegocio.Correo}", 9, FontWeights.Normal, TextAlignment.Center));
            stackNegocio.Children.Add(CrearTexto($"Tel: {datosNegocio.Telefono}", 9, FontWeights.Normal, TextAlignment.Center));

            borderNegocio.Child = stackNegocio;
            Canvas.SetLeft(borderNegocio, margenIzq + 135);
            Canvas.SetTop(borderNegocio, yPos);
            canvas.Children.Add(borderNegocio);

            // Tipo y Número de Documento (Derecha)
            Border borderDoc = new Border
            {
                Width = 180,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0.5),
                Background = Brushes.White
            };

            StackPanel stackDoc = new StackPanel
            {
                Margin = new Thickness(10, 8, 10, 8)
            };

            stackDoc.Children.Add(CrearTexto(venta.TipoDocumento?.ToUpper() ?? "FACTURA", 10, FontWeights.Bold, TextAlignment.Center));
            stackDoc.Children.Add(CrearTexto($"001 - {venta.NumeroDocumento}", 10, FontWeights.Normal, TextAlignment.Center));

            borderDoc.Child = stackDoc;
            Canvas.SetLeft(borderDoc, margenIzq + 470);
            Canvas.SetTop(borderDoc, yPos);
            canvas.Children.Add(borderDoc);

            yPos += 75;

            // DATOS DEL CLIENTE
            // Fila 1
            canvas.Children.Add(CrearCeldaBorde("Cliente:", 100, 20, margenIzq, yPos, FontWeights.Bold));
            canvas.Children.Add(CrearCeldaBorde(venta.CLIENTES?.RazonSocial ?? "N/A", 440, 20, margenIzq + 100, yPos));
            canvas.Children.Add(CrearCeldaBorde("Fecha:", 80, 20, margenIzq + 540, yPos));

            yPos += 20;

            // Fila 2
            canvas.Children.Add(CrearCeldaBorde("Dirección:", 100, 20, margenIzq, yPos, FontWeights.Bold));
            canvas.Children.Add(CrearCeldaBorde(venta.CLIENTES?.Documento ?? "N/A", 440, 20, margenIzq + 100, yPos));
            canvas.Children.Add(CrearCeldaBorde(venta.FechaRegistro?.ToString("dd/MM/yyyy") ?? "N/A", 80, 20, margenIzq + 540, yPos));

            yPos += 30;

            // TABLA DE PRODUCTOS - Encabezado
            double[] anchoColumnas = { 70, 220, 100, 60, 40, 70 };
            string[] encabezados = { "Código", "Producto", "Laboratorio", "Precio", "Cant.", "Total" };
            double xPosTabla = margenIzq;

            for (int i = 0; i < encabezados.Length; i++)
            {
                Border headerCell = CrearCeldaBorde(encabezados[i], anchoColumnas[i], 20, xPosTabla, yPos, FontWeights.Bold);
                headerCell.Background = new SolidColorBrush(Color.FromRgb(248, 248, 248));
                canvas.Children.Add(headerCell);
                xPosTabla += anchoColumnas[i];
            }

            yPos += 20;

            // Datos de productos
            foreach (var detalle in detalles)
            {
                xPosTabla = margenIzq;
                canvas.Children.Add(CrearCeldaBorde(detalle.LOTES?.PRODUCTOS?.Codigo ?? "", anchoColumnas[0], 15, xPosTabla, yPos));
                xPosTabla += anchoColumnas[0];

                canvas.Children.Add(CrearCeldaBorde(detalle.LOTES?.PRODUCTOS?.Nombre ?? "", anchoColumnas[1], 15, xPosTabla, yPos));
                xPosTabla += anchoColumnas[1];

                canvas.Children.Add(CrearCeldaBorde(detalle.LOTES?.PRODUCTOS?.LABORATORIOS?.Nombre ?? "", anchoColumnas[2], 15, xPosTabla, yPos));
                xPosTabla += anchoColumnas[2];

                canvas.Children.Add(CrearCeldaBorde($"{detalle.PrecioVentaUnitario:F2}", anchoColumnas[3], 15, xPosTabla, yPos, FontWeights.Normal, TextAlignment.Right));
                xPosTabla += anchoColumnas[3];

                canvas.Children.Add(CrearCeldaBorde($"{detalle.Cantidad}", anchoColumnas[4], 15, xPosTabla, yPos, FontWeights.Normal, TextAlignment.Center));
                xPosTabla += anchoColumnas[4];

                canvas.Children.Add(CrearCeldaBorde($"{detalle.SubTotal:F2}", anchoColumnas[5], 15, xPosTabla, yPos, FontWeights.Normal, TextAlignment.Right));

                yPos += 15;
            }

            // Filas vacías para completar 45
            int filasVacias = Math.Max(0, 45 - detalles.Count);
            for (int i = 0; i < filasVacias; i++)
            {
                xPosTabla = margenIzq;
                for (int j = 0; j < anchoColumnas.Length; j++)
                {
                    canvas.Children.Add(CrearCeldaBorde(" ", anchoColumnas[j], 15, xPosTabla, yPos));
                    xPosTabla += anchoColumnas[j];
                }
                yPos += 15;
            }

            yPos += 10;

            // TOTALES
            string montoLetras = NumeroALetras.Convertir(venta.MontoTotal ?? 0);
            canvas.Children.Add(CrearCeldaBorde($"Son: {montoLetras}", 470, 30, margenIzq, yPos, FontWeights.Normal, TextAlignment.Left));
            canvas.Children.Add(CrearCeldaBorde($"Total: S/ {venta.MontoTotal:F2}", 150, 30, margenIzq + 470, yPos, FontWeights.Bold, TextAlignment.Right));

            yPos += 40;

            // CANCELADO
            Border borderCancelado = new Border
            {
                Width = 200,
                Height = 70,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0.5),
                Background = Brushes.White
            };

            StackPanel stackCancelado = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(10)
            };

            stackCancelado.Children.Add(CrearTexto("CANCELADO", 11, FontWeights.Bold, TextAlignment.Center));
            stackCancelado.Children.Add(new TextBlock { Height = 30 }); // Espacio para firma
            stackCancelado.Children.Add(CrearTexto("Fecha ____ de ______ del 20___", 9, FontWeights.Normal, TextAlignment.Center));

            borderCancelado.Child = stackCancelado;
            Canvas.SetLeft(borderCancelado, margenIzq + 220);
            Canvas.SetTop(borderCancelado, yPos);
            canvas.Children.Add(borderCancelado);

            fixedPage.Children.Add(canvas);
            ((IAddChild)pageContent).AddChild(fixedPage);
            document.Pages.Add(pageContent);

            return document;
        }

        private static FixedDocument CrearDocumentoCompraFijo(COMPRAS compra, List<DETALLE_COMPRAS> detalles, double anchoImpresion, double altoImpresion)
        {
            FixedDocument document = new FixedDocument();
            PageContent pageContent = new PageContent();
            FixedPage fixedPage = new FixedPage();
            fixedPage.Width = 793.7;
            fixedPage.Height = 1122.5;

            CN_Negocio cnNegocio = new CN_Negocio();
            Negocio datosNegocio = cnNegocio.ObtenerDatos();

            Canvas canvas = new Canvas();
            canvas.Width = fixedPage.Width;
            canvas.Height = fixedPage.Height;

            double margenIzq = 30;
            double margenSup = 50;
            double yPos = margenSup;

            // ENCABEZADO (igual que venta)
            if (File.Exists(RutaLogo))
            {
                try
                {
                    BitmapImage logo = new BitmapImage(new Uri(RutaLogo));
                    Image imgLogo = new Image
                    {
                        Source = logo,
                        Width = 120,
                        Height = 60,
                        Stretch = Stretch.Uniform
                    };
                    Canvas.SetLeft(imgLogo, margenIzq);
                    Canvas.SetTop(imgLogo, yPos);
                    canvas.Children.Add(imgLogo);
                }
                catch { }
            }

            Border borderNegocio = new Border
            {
                Width = 320,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0.5),
                Background = Brushes.White
            };

            StackPanel stackNegocio = new StackPanel { Margin = new Thickness(10, 5, 10, 5) };
            stackNegocio.Children.Add(CrearTexto($"RUC: {datosNegocio.RUC}", 9, FontWeights.Normal, TextAlignment.Center));
            stackNegocio.Children.Add(CrearTexto(datosNegocio.Direccion, 9, FontWeights.Normal, TextAlignment.Center));
            stackNegocio.Children.Add(CrearTexto($"Email: {datosNegocio.Correo}", 9, FontWeights.Normal, TextAlignment.Center));
            stackNegocio.Children.Add(CrearTexto($"Tel: {datosNegocio.Telefono}", 9, FontWeights.Normal, TextAlignment.Center));

            borderNegocio.Child = stackNegocio;
            Canvas.SetLeft(borderNegocio, margenIzq + 135);
            Canvas.SetTop(borderNegocio, yPos);
            canvas.Children.Add(borderNegocio);

            Border borderDoc = new Border
            {
                Width = 180,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0.5),
                Background = Brushes.White
            };

            StackPanel stackDoc = new StackPanel { Margin = new Thickness(10, 8, 10, 8) };
            stackDoc.Children.Add(CrearTexto(compra.TipoDocumento?.ToUpper() ?? "COMPRA", 10, FontWeights.Bold, TextAlignment.Center));
            stackDoc.Children.Add(CrearTexto($"001 - {compra.NumeroDocumento}", 10, FontWeights.Normal, TextAlignment.Center));

            borderDoc.Child = stackDoc;
            Canvas.SetLeft(borderDoc, margenIzq + 470);
            Canvas.SetTop(borderDoc, yPos);
            canvas.Children.Add(borderDoc);

            yPos += 75;

            // DATOS DEL PROVEEDOR
            canvas.Children.Add(CrearCeldaBorde("Proveedor:", 100, 20, margenIzq, yPos, FontWeights.Bold));
            canvas.Children.Add(CrearCeldaBorde(compra.PROVEEDORES?.RazonSocial ?? "N/A", 440, 20, margenIzq + 100, yPos));
            canvas.Children.Add(CrearCeldaBorde("Fecha:", 80, 20, margenIzq + 540, yPos));

            yPos += 20;

            canvas.Children.Add(CrearCeldaBorde("RUC:", 100, 20, margenIzq, yPos, FontWeights.Bold));
            canvas.Children.Add(CrearCeldaBorde(compra.PROVEEDORES?.Documento ?? "N/A", 440, 20, margenIzq + 100, yPos));
            canvas.Children.Add(CrearCeldaBorde(compra.FechaRegistro?.ToString("dd/MM/yyyy") ?? "N/A", 80, 20, margenIzq + 540, yPos));

            yPos += 30;

            // TABLA
            double[] anchoColumnas = { 70, 220, 100, 60, 40, 70 };
            string[] encabezados = { "Código", "Producto", "Laboratorio", "Precio", "Cant.", "Total" };
            double xPosTabla = margenIzq;

            for (int i = 0; i < encabezados.Length; i++)
            {
                Border headerCell = CrearCeldaBorde(encabezados[i], anchoColumnas[i], 20, xPosTabla, yPos, FontWeights.Bold);
                headerCell.Background = new SolidColorBrush(Color.FromRgb(248, 248, 248));
                canvas.Children.Add(headerCell);
                xPosTabla += anchoColumnas[i];
            }

            yPos += 20;

            foreach (var detalle in detalles)
            {
                xPosTabla = margenIzq;
                canvas.Children.Add(CrearCeldaBorde(detalle.LOTES?.PRODUCTOS?.Codigo ?? "", anchoColumnas[0], 15, xPosTabla, yPos));
                xPosTabla += anchoColumnas[0];

                canvas.Children.Add(CrearCeldaBorde(detalle.LOTES?.PRODUCTOS?.Nombre ?? "", anchoColumnas[1], 15, xPosTabla, yPos));
                xPosTabla += anchoColumnas[1];

                canvas.Children.Add(CrearCeldaBorde(detalle.LOTES?.PRODUCTOS?.LABORATORIOS?.Nombre ?? "", anchoColumnas[2], 15, xPosTabla, yPos));
                xPosTabla += anchoColumnas[2];

                canvas.Children.Add(CrearCeldaBorde($"{detalle.PrecioCompraUnitario:F2}", anchoColumnas[3], 15, xPosTabla, yPos, FontWeights.Normal, TextAlignment.Right));
                xPosTabla += anchoColumnas[3];

                canvas.Children.Add(CrearCeldaBorde($"{detalle.Cantidad}", anchoColumnas[4], 15, xPosTabla, yPos, FontWeights.Normal, TextAlignment.Center));
                xPosTabla += anchoColumnas[4];

                canvas.Children.Add(CrearCeldaBorde($"{detalle.MontoTotal:F2}", anchoColumnas[5], 15, xPosTabla, yPos, FontWeights.Normal, TextAlignment.Right));

                yPos += 15;
            }

            int filasVacias = Math.Max(0, 45 - detalles.Count);
            for (int i = 0; i < filasVacias; i++)
            {
                xPosTabla = margenIzq;
                for (int j = 0; j < anchoColumnas.Length; j++)
                {
                    canvas.Children.Add(CrearCeldaBorde(" ", anchoColumnas[j], 15, xPosTabla, yPos));
                    xPosTabla += anchoColumnas[j];
                }
                yPos += 15;
            }

            yPos += 10;

            // TOTALES
            string montoLetras = NumeroALetras.Convertir(compra.MontoTotal ?? 0);
            canvas.Children.Add(CrearCeldaBorde($"Son: {montoLetras}", 470, 30, margenIzq, yPos, FontWeights.Normal, TextAlignment.Left));
            canvas.Children.Add(CrearCeldaBorde($"Total: S/ {compra.MontoTotal:F2}", 150, 30, margenIzq + 470, yPos, FontWeights.Bold, TextAlignment.Right));

            yPos += 40;

            // CANCELADO
            Border borderCancelado = new Border
            {
                Width = 200,
                Height = 70,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0.5),
                Background = Brushes.White
            };

            StackPanel stackCancelado = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(10)
            };

            stackCancelado.Children.Add(CrearTexto("CANCELADO", 11, FontWeights.Bold, TextAlignment.Center));
            stackCancelado.Children.Add(new TextBlock { Height = 30 });
            stackCancelado.Children.Add(CrearTexto("Fecha ____ de ______ del 20___", 9, FontWeights.Normal, TextAlignment.Center));

            borderCancelado.Child = stackCancelado;
            Canvas.SetLeft(borderCancelado, margenIzq + 220);
            Canvas.SetTop(borderCancelado, yPos);
            canvas.Children.Add(borderCancelado);

            fixedPage.Children.Add(canvas);
            ((IAddChild)pageContent).AddChild(fixedPage);
            document.Pages.Add(pageContent);

            return document;
        }

        private static Border CrearCeldaBorde(string texto, double ancho, double alto, double x, double y,
            FontWeight? peso = null, TextAlignment alineacion = TextAlignment.Left)
        {
            Border border = new Border
            {
                Width = ancho,
                Height = alto,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0.5),
                Background = Brushes.White
            };

            TextBlock textBlock = new TextBlock
            {
                Text = texto,
                FontFamily = new FontFamily("Arial"),
                FontSize = 9,
                FontWeight = peso ?? FontWeights.Normal,
                TextAlignment = alineacion,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(3, 2, 3, 2),
                TextWrapping = TextWrapping.NoWrap,
                TextTrimming = TextTrimming.CharacterEllipsis
            };

            border.Child = textBlock;
            Canvas.SetLeft(border, x);
            Canvas.SetTop(border, y);

            return border;
        }

        private static TextBlock CrearTexto(string texto, double tamano, FontWeight peso, TextAlignment alineacion)
        {
            return new TextBlock
            {
                Text = texto,
                FontFamily = new FontFamily("Arial"),
                FontSize = tamano,
                FontWeight = peso,
                TextAlignment = alineacion,
                Margin = new Thickness(0, 2, 0, 2),
                TextWrapping = TextWrapping.Wrap
            };
        }
    }
}