using System.Data;
using System.IO;

namespace VA.Framework.Common.Helpers.Definition
{
    public interface IExcelDataReaderHelper
    {
        DataSet GetDataFromExcel(Stream stream, string fileName, bool useHeaderRow);
    }
}
