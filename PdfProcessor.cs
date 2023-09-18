using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text.pdf;
using Tesseract;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows;
using System.Drawing;

namespace SenitronPrintHandler
{
    internal class PdfProcessor
    {
        public static void Process(string pdfFilePath)
        {
            // Load the setup.ini file
            IniFile ini = new IniFile();
            ini.InitReadFile("setup.ini");
            // get application running times
            string appRunningTimes = ini.GetConfigInfo("ApplicationRunningTimes");
            // log file 
            LogHelper pdfHandlelog = new LogHelper($"HandlePdf{appRunningTimes}");
            pdfHandlelog.WriteLog($"begin to handle the PDF Files {pdfFilePath}!");

            // final log 
            LogHelper finalResultLog = new LogHelper($"jobs");


            // Get label-extract and ItemCode-extract coordinates from the INI file
            List<string> targetCoord = new List<string> {"AssetId","Sku","EpcAltSerial","EpcAttrib1","EpcAttrib2",
                                      "EpcAttrib3","EpcAttrib4","EpcAttrib5","EpcAttrib6","EpcAttrib7",
                                       "EpcAttrib8","EpcAttrib9","EpcAttrib10","EpcContainerQty"};

            // ALL COOR DICT
            Dictionary<string, System.Drawing.Rectangle> rectDic = ReadCoo(targetCoord, ini);

            string[] filter_list = new string[targetCoord.Count];

            for (int i = 0; i < filter_list.Length; i++)
            {
                if (i < 2 )
                {
                    filter_list[i] = @"[^0-9A-Z\-]";
                }
                else if (i == 2)
                {
                    filter_list[i] = @"[^0-9A-Z]";
                }
                else if (i < 13)
                {
                    filter_list[i] = @"[^0-9A-Za-z\-\,:\/\.\s]";
                }
                else if (i == 13)
                {
                    filter_list[i] = @"[^0-9]";
                }
                else
                {
                    filter_list[i] = "";
                }
            }



            // get the final image path
            //string finalImagePath = "./Images/";
            //string finalImagePath = ini.GetConfigInfo("FinalImagePath");
            //if (!Directory.Exists(finalImagePath))
            //{
            //    Directory.CreateDirectory(finalImagePath);
            //}

            using (PdfReader reader = new PdfReader(pdfFilePath))
            {

                // save the item code per page
                Dictionary<string, string> pageItemCountsDirc = new Dictionary<string, string>();
                // handler the PDF files

                // pdf total pagenumbers
                int numPages = reader.NumberOfPages;
                reader.Close();
                
                for (int page = 1; page <= numPages; page++)
                {

                    pdfHandlelog.WriteLog($"start read page {page}!");
                    pageItemCountsDirc = ExtractAllItemCode(page, pdfFilePath,rectDic, targetCoord, filter_list,pdfHandlelog);
                    string infor1 = $"the handle result of the page {page}";
                    string infor2 = $"the handle result of the page {page}";
                    string finalInfor1 = "";
                    string finalInfor2 = "";
                    
                    for(int i = 0; i < targetCoord.Count; i++)
                    {
                        infor1 += $",{targetCoord[i]}";
                        infor2 += $",\"{pageItemCountsDirc[targetCoord[i]]}\"";
                       
                    }
                    pdfHandlelog.WriteLog($"{infor1} !");
                    pdfHandlelog.WriteLog($"{infor2} !");

                    for (int i = 0; i < targetCoord.Count; i++)
                    {
                        finalInfor1 += $",{targetCoord[i]}";
                        finalInfor2 += $",\"{pageItemCountsDirc[targetCoord[i]]}\"";
                    }

                    finalResultLog.WriteLog(finalInfor1);
                    finalResultLog.WriteLog(finalInfor2);

                    pdfHandlelog.WriteLog($"end read page {page}!");

                }
                //Console.WriteLine("finished the reading of the PDF");
                
            }
            ini.ResetSetUpFile(ini, targetCoord);
            pdfHandlelog.WriteLog($"finished to handle the PDF Files {pdfFilePath} !");
            //MessageBox.Show($"finished to handle the PDF Files {pdfFilePath} !");
        }


        private static Dictionary<string, string> ExtractAllItemCode(int page, string pdfFilePath, Dictionary<string, System.Drawing.Rectangle> rectDic, List<string> targetCoord,string[] filter_list, LogHelper pdfHandlelog)
        {
            Dictionary<string, string> pageItemcodeDic = new Dictionary<string, string>();
                                                                       
            for (int i = 0; i < targetCoord.Count; i++)
            {
                string tohandleImagePath = CutOffImagesFromPDF(page, pdfFilePath, rectDic, targetCoord[i], pdfHandlelog);
               
                if(tohandleImagePath != "")
                {
                    string ItemCode = ExtractItemCode(tohandleImagePath, filter_list[i], pdfHandlelog, targetCoord[i]);
                    //pageItemcodeDic[page] = targetCoord[i];
                    //itemCodeList[i] = ItemCode;
                    pageItemcodeDic[targetCoord[i]] = ItemCode;
                    File.Delete(tohandleImagePath);
                }
                else
                {
                    pageItemcodeDic[targetCoord[i]] = "";
                }

            }

           
            return pageItemcodeDic;

        }


        private static string CutOffImagesFromPDF(int page, string pdfFilePath, Dictionary<string, System.Drawing.Rectangle> rectDic, string reactangleName, LogHelper pdfHandlelog)
        {

            string outImagePath = "";
            if (rectDic[reactangleName].IsEmpty)
            {
                return outImagePath;
            }


            // the temp  cache path 
            string temp_path_dir = "./Temp/";
            if (!Directory.Exists(temp_path_dir))
            {
                Directory.CreateDirectory(temp_path_dir);
            }

            // get the itemcode image
            //System.Drawing.Rectangle itemRect = LableReac;
            string tempImageName = $"temp{reactangleName}{page}";
           
            //targetCoord
            try
            {
                outImagePath = TransferPdf2Images.CovertPDF2ImageFromOnePage(pdfFilePath, temp_path_dir, tempImageName, page, System.Drawing.Imaging.ImageFormat.Bmp, Definition.Ten, rectDic[reactangleName]);


            }
            catch (Exception e)
            {
                string errorInfo = $" the Coordinates of {reactangleName} in the setup.ini for the page {page} was wrong!" +
                    $" please reselect a rectangle and update it in the setup.ini file! By the way, don't zoomIn/zoomOut too much size !!!";
                pdfHandlelog.WriteLog($"{errorInfo}");
                //MessageBox.Show(errorInfo);
            }
            return outImagePath;
        }



        private static string ExtractItemCode(string imagePath, string filter, LogHelper pdfHandlelog, string itemcodeName)
        {

            // get the itemCode from the image in the temp dir
            string itemCode = "";

            if (imagePath == null || imagePath == "")
            {
                return itemCode;   // path not valid
            }

            using (var engine = new TesseractEngine("tessdata", "eng", EngineMode.Default))
            {
                try
                {
                    Pix img = Pix.LoadFromFile(imagePath);
                    using (var pageTesseract = engine.Process(img))
                    {
                        string itemCodetemp = pageTesseract.GetText().Trim();
                        if (filter != null && filter != "")
                        {

                            itemCode = Regex.Replace(itemCodetemp, filter, "");
                        }
                        else
                        {
                            itemCode = itemCodetemp;
                        }
                    }
                    img.Dispose();

                }
                catch (Exception e)
                {
                    pdfHandlelog.WriteLog($" scan the code of  {itemcodeName} failed, Image not existed , path: {imagePath}  error: {e}  !");
                }
                //// delete the temp file
            }

           
            return itemCode;
        }





        private static Dictionary<string, System.Drawing.Rectangle> ReadCoo(List<string> targetCoord, IniFile ini)
        {

            Dictionary<string, System.Drawing.Rectangle> rectDic = new Dictionary<string, System.Drawing.Rectangle>();

            // the other coor
            for (int i = 0; i < targetCoord.Count; i++)
            {
                int[] extractTarget = ini.GetIntArray(targetCoord[i]);
                Rectangle selectedCoord = new Rectangle(extractTarget[0], extractTarget[1], extractTarget[2], extractTarget[3]);
                rectDic[targetCoord[i]] = selectedCoord;
            }
            return rectDic;
        }




    }
}
