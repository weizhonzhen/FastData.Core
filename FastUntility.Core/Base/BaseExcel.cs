using FastUntility.Core.Base;
using NPOI.HPSF;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System;
using System.Collections.Generic;
using System.IO;

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
            public HSSFWorkbook workbook { get; set; }

            /// <summary>
            /// 工作页
            /// </summary>
            public ISheet sheet { get; set; }

            /// <summary>
            /// 行
            /// </summary>
            public IRow row { get; set; }

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
        public static ExcelModel Init(string headerText, Dictionary<string, object> title)
        {
            try
            {
                var result = new ExcelModel();

                result.workbook = new HSSFWorkbook();
                InitializeWorkbook(result.workbook);
                result.sheet = result.workbook.CreateSheet(headerText);

                //写入总标题，合并居中
                result.row = result.sheet.CreateRow(0);
                result.cell = result.row.CreateCell(0);
                result.cell.SetCellValue(headerText);

                var style = result.workbook.CreateCellStyle();
                style.Alignment = HorizontalAlignment.Center;
                var font = result.workbook.CreateFont();
                font.FontHeight = 20 * 20;
                style.SetFont(font);
                result.cell.CellStyle = style;
                result.sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, title.Count - 1));

                //插入列标题
                result.row = result.sheet.CreateRow(1);
                int i = 0;

                foreach (var item in title)
                {
                    result.cell = result.row.CreateCell(i++);
                    result.cell.Row.Height = 420;
                    result.cell.SetCellValue(item.Value.ToStr());
                    result.cell.CellStyle = GetStyle(result.workbook, true);
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
        public static void FillContent(List<Dictionary<string, object>> listContent, ExcelModel model, string exclude = "")
        {
            try
            {
                //插入查询结果
                var i = 0;
                if (listContent != null)
                {
                    model.style_n = GetStyle(model.workbook, false, true);
                    model.style = GetStyle(model.workbook);
                    foreach (var item in listContent)
                    {
                        model.row = model.sheet.CreateRow(i + 2);
                        int j = 0;
                        foreach (var temp in item)
                        {
                            if (temp.Key.ToLower() == exclude.ToLower())
                                continue;

                            model.cell = model.row.CreateCell(j++);
                            model.cell.Row.Height = 420;
                            model.cell.SetCellValue(temp.Value.ToStr());

                            if (temp.Value.ToStr().Contains("\n"))
                                model.cell.CellStyle = model.style_n;
                            else
                                model.cell.CellStyle = model.style;
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
                foreach (var item in title)
                {
                    model.sheet.AutoSizeColumn(i++, true);
                    model.sheet.Autobreaks = true;
                    model.sheet.HorizontallyCenter = true;
                }

                var file = new MemoryStream();
                model.workbook.Write(file);

                return file.ToArray();
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
        private static void InitializeWorkbook(HSSFWorkbook hssfworkbook)
        {
            //信息
            var dsi = PropertySetFactory.CreateDocumentSummaryInformation();
            dsi.Company = "";
            hssfworkbook.DocumentSummaryInformation = dsi;

            //工区名称
            var si = PropertySetFactory.CreateSummaryInformation();
            si.Subject = "";
            hssfworkbook.SummaryInformation = si;
        }
        #endregion

        #region 样式
        /// <summary>
        /// 样式
        /// </summary>
        /// <returns></returns>
        private static ICellStyle GetStyle(HSSFWorkbook hssfworkbook, bool IsHead = false, bool IsWrapText = false)
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
