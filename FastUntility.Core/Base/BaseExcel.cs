using NPOI.HPSF;
using NPOI.HSSF.UserModel;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FastUntility.Core.Base
{
    public static class BaseExcel
    {
        #region excel实体
        /// <summary>
        /// excel实体
        /// </summary>
        public class ExcelModel
        {
            /// <summary>
            /// 工作区
            /// </summary>
            public XSSFWorkbook workbook { get; set; }

            /// <summary>
            /// 工作页
            /// </summary>
            public ISheet sheet { get; set; }

            /// <summary>
            /// 行
            /// </summary>
            public IRow row { get; set; }

            /// <summary>
            /// 行
            /// </summary>
            public List<IRow> rows { get; set; } = new List<IRow>();

            /// <summary>
            /// 单元格
            /// </summary>
            public ICell cell { get; set; }

            /// <summary>
            /// style
            /// </summary>
            public ICellStyle style { get; set; }

            /// <summary>
            /// style
            /// </summary>
            public ICellStyle style_n { get; set; }
        }
        #endregion

        #region 初始化excel
        /// <summary>
        /// 初始化excel
        /// </summary>
        /// <param name="headerText">标题</param>
        /// <param name="title">表头</param>
        /// <returns></returns>
        public static ExcelModel Init(string headerText, List<Dictionary<string, object>> title1, Dictionary<string, object> title2, int rows = 0)
        {
            try
            {
                var result = new ExcelModel();
                result.workbook = new XSSFWorkbook();
                result.workbook.SetForceFormulaRecalculation(false);
                InitializeWorkbook(result.workbook);
                result.sheet = result.workbook.CreateSheet(headerText);

                result.row = result.sheet.CreateRow(0);
                result.cell = result.row.CreateCell(0);
                result.cell.SetCellValue(headerText);
                var style = result.workbook.CreateCellStyle();
                style.Alignment = HorizontalAlignment.Center;
                var font = result.workbook.CreateFont();
                font.FontHeight = 20 * 20;
                style.SetFont(font);
                result.cell.CellStyle = style;
                result.sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, title2.Count - 1));

                var i = 0;
                if (title1 != null)
                {
                    var step = 0;
                    result.row = result.sheet.CreateRow(1);
                    title1.ToList().ForEach(a =>
                    {
                        if (i == 0)
                            result.cell = result.row.CreateCell(i++);
                        else
                            result.cell = result.row.CreateCell(step);
                        result.cell.Row.Height = 420;
                        result.cell.SetCellValue(a.GetValue("text").ToStr());
                        result.sheet.AddMergedRegion(new CellRangeAddress(1, 1, step, step + a.GetValue("step").ToStr().ToInt(0) - 1));
                        result.cell.CellStyle = GetStyle(result.workbook, true);
                        step = step + a.GetValue("step").ToStr().ToInt(0);
                    });
                }

                result.row = result.sheet.CreateRow(2);
                i = 0;
                title2.ToList().ForEach(a =>
                {
                    result.cell = result.row.CreateCell(i++);
                    result.cell.Row.Height = 420;
                    result.cell.SetCellValue(a.Value.ToStr());
                    result.cell.CellStyle = GetStyle(result.workbook, true);
                });

                for (var j = 0; j < rows; j++)
                {
                    var row = result.sheet.CreateRow(j + 2);
                    row.Height = 420;
                    result.rows.Add(row);
                }

                return result;
            }
            catch (Exception ex)
            {
                BaseLog.SaveLog(ex.ToString(), "ToExcel.Init");
                return null;
            }
        }
        #endregion

        #region 初始化excel
        /// <summary>
        /// 初始化excel
        /// </summary>
        /// <param name="headerText">标题</param>
        /// <param name="title">表头</param>
        /// <returns></returns>
        public static ExcelModel Init(string headerText, Dictionary<string, object> title1, List<Dictionary<string, object>> title2 = null, int rows = 0)
        {
            try
            {
                var result = new ExcelModel();

                result.workbook = new XSSFWorkbook();
                result.workbook.SetForceFormulaRecalculation(false);
                InitializeWorkbook(result.workbook);
                result.sheet = result.workbook.CreateSheet(headerText);

                result.row = result.sheet.CreateRow(0);
                result.cell = result.row.CreateCell(0);
                result.cell.SetCellValue(headerText);
                var style = result.workbook.CreateCellStyle();
                style.Alignment = HorizontalAlignment.Center;
                var font = result.workbook.CreateFont();
                font.FontHeight = 20 * 20;
                style.SetFont(font);
                result.cell.CellStyle = style;
                result.sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, title1.Count - 1));

                result.row = result.sheet.CreateRow(1);
                int i = 0;
                title1.ToList().ForEach(a =>
                {
                    result.cell = result.row.CreateCell(i++);
                    result.cell.Row.Height = 420;
                    result.cell.SetCellValue(a.Value.ToStr());
                    result.cell.CellStyle = GetStyle(result.workbook, true);
                });

                if (title2 != null)
                {
                    var step = 0;
                    i = 0;
                    result.row = result.sheet.CreateRow(2);
                    title2.ToList().ForEach(a =>
                    {
                        if (i == 0)
                            result.cell = result.row.CreateCell(i++);
                        else
                            result.cell = result.row.CreateCell(step);
                        result.cell.Row.Height = 420;
                        result.cell.SetCellValue(a.GetValue("text").ToStr());
                        result.sheet.AddMergedRegion(new CellRangeAddress(2, 2, step, step + a.GetValue("step").ToStr().ToInt(0) - 1));
                        result.cell.CellStyle = GetStyle(result.workbook, true);
                        step = step + a.GetValue("step").ToStr().ToInt(0);
                    });
                }

                for (var j = 0; j < rows; j++)
                {
                    var row = result.sheet.CreateRow(j + (title2 == null ? 2 : 3));
                    row.Height = 420;
                    result.rows.Add(row);
                }
                return result;
            }
            catch (Exception ex)
            {
                BaseLog.SaveLog(ex.ToString(), "ToExcel.Init");
                return null;
            }
        }
        #endregion

        #region 填充内容
        /// <summary>
        /// 填充内容
        /// </summary>
        /// <param name="listContent">内容列表</param>
        /// <param name="model"></param>
        public static void FillContent(List<Dictionary<string, object>> listContent, ExcelModel model, string exclude = "", bool IsSmallTile = false)
        {
            try
            {
                //插入查询结果
                var i = 0;
                if (listContent != null)
                {
                    model.style_n = GetStyle(model.workbook, true, true);
                    model.style = GetStyle(model.workbook);
                    for (var row = 0; row < listContent.Count; row++)
                    {
                        var item = listContent[row];

                        if (model.rows.Count == 0)
                        {
                            if (IsSmallTile)
                                model.row = model.sheet.CreateRow(i + 3);
                            else
                                model.row = model.sheet.CreateRow(i + 2);
                        }
                        else
                            model.row = model.rows[row];

                        int j = 0;

                        foreach (var temp in item)
                        {
                            if (string.Compare(temp.Key, exclude, true) == 0)
                                continue;

                            model.cell = model.row.CreateCell(j++);
                            model.cell.Row.Height = 420;

                            if (temp.Value is Dictionary<string, object>)
                            {
                                var info = temp.Value as Dictionary<string, object>;
                                model.sheet.AddMergedRegion(new CellRangeAddress(
                                    info.GetValue("rowbegin").ToStr().ToInt(0), info.GetValue("rowend").ToStr().ToInt(0),
                                    info.GetValue("colbegin").ToStr().ToInt(0) - 1, info.GetValue("colend").ToStr().ToInt(0) - 1));


                                if (BaseRegular.IsNumericType(info.GetValue("text").GetType()))
                                {
                                    model.cell.SetCellType(CellType.Numeric);
                                    model.cell.SetCellValue(info.GetValue("text").ToStr().ToDouble(0));
                                }
                                else
                                {
                                    model.cell.SetCellType(CellType.String);
                                    model.cell.SetCellValue(info.GetValue("text").ToStr());
                                }

                                if (info.GetValue("text").ToStr().Contains("\n"))
                                    model.cell.CellStyle = model.style_n;
                                else
                                    model.cell.CellStyle = model.style;
                            }
                            else
                            {
                                if (BaseRegular.IsNumericType(temp.Value.GetType()))
                                {
                                    model.cell.SetCellType(CellType.Numeric);
                                    model.cell.SetCellValue(temp.Value.ToStr().ToDouble(0));
                                }
                                else
                                {
                                    model.cell.SetCellType(CellType.String);
                                    model.cell.SetCellValue(temp.Value.ToStr());
                                }
                               
                                var height = 0;
                                var length = System.Text.Encoding.UTF8.GetBytes(model.cell.ToString()).Length;
                                if ((25 * (length / 60 + 1)) > height)
                                    height = 25 * (length / 60 + 1);
                                model.rows[row].HeightInPoints = height;

                                if (temp.Value.ToStr().Contains("\n"))
                                    model.cell.CellStyle = model.style_n;
                                else
                                    model.cell.CellStyle = model.style;
                            }
                        }

                        i++;
                    }
                }
            }
            catch (Exception ex)
            {
                BaseLog.SaveLog(ex.ToString(), "ToExcel.FillContent");
            }
        }
        #endregion

        #region 获取excel流
        /// <summary>
        /// 获取excel流
        /// </summary>
        /// <returns></returns>
        public static byte[] Result(ExcelModel model, Dictionary<string, object> title)
        {
            try
            {
                //自动列宽
                var i = 0;
                title.ToList().ForEach(a =>
                {
                    model.sheet.AutoSizeColumn(i++, true);
                    model.sheet.Autobreaks = true;
                    model.sheet.HorizontallyCenter = true;
                });

                using (var file = new MemoryStream())
                {
                    model.workbook.Write(file);
                    return file.ToArray();
                }
            }
            catch (Exception ex)
            {
                BaseLog.SaveLog(ex.ToString(), "ToExcel.Result");
                return null;
            }
        }
        #endregion

        #region excel工作区
        /// <summary>
        /// 初始化工作区
        /// </summary>
        private static void InitializeWorkbook(XSSFWorkbook hssfworkbook)
        {
            //信息
            var dsi = PropertySetFactory.CreateDocumentSummaryInformation();
            dsi.Company = "";
            //hssfworkbook.DocumentSummaryInformation = dsi;

            //工区名称
            var si = PropertySetFactory.CreateSummaryInformation();
            si.Subject = "";
            //hssfworkbook.SummaryInformation = si;
        }
        #endregion

        #region 样式
        /// <summary>
        /// 样式
        /// </summary>
        /// <returns></returns>
        private static ICellStyle GetStyle(XSSFWorkbook hssfworkbook, bool IsHead = false, bool IsWrapText = false)
        {
            var style = hssfworkbook.CreateCellStyle();
            style.Alignment = HorizontalAlignment.Center;
            style.VerticalAlignment = VerticalAlignment.Center;

            style.BorderLeft = BorderStyle.Thin;
            style.BorderRight = BorderStyle.Thin;
            style.BorderBottom = BorderStyle.Thin;
            style.WrapText = IsWrapText;

            style.Indention = 0;

            if (IsHead)
                style.BorderTop = BorderStyle.Thin;

            return style;
        }
        #endregion    
    }
}
