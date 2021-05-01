using System;
using ClosedXML.Excel;
using Maestria.Extensions;
using Maestria.TypeProviders;

namespace ExcelSample
{
    [ExcelProvider]
    public partial class ExcelDto
    {
    }
    
    public static class ExcelExtensions
    {
        public static int ColumnUsedCount(this IXLWorksheet sheet)
        {
            return sheet.LastColumnUsed().ColumnNumber();
        }

        public static int ColumnByName(this IXLWorksheet sheet, string columnName)
        {
            if (columnName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(columnName), "Informe o nome da coluna para obter posição!");
            if (sheet.RowCount() <= 0) return 0;
            for (var i = 1; i <= sheet.ColumnUsedCount(); i++)
                if (sheet.Row(1).Cell(i).Value.ToString()?.ToUpper() == columnName.ToUpper()) return i;
            return 0;
        }
    }
}
