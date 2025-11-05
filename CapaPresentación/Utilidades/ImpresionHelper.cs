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
using System.Windows.Shapes;
using CapaDatos;
using CapaNegocio;

namespace CapaPresentación.Utilidades
{
    public static class ImpresionHelper
    {
        private static string RutaLogo = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "logo.png");

        // Define un grosor de línea constante para toda la impresión
        private const double GROSOR_LINEA = 0.75; // Puedes ajustar este valor (0.5, 1.0, etc.)

        public static void ImprimirVenta(VENTAS venta, List<DETALLE_VENTAS> detalles)
        {
            if (venta == null)
            {
                MessageBox.Show("No hay datos de venta para imprimir.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    FixedDocument document = CrearDocumentoVentaFijo(venta, detalles);
                    printDialog.PrintDocument(document.DocumentPaginator, $"Venta {venta.NumeroDocumento}");
                    MessageBox.Show("Documento impreso correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al imprimir: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void ImprimirCompra(COMPRAS compra, List<DETALLE_COMPRAS> detalles)
        {
            if (compra == null)
            {
                MessageBox.Show("No hay datos de compra para imprimir.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    FixedDocument document = CrearDocumentoCompraFijo(compra, detalles);
                    printDialog.PrintDocument(document.DocumentPaginator, $"Compra {compra.NumeroDocumento}");
                    MessageBox.Show("Documento impreso correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al imprimir: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static FixedDocument CrearDocumentoVentaFijo(VENTAS venta, List<DETALLE_VENTAS> detalles)
        {
            FixedDocument document = new FixedDocument();
            PageContent pageContent = new PageContent();
            FixedPage fixedPage = new FixedPage
            {
                Width = 793.7,  // A4 width
                Height = 1122.5 // A4 height
            };

            CN_Negocio cnNegocio = new CN_Negocio();
            Negocio datosNegocio = cnNegocio.ObtenerDatos();

            Canvas canvas = new Canvas
            {
                Width = fixedPage.Width,
                Height = fixedPage.Height
            };

            double anchoContenidoMax = 650.0;
            double margenIzq = (fixedPage.Width - anchoContenidoMax) / 2.0;
            if (margenIzq < 40) margenIzq = 40;

            double margenSup = 40;
            double yPos = margenSup;

            // ===== ENCABEZADO =====
            if (File.Exists(RutaLogo))
            {
                try
                {
                    BitmapImage logo = new BitmapImage();
                    logo.BeginInit();
                    logo.CacheOption = BitmapCacheOption.OnLoad;
                    logo.UriSource = new Uri(RutaLogo, UriKind.Absolute);
                    logo.EndInit();
                    logo.Freeze();

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
                catch (Exception ex)
                {
                    Console.WriteLine($"Error cargando logo: {ex.Message}");
                    MessageBox.Show($"No se pudo cargar el logo desde '{RutaLogo}'.\nError: {ex.Message}", "Error de Logo", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show($"No se encontró el logo en '{RutaLogo}'.\n\nPor favor, verifique que:\n1. El archivo 'logo.png' exista en la carpeta 'Resources' de su proyecto.\n2. En Visual Studio, las propiedades del archivo 'logo.png' estén configuradas como:\n   - 'Build Action' (Acción de compilación) = 'Content'\n   - 'Copy to Output Directory' (Copiar al directorio de salida) = 'Copy if newer' o 'Copy always'", "Error de Logo", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Border borderNegocio = new Border
            {
                Width = 300,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(GROSOR_LINEA), // Usar grosor constante
                Background = Brushes.White,
                Padding = new Thickness(8)
            };

            StackPanel stackNegocio = new StackPanel { Margin = new Thickness(5) };
            stackNegocio.Children.Add(CrearTexto($"RUC: {datosNegocio.RUC}", 9, FontWeights.Normal, TextAlignment.Center));
            stackNegocio.Children.Add(CrearTexto(datosNegocio.Direccion ?? "", 9, FontWeights.Normal, TextAlignment.Center));
            stackNegocio.Children.Add(CrearTexto($"Email: {datosNegocio.Correo}", 9, FontWeights.Normal, TextAlignment.Center));
            stackNegocio.Children.Add(CrearTexto($"Tel: {datosNegocio.Telefono}", 9, FontWeights.Normal, TextAlignment.Center));

            borderNegocio.Child = stackNegocio;
            Canvas.SetLeft(borderNegocio, margenIzq + 170);
            Canvas.SetTop(borderNegocio, yPos);
            canvas.Children.Add(borderNegocio);

            Border borderDoc = new Border
            {
                Width = 160,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(GROSOR_LINEA), // Usar grosor constante
                Background = Brushes.White,
                Padding = new Thickness(8)
            };

            StackPanel stackDoc = new StackPanel { Margin = new Thickness(5) };
            stackDoc.Children.Add(CrearTexto(venta.TipoDocumento?.ToUpper() ?? "FACTURA", 10, FontWeights.Bold, TextAlignment.Center));
            stackDoc.Children.Add(CrearTexto($"001 - {venta.NumeroDocumento}", 10, FontWeights.Normal, TextAlignment.Center));

            borderDoc.Child = stackDoc;
            Canvas.SetLeft(borderDoc, margenIzq + 490);
            Canvas.SetTop(borderDoc, yPos);
            canvas.Children.Add(borderDoc);

            yPos += 80;

            // ===== DATOS DEL CLIENTE =====
            canvas.Children.Add(CrearCeldaBorde("Cliente:", 100, 20, margenIzq, yPos, FontWeights.Bold));
            canvas.Children.Add(CrearCeldaBorde(venta.CLIENTES?.RazonSocial ?? "N/A", 420, 20, margenIzq + 100, yPos));
            canvas.Children.Add(CrearCeldaBorde("Fecha:", 100, 20, margenIzq + 520, yPos, FontWeights.Bold));
            yPos += 20;

            canvas.Children.Add(CrearCeldaBorde("Dirección:", 100, 20, margenIzq, yPos, FontWeights.Bold));
            canvas.Children.Add(CrearCeldaBorde(venta.CLIENTES?.Documento ?? "N/A", 420, 20, margenIzq + 100, yPos));
            canvas.Children.Add(CrearCeldaBorde(venta.FechaRegistro?.ToString("dd/MM/yyyy") ?? "N/A", 100, 20, margenIzq + 520, yPos));
            yPos += 30;

            // ===== TABLA DE PRODUCTOS =====
            double[] anchoColumnas = { 70, 240, 130, 60, 40, 80 }; // Suma = 620
            string[] encabezados = { "Código", "Producto", "Laboratorio", "Precio", "Cant.", "Total" };

            // Encabezados con fondo gris
            double xPosTabla = margenIzq;
            for (int i = 0; i < encabezados.Length; i++)
            {
                Border headerCell = CrearCeldaBorde(encabezados[i], anchoColumnas[i], 18, xPosTabla, yPos, FontWeights.Bold);
                headerCell.Background = new SolidColorBrush(Color.FromRgb(240, 240, 240));
                canvas.Children.Add(headerCell);
                xPosTabla += anchoColumnas[i];
            }
            yPos += 18;

            // Datos de productos
            foreach (var detalle in detalles)
            {
                xPosTabla = margenIzq;
                // Usar CrearCeldaSinBordeHorizontal con GROSOR_LINEA
                canvas.Children.Add(CrearCeldaSinBordeHorizontal(detalle.LOTES?.PRODUCTOS?.Codigo ?? "", anchoColumnas[0], 14, xPosTabla, yPos));
                xPosTabla += anchoColumnas[0];
                canvas.Children.Add(CrearCeldaSinBordeHorizontal(detalle.LOTES?.PRODUCTOS?.Nombre ?? "", anchoColumnas[1], 14, xPosTabla, yPos));
                xPosTabla += anchoColumnas[1];
                canvas.Children.Add(CrearCeldaSinBordeHorizontal(detalle.LOTES?.PRODUCTOS?.LABORATORIOS?.Nombre ?? "", anchoColumnas[2], 14, xPosTabla, yPos));
                xPosTabla += anchoColumnas[2];
                canvas.Children.Add(CrearCeldaSinBordeHorizontal($"{detalle.PrecioVentaUnitario:F2}", anchoColumnas[3], 14, xPosTabla, yPos, FontWeights.Normal, TextAlignment.Right));
                xPosTabla += anchoColumnas[3];
                canvas.Children.Add(CrearCeldaSinBordeHorizontal($"{detalle.Cantidad}", anchoColumnas[4], 14, xPosTabla, yPos, FontWeights.Normal, TextAlignment.Center));
                xPosTabla += anchoColumnas[4];
                canvas.Children.Add(CrearCeldaSinBordeHorizontal($"{detalle.SubTotal:F2}", anchoColumnas[5], 14, xPosTabla, yPos, FontWeights.Normal, TextAlignment.Right));
                yPos += 14;
            }

            // Filas vacías para completar el espacio
            int filasVacias = Math.Max(0, 45 - detalles.Count);
            for (int i = 0; i < filasVacias; i++)
            {
                xPosTabla = margenIzq;
                for (int j = 0; j < anchoColumnas.Length; j++)
                {
                    // Usar CrearCeldaSinBordeHorizontal con GROSOR_LINEA
                    canvas.Children.Add(CrearCeldaSinBordeHorizontal("", anchoColumnas[j], 14, xPosTabla, yPos));
                    xPosTabla += anchoColumnas[j];
                }
                yPos += 14;
            }

            // Borde inferior de la tabla
            Line lineaInferior = new Line
            {
                X1 = margenIzq,
                Y1 = yPos,
                X2 = margenIzq + anchoColumnas.Sum(),
                Y2 = yPos,
                Stroke = Brushes.Black,
                StrokeThickness = GROSOR_LINEA // Usar grosor constante
            };
            canvas.Children.Add(lineaInferior);

            yPos += 15;

            // ===== TOTALES =====
            string montoLetras = "SON " + NumeroALetras.Convertir(venta.MontoTotal ?? 0);
            canvas.Children.Add(CrearCeldaBorde(montoLetras, 450, 30, margenIzq, yPos, FontWeights.Normal, TextAlignment.Left));
            canvas.Children.Add(CrearCeldaBorde($"Total: S/ {venta.MontoTotal:F2}", 170, 30, margenIzq + 450, yPos, FontWeights.Bold, TextAlignment.Right));

            yPos += 45;

            // ===== CANCELADO =====
            double anchoCancelado = 250;
            Border borderCancelado = new Border
            {
                Width = anchoCancelado,
                Height = 80,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(GROSOR_LINEA), // Usar grosor constante
                Background = Brushes.White
            };

            StackPanel stackCancelado = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(10)
            };

            stackCancelado.Children.Add(CrearTexto("CANCELADO", 12, FontWeights.Bold, TextAlignment.Center));
            stackCancelado.Children.Add(new Border { Height = 35 });
            stackCancelado.Children.Add(CrearTexto("Fecha ____ de ______ del 20___", 9, FontWeights.Normal, TextAlignment.Center));

            borderCancelado.Child = stackCancelado;

            Canvas.SetLeft(borderCancelado, margenIzq + (anchoContenidoMax - anchoCancelado) / 2.0);
            Canvas.SetTop(borderCancelado, yPos);
            canvas.Children.Add(borderCancelado);

            fixedPage.Children.Add(canvas);
            ((IAddChild)pageContent).AddChild(fixedPage);
            document.Pages.Add(pageContent);

            return document;
        }

        private static FixedDocument CrearDocumentoCompraFijo(COMPRAS compra, List<DETALLE_COMPRAS> detalles)
        {
            FixedDocument document = new FixedDocument();
            PageContent pageContent = new PageContent();
            FixedPage fixedPage = new FixedPage
            {
                Width = 793.7,
                Height = 1122.5
            };

            CN_Negocio cnNegocio = new CN_Negocio();
            Negocio datosNegocio = cnNegocio.ObtenerDatos();

            Canvas canvas = new Canvas
            {
                Width = fixedPage.Width,
                Height = fixedPage.Height
            };

            double anchoContenidoMax = 650.0;
            double margenIzq = (fixedPage.Width - anchoContenidoMax) / 2.0;
            if (margenIzq < 40) margenIzq = 40;

            double margenSup = 40;
            double yPos = margenSup;

            // ===== ENCABEZADO =====
            if (File.Exists(RutaLogo))
            {
                try
                {
                    BitmapImage logo = new BitmapImage();
                    logo.BeginInit();
                    logo.CacheOption = BitmapCacheOption.OnLoad;
                    logo.UriSource = new Uri(RutaLogo, UriKind.Absolute);
                    logo.EndInit();
                    logo.Freeze();

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
                catch (Exception ex)
                {
                    Console.WriteLine($"Error cargando logo: {ex.Message}");
                    MessageBox.Show($"No se pudo cargar el logo desde '{RutaLogo}'.\nError: {ex.Message}", "Error de Logo", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show($"No se encontró el logo en '{RutaLogo}'.\n\nVerifique que 'logo.png' esté en 'Resources' y tenga:\n- Build Action = Content\n- Copy to Output = Copy if newer", "Error de Logo", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Border borderNegocio = new Border
            {
                Width = 300,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(GROSOR_LINEA), // Usar grosor constante
                Background = Brushes.White,
                Padding = new Thickness(8)
            };

            StackPanel stackNegocio = new StackPanel { Margin = new Thickness(5) };
            stackNegocio.Children.Add(CrearTexto($"RUC: {datosNegocio.RUC}", 9, FontWeights.Normal, TextAlignment.Center));
            stackNegocio.Children.Add(CrearTexto(datosNegocio.Direccion ?? "", 9, FontWeights.Normal, TextAlignment.Center));
            stackNegocio.Children.Add(CrearTexto($"Email: {datosNegocio.Correo}", 9, FontWeights.Normal, TextAlignment.Center));
            stackNegocio.Children.Add(CrearTexto($"Tel: {datosNegocio.Telefono}", 9, FontWeights.Normal, TextAlignment.Center));

            borderNegocio.Child = stackNegocio;
            Canvas.SetLeft(borderNegocio, margenIzq + 170);
            Canvas.SetTop(borderNegocio, yPos);
            canvas.Children.Add(borderNegocio);

            Border borderDoc = new Border
            {
                Width = 160,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(GROSOR_LINEA), // Usar grosor constante
                Background = Brushes.White,
                Padding = new Thickness(8)
            };

            StackPanel stackDoc = new StackPanel { Margin = new Thickness(5) };
            stackDoc.Children.Add(CrearTexto("GUÍA DE COMPRA", 10, FontWeights.Bold, TextAlignment.Center));
            stackDoc.Children.Add(CrearTexto($"001 - {compra.NumeroDocumento}", 10, FontWeights.Normal, TextAlignment.Center));

            borderDoc.Child = stackDoc;
            Canvas.SetLeft(borderDoc, margenIzq + 490);
            Canvas.SetTop(borderDoc, yPos);
            canvas.Children.Add(borderDoc);

            yPos += 80;

            // ===== DATOS DEL PROVEEDOR =====
            canvas.Children.Add(CrearCeldaBorde("Proveedor:", 100, 20, margenIzq, yPos, FontWeights.Bold));
            canvas.Children.Add(CrearCeldaBorde(compra.PROVEEDORES?.RazonSocial ?? "N/A", 420, 20, margenIzq + 100, yPos));
            canvas.Children.Add(CrearCeldaBorde("Fecha:", 100, 20, margenIzq + 520, yPos, FontWeights.Bold));
            yPos += 20;

            canvas.Children.Add(CrearCeldaBorde("RUC:", 100, 20, margenIzq, yPos, FontWeights.Bold));
            canvas.Children.Add(CrearCeldaBorde(compra.PROVEEDORES?.Documento ?? "N/A", 420, 20, margenIzq + 100, yPos));
            canvas.Children.Add(CrearCeldaBorde(compra.FechaRegistro?.ToString("dd/MM/yyyy") ?? "N/A", 100, 20, margenIzq + 520, yPos));
            yPos += 30;

            // ===== TABLA DE PRODUCTOS =====
            double[] anchoColumnas = { 70, 240, 130, 60, 40, 80 }; // Suma = 620
            string[] encabezados = { "Código", "Producto", "Laboratorio", "Precio", "Cant.", "Total" };

            double xPosTabla = margenIzq;
            for (int i = 0; i < encabezados.Length; i++)
            {
                Border headerCell = CrearCeldaBorde(encabezados[i], anchoColumnas[i], 18, xPosTabla, yPos, FontWeights.Bold);
                headerCell.Background = new SolidColorBrush(Color.FromRgb(240, 240, 240));
                canvas.Children.Add(headerCell);
                xPosTabla += anchoColumnas[i];
            }
            yPos += 18;

            foreach (var detalle in detalles)
            {
                xPosTabla = margenIzq;
                // Usar CrearCeldaSinBordeHorizontal con GROSOR_LINEA
                canvas.Children.Add(CrearCeldaSinBordeHorizontal(detalle.LOTES?.PRODUCTOS?.Codigo ?? "", anchoColumnas[0], 14, xPosTabla, yPos));
                xPosTabla += anchoColumnas[0];
                canvas.Children.Add(CrearCeldaSinBordeHorizontal(detalle.LOTES?.PRODUCTOS?.Nombre ?? "", anchoColumnas[1], 14, xPosTabla, yPos));
                xPosTabla += anchoColumnas[1];
                canvas.Children.Add(CrearCeldaSinBordeHorizontal(detalle.LOTES?.PRODUCTOS?.LABORATORIOS?.Nombre ?? "", anchoColumnas[2], 14, xPosTabla, yPos));
                xPosTabla += anchoColumnas[2];
                canvas.Children.Add(CrearCeldaSinBordeHorizontal($"{detalle.PrecioCompraUnitario:F2}", anchoColumnas[3], 14, xPosTabla, yPos, FontWeights.Normal, TextAlignment.Right));
                xPosTabla += anchoColumnas[3];
                canvas.Children.Add(CrearCeldaSinBordeHorizontal($"{detalle.Cantidad}", anchoColumnas[4], 14, xPosTabla, yPos, FontWeights.Normal, TextAlignment.Center));
                xPosTabla += anchoColumnas[4];
                canvas.Children.Add(CrearCeldaSinBordeHorizontal($"{detalle.MontoTotal:F2}", anchoColumnas[5], 14, xPosTabla, yPos, FontWeights.Normal, TextAlignment.Right));
                yPos += 14;
            }

            int filasVacias = Math.Max(0, 45 - detalles.Count);
            for (int i = 0; i < filasVacias; i++)
            {
                xPosTabla = margenIzq;
                for (int j = 0; j < anchoColumnas.Length; j++)
                {
                    // Usar CrearCeldaSinBordeHorizontal con GROSOR_LINEA
                    canvas.Children.Add(CrearCeldaSinBordeHorizontal("", anchoColumnas[j], 14, xPosTabla, yPos));
                    xPosTabla += anchoColumnas[j];
                }
                yPos += 14;
            }

            // Borde inferior
            Line lineaInferior = new Line
            {
                X1 = margenIzq,
                Y1 = yPos,
                X2 = margenIzq + anchoColumnas.Sum(),
                Y2 = yPos,
                Stroke = Brushes.Black,
                StrokeThickness = GROSOR_LINEA // Usar grosor constante
            };
            canvas.Children.Add(lineaInferior);
            yPos += 15;

            // ===== TOTALES =====
            string montoLetras = "SON " + NumeroALetras.Convertir(compra.MontoTotal ?? 0);
            canvas.Children.Add(CrearCeldaBorde(montoLetras, 450, 30, margenIzq, yPos, FontWeights.Normal, TextAlignment.Left));
            canvas.Children.Add(CrearCeldaBorde($"Total: S/ {compra.MontoTotal:F2}", 170, 30, margenIzq + 450, yPos, FontWeights.Bold, TextAlignment.Right));
            yPos += 45;

            // ===== RECIBIDO =====
            double anchoRecibido = 250;
            Border borderRecibido = new Border
            {
                Width = anchoRecibido,
                Height = 80,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(GROSOR_LINEA), // Usar grosor constante
                Background = Brushes.White
            };

            StackPanel stackRecibido = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(10)
            };

            stackRecibido.Children.Add(CrearTexto("RECIBIDO", 12, FontWeights.Bold, TextAlignment.Center));
            stackRecibido.Children.Add(new Border { Height = 35 });
            stackRecibido.Children.Add(CrearTexto("Fecha ____ de ______ del 20___", 9, FontWeights.Normal, TextAlignment.Center));

            borderRecibido.Child = stackRecibido;

            Canvas.SetLeft(borderRecibido, margenIzq + (anchoContenidoMax - anchoRecibido) / 2.0);
            Canvas.SetTop(borderRecibido, yPos);
            canvas.Children.Add(borderRecibido);

            fixedPage.Children.Add(canvas);
            ((IAddChild)pageContent).AddChild(fixedPage);
            document.Pages.Add(pageContent);

            return document;
        }

        // Celda SIN bordes horizontales entre filas (solo verticales)
        private static Border CrearCeldaSinBordeHorizontal(string texto, double ancho, double alto, double x, double y,
            FontWeight? peso = null, TextAlignment alineacion = TextAlignment.Left)
        {
            Border border = new Border
            {
                Width = ancho,
                Height = alto,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(GROSOR_LINEA, 0, GROSOR_LINEA, 0), // Usar grosor constante en bordes verticales
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

        private static Border CrearCeldaBorde(string texto, double ancho, double alto, double x, double y,
            FontWeight? peso = null, TextAlignment alineacion = TextAlignment.Left)
        {
            Border border = new Border
            {
                Width = ancho,
                Height = alto,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(GROSOR_LINEA), // Usar grosor constante
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
                Margin = new Thickness(0, 1, 0, 1),
                TextWrapping = TextWrapping.Wrap
            };
        }
    }
}