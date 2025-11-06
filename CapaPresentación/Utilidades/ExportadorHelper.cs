using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ClosedXML.Excel;
using Microsoft.Win32;

namespace CapaPresentación.Utilidades
{
    public static class ExportadorHelper
    {
        /// <summary>
        /// Exporta un DataGrid a un archivo Excel, respetando las columnas, cabeceras y valores visibles.
        /// </summary>
        /// <param name="grid">El DataGrid que se va a exportar.</param>
        /// <param name="tituloReporte">El nombre para la hoja y el archivo.</param>
        public static void ExportarDataGridAExcel(DataGrid grid, string tituloReporte)
        {
            // --- 1. VALIDACIÓN ---
            if (grid.ItemsSource == null || !grid.Items.Cast<object>().Any())
            {
                MessageBox.Show("No hay datos para exportar.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // --- 2. DIÁLOGO "GUARDAR COMO..." ---
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Archivos de Excel (*.xlsx)|*.xlsx";
            saveFileDialog.Title = $"Guardar {tituloReporte}";
            saveFileDialog.FileName = $"{tituloReporte}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // --- 3. CREAR EL DATATABLE ---
                    DataTable dt = new DataTable();

                    // --- 4. AÑADIR CABECERAS (COLUMNAS) AL DATATABLE ---
                    // Iteramos solo las columnas visibles y en el orden que se muestran
                    var columnasVisibles = grid.Columns
                        .Where(c => c.Visibility == Visibility.Visible)
                        .OrderBy(c => c.DisplayIndex);

                    foreach (var column in columnasVisibles)
                    {
                        // Usamos el Header del DataGrid como nombre de columna
                        dt.Columns.Add(column.Header.ToString());
                    }

                    // --- 5. AÑADIR FILAS (DATOS) AL DATATABLE ---
                    foreach (var item in grid.Items)
                    {
                        // Ignorar la fila de "Nuevo Item" si está presente
                        if (item == CollectionView.NewItemPlaceholder || item == null) continue;

                        DataRow newRow = dt.NewRow();

                        foreach (var column in columnasVisibles)
                        {
                            // Obtenemos el valor de la celda usando el Binding
                            string cellValue = GetValorCelda(item, column);
                            newRow[column.Header.ToString()] = cellValue;
                        }
                        dt.Rows.Add(newRow);
                    }

                    // --- 6. CREAR EL LIBRO DE EXCEL Y AÑADIR EL DATATABLE ---
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add(tituloReporte);

                        // Insertar el DataTable. Esto usa nuestras cabeceras
                        // y NO añade la columna de secuencia numérica.
                        worksheet.Cell(1, 1).InsertTable(dt);

                        worksheet.Columns().AdjustToContents(); // Ajustar columnas
                        workbook.SaveAs(saveFileDialog.FileName);

                        MessageBox.Show("Reporte exportado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ocurrió un error al exportar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Obtiene el valor de una celda para un item y columna específicos.
        /// </summary>
        private static string GetValorCelda(object item, DataGridColumn column)
        {
            if (column is DataGridBoundColumn boundColumn)
            {
                var binding = boundColumn.Binding as Binding;
                if (binding != null)
                {
                    // Usar el Path del Binding para obtener el valor por reflexión
                    string propertyPath = binding.Path.Path;
                    object propValue = GetValorPropiedad(item, propertyPath);

                    if (propValue != null)
                    {
                        // Aplicar el StringFormat si existe (ej: para fechas o monedas)
                        if (!string.IsNullOrEmpty(binding.StringFormat))
                        {
                            return string.Format(binding.StringFormat, propValue);
                        }
                        return propValue.ToString();
                    }
                }
            }
            // Fallback para columnas de plantilla (DataGridTemplateColumn)
            // Esto es menos fiable pero puede funcionar
            try
            {
                var cellContent = column.GetCellContent(item);
                if (cellContent is TextBlock tb)
                {
                    return tb.Text;
                }
            }
            catch { }

            return string.Empty; // Valor por defecto
        }

        /// <summary>
        /// Función auxiliar de reflexión para obtener valores de propiedades,
        /// incluyendo propiedades anidadas (ej: "CATEGORIAS.Nombre").
        /// </summary>
        private static object GetValorPropiedad(object obj, string propertyName)
        {
            if (obj == null) return null;

            object currentObj = obj;

            // Manejar propiedades anidadas (ej: "CATEGORIAS.Nombre")
            foreach (var part in propertyName.Split('.'))
            {
                if (currentObj == null) return null;
                Type type = currentObj.GetType();
                PropertyInfo info = type.GetProperty(part);
                if (info == null) return null;
                currentObj = info.GetValue(currentObj, null);
            }

            return currentObj;
        }
    }
}   