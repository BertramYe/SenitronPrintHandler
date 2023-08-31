using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using O2S.Components.PDFRender4NET;
using System.IO;

namespace SenitronPrintHandler

{
    public enum Definition
    {
        One = 1, Two = 2, Three = 3, Four = 4, Five = 5, Six = 6, Seven = 7, Eight = 8, Nine = 9, Ten = 10
    }


    internal class TransferPdf2Images
    {
        /// <summary>
        /// transfer the PDF's onePages into picture
        /// </summary>
        /// <param name="pdfInputPath">PDF input path</param>
        /// <param name="imageOutputPath">out image path</param>
        /// <param name="imageName">new image name</param>
        /// <param name="PageNum">transfer the page into the image </param>
        /// <param name="imageFormat">the new image format </param>
        /// <param name="definition">Picture clarity, the definetion bigger, the result grater </param>
        public static string CovertPDF2ImageFromOnePage(string pdfInputPath, string imageOutputPath, string imageName, int PageNum, System.Drawing.Imaging.ImageFormat imageFormat, Definition definition, Rectangle newPagereact)
        {

            string outImagePath = "";

            // open pdf
            PDFFile pdfFile = PDFFile.Open(pdfInputPath);

            // check the outout image path
            if (!Directory.Exists(imageOutputPath))
            {
                Directory.CreateDirectory(imageOutputPath);
            }
            // start to convert each page
            //Bitmap pageImage = pdfFile.GetPageImage(PageNum - 1, 56 * (int)definition);
            Bitmap pageImage = pdfFile.GetPageImage(PageNum - 1, 56 * (int)definition);
            //new_page = pageImage.Clone();
            // Bitmap newbitmap = pageImage.Clone(new Rectangle(0, 0, pageImage.Width, pageImage.Height), System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
            Bitmap newbitmap = pageImage.Clone(newPagereact, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
            //pageImage.Save(imageOutputPath + imageName + "." + imageFormat.ToString(), imageFormat);
            outImagePath = imageOutputPath + imageName + "." + imageFormat.ToString().ToLower();
            newbitmap.Save(outImagePath, imageFormat);

            //pageImage.Dispose();
            newbitmap.Dispose();
            pdfFile.Dispose();
            pageImage.Dispose();

            return outImagePath;
        }



    }
}
