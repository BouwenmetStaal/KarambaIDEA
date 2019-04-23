using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using _Excel = Microsoft.Office.Interop.Excel;

namespace WindowsFormsApp1
{
    class Excel
    {
        string path = "";
        _Application excel = new _Excel.Application();
        Workbook wb;
        Worksheet ws;


        public Excel(string path, int Sheet)
        {
            this.path = path;
            wb = excel.Workbooks.Open(path);
            ws = wb.Worksheets[Sheet];
        }

        public string Readcell(int i, int j)
        {
            //excel start form cell [1,1] bottom two lines add one because sharp start at zero
            i++;
            j++;
            if (ws.Cells[i, j].Value2 != null)
                return ws.Cells[i, j].Value2;
            else
                return "";

        }
        public void WritetoCell(int i, int j, double s)
        {
            i++;
            j++;
            //ws.Cells[i, j].Value2 = s;
            //ws.Cells[i, j] = s;
            ws.Cells[i, j].Value2 = s;
        }

        public void WritetoCellstring(int i, int j, string s)
        {
            i++;
            j++;
            //ws.Cells[i, j].Value2 = s;
            //ws.Cells[i, j] = s;
            ws.Cells[i, j].Value2 = s;
        }

        public void Save()
        {
            wb.Save();
        }

        public void SaveAs(string path)
        {
            wb.SaveAs(path);
        }

        public void Close()
        {
            wb.Close();
        }
    }
}
