using SelectPdf;

namespace FastUntility.Core.Base
{
    /// <summary>
    /// pdf转换
    /// </summary>
    public static class BasePdf
    {
        #region to pdf
        /// <summary>
        /// to pdf
        /// </summary>
        /// <param name="url">url地址</param>
        /// <param name="width">宽</param>
        /// <param name="height">高</param>
        /// <returns></returns>
        public static byte[] ToPdf(string url, int width, int height)
        {
            var convert = new HtmlToPdf();

            convert.Options.PdfPageSize = PdfPageSize.A4;
            convert.Options.PdfPageOrientation = PdfPageOrientation.Portrait;
            convert.Options.WebPageWidth = width;
            convert.Options.WebPageHeight = height;
            convert.Options.MarginBottom = 5;
            convert.Options.MarginLeft = 5;
            convert.Options.MarginRight = 5;
            convert.Options.MarginTop = 5;

            var document = convert.ConvertUrl(url);
            var result = document.Save();
            document.Close();

            return result;
        }
        #endregion
    }
}
