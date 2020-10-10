using ExcelDataReader;
using System;
using System.Data;
using System.IO;
using VA.Framework.Common.Helpers.Definition;
using VA.Framework.Common.Helpers.Enums;

namespace VA.Framework.Common.Helpers.Implementation
{
    public class ExcelDataReaderHelper : IExcelDataReaderHelper
    {
        public DataSet GetDataFromExcel(Stream stream, string fileName, bool useHeaderRow)
        {
            ExcelFileExtension extension = GetExtension(fileName);
            using IExcelDataReader excelReader = GetExcelReader(extension, stream);
            DataSet ds = excelReader.AsDataSet(new ExcelDataSetConfiguration
            {
                ConfigureDataTable = (_) => new ExcelDataTableConfiguration
                {
                    UseHeaderRow = useHeaderRow
                }
            });

            return ds;
        }

        private ExcelFileExtension GetExtension(string fileName)
        {
            if (fileName.EndsWith("xls"))
                return ExcelFileExtension.Xls;
            else if (fileName.EndsWith("xlsx"))
                return ExcelFileExtension.Xlsx;
            else
                return ExcelFileExtension.Unknown;
        }

        private static IExcelDataReader GetExcelReader(ExcelFileExtension extension, Stream stream) => extension switch
        {
            ExcelFileExtension.Xls => ExcelReaderFactory.CreateBinaryReader(stream),
            ExcelFileExtension.Xlsx => ExcelReaderFactory.CreateOpenXmlReader(stream),
            _ => throw new ArgumentException("Invalid File Extension"),
        };

    }
}
