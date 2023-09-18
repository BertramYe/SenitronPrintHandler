using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using MoonPdfLib;
using O2S.Components.PDFRender4NET;
using O2S.Components.PDFView4NET;



namespace SenitronPrintHandler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        // Logfile
        LogHelper appLog = new LogHelper("appRunning");
        // the select retangle positions
        private Point startPoint;
        private bool isSelecting;
        private bool canSelectedOrNot;
        //private bool doubleclicked;

        // tohandlePdfPath
        string toHandlePdfPath = "";

        // the PDF loaded or not
        private bool pdfLoaded;
        private bool m_bLoaded = false;


        // get currentzoom
        float currentZoom = 1;

        // pageSize 
        double[] pdfPageSize;

        List<string> toSetValueNames = new List<string> { "AssetId","Sku","EpcAltSerial","EpcAttrib1",
                                                                            "EpcAttrib2","EpcAttrib3","EpcAttrib4","EpcAttrib5",
                                                                            "EpcAttrib6","EpcAttrib7","EpcAttrib8","EpcAttrib9",
                                                                            "EpcAttrib10","EpcContainerQty"};


        //ini.InitReadFile("setup.ini");
        //string toReadpdfPath = ini.GetConfigInfo("ToScanPdfPath");
        //PdfFolderWatcher pdfwatcher = new PdfFolderWatcher(toReadpdfPath);
        IniFile ini = new IniFile();

        private Task _pdf_task;
        private static CancellationTokenSource _cancellationTokenSource;
        public MainWindow()
        {
            InitializeComponent();
            appLog.WriteLog($"app SenitronPrintHandler Start !");

            ini.ResetSetUpFile(ini, toSetValueNames);
            //appLog.WriteLog($"app SenitronPrintHandler Start !");
        }




        private void Button_Click(object sender, RoutedEventArgs e)
        {

            string defaultPdfPathDic = @"C:\";

            string actualPdfPath = GetPDFPathDialog(defaultPdfPathDic);


            //ReadPDF(fileName);
            pdfLoaded = LoadPdfDoc(actualPdfPath);
            // open the retango selected functions
            //canSelectedOrNot = true;
            //toHandlePdfPath = fileName;
            if (pdfLoaded)
            {
                int pageNumber = this.MoonPdfPanel.GetCurrentPageNumber();

                pdfPageSize = GetPDFPageSize(actualPdfPath, pageNumber);


                //MessageBox.Show($"scanCoo: {pdfPageSize[0]}, {pdfPageSize[1]}");
                toHandlePdfPath = actualPdfPath;
                canSelectedOrNot = true;

                this.MoonPdfPanel.Width = pdfPageSize[0];
                this.MoonPdfPanel.Height = pdfPageSize[1];
                this.DrawRectArea.Width = this.MoonPdfPanel.Width;
                this.DrawRectArea.Height = this.MoonPdfPanel.Height;


                this.DrawRectArea.Visibility = Visibility.Visible;
                this.ZoomOutButton.IsEnabled = true;
                this.ZoomInButton.IsEnabled = true;

            }
            else
            {
                canSelectedOrNot = false;
                this.DrawRectArea.Visibility = Visibility.Collapsed;
                this.ZoomOutButton.IsEnabled = false;
                this.ZoomInButton.IsEnabled = false;
            }

        }


        private string GetPDFPathDialog(string defaltPdfPathDic)
        {

            string pdfFilePath = "";

            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.InitialDirectory = defaltPdfPathDic;
            openFileDialog.Filter = "All files (*.*)|*.*";   // file type
            openFileDialog.RestoreDirectory = false;   // false stands open the same directory as the last time

            if (openFileDialog.ShowDialog() == false)  // if not open file
            {
                return pdfFilePath;
            }
            else
            {
                // get the file name and path
                pdfFilePath = openFileDialog.FileName;
            }



            // just only pdf files
            string fileExtentionName = System.IO.Path.GetExtension(pdfFilePath); // PDF

            if (fileExtentionName != ".pdf")
            {
                MessageBox.Show("Only PDF files can be handled, please open the right file!");
                return "";
            }
            return pdfFilePath;
        }




        public bool LoadPdfDoc(string strPdfPath)
        {
            try
            {
                this.MoonPdfPanel.OpenFile(strPdfPath);
                this.MoonPdfPanel.Zoom(1.0);
                //this.MoonPdfPanel.ZoomToWidth();
                m_bLoaded = true;
                this.ShowZoomPercent.Content = (currentZoom * 100).ToString() + "%";
                // forbiden MouseWheel
                //this.MoonPdfPanel.PreviewMouseWheel -= (sender, e) => { };


            }

            catch
            {
                m_bLoaded = false;
            }

            return m_bLoaded;
        }



        public double[] GetPDFPageSize(string pdfFilePath, int pageNumber)
        {
            double[] pdfPageSize = new double[2];

            PDFFile pdfFile = PDFFile.Open(pdfFilePath);
            if (pageNumber >= 1 && pageNumber <= pdfFile.PageCount)
            {
                O2S.Components.PDFRender4NET.PDFSize pdfPageSizes = pdfFile.GetPageSize(pageNumber);

                double pageWidth = pdfPageSizes.Width;
                double pageHeight = pdfPageSizes.Height;


                //Console.WriteLine($"Page {pageNumber} width: {pageWidth}");
                //Console.WriteLine($"Page {pageNumber} height: {pageHeight}");
                pdfPageSize[0] = pageWidth;
                pdfPageSize[1] = pageHeight;

            }

            pdfFile.Dispose();

            return pdfPageSize;
        }




        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && canSelectedOrNot)
            {
                startPoint = e.GetPosition(canvas);
                selectionBox.Width = 0;
                selectionBox.Height = 0;
                Canvas.SetLeft(selectionBox, startPoint.X);
                Canvas.SetTop(selectionBox, startPoint.Y);
                selectionBox.Visibility = Visibility.Visible;
                isSelecting = true;
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isSelecting)
            {
                Point endPoint = e.GetPosition(canvas);
                double width = endPoint.X - startPoint.X;
                double height = endPoint.Y - startPoint.Y;

                selectionBox.Width = width >= 0 ? width : 0;
                selectionBox.Height = height >= 0 ? height : 0;

                Canvas.SetLeft(selectionBox, width >= 0 ? startPoint.X : endPoint.X);
                Canvas.SetTop(selectionBox, height >= 0 ? startPoint.Y : endPoint.Y);


            }
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released && isSelecting)
            {
                isSelecting = false;

                // get the coodites info
                double x = Canvas.GetLeft(selectionBox);
                double y = Canvas.GetTop(selectionBox);
                double width = selectionBox.Width;
                double height = selectionBox.Height;

                //MessageBox.Show($"scanCoo: {x}, {y}, {width}, {height}");

                if (width > 0 && height > 0)
                {
                    //Dictionary<string, int[]> CooDict = new Dictionary<string, int[]>();
                    double[] getCoo = new double[4];
                    getCoo[0] = x;
                    getCoo[1] = y;
                    getCoo[2] = width;
                    getCoo[3] = height;

                    //int[] ActualCoo = CaculateActualCoo(x, y, width, height);
                    ShowRadioButtonDialog(getCoo);
                }

                selectionBox.Visibility = Visibility.Collapsed;
            }
        }

        public void ShowRadioButtonDialog(double[] selectedCoo)
        {
            // define dialog
            RadioButtonDialog radioButtonDialog = new RadioButtonDialog();
            radioButtonDialog.Message = " which Coodinates do you want to update ?";


            // <RadioButton x:Name="Option10RadioButton" Content="EpcAttrib4-Extract" GroupName="Options"
            // IsChecked="{Binding SetIsOptionChecked, Mode=TwoWay}" Margin="20,10,20,10" />


            for (int i = 0; i < toSetValueNames.Count; i++)
            {
                RadioButton radioButton = new RadioButton();
                radioButton.Content = toSetValueNames[i];
                radioButton.Name = $"RadioButton{i}";
                radioButton.Margin = new Thickness(20, 10, 20, 10);
                radioButton.GroupName = "Options";  // Set the group name to ensure they work as a radio group

                // binding check events
                //radioButton.Checked += radioButtonDialog.RadioButtonChecked;

                radioButtonDialog.radioButtonContainer.Children.Add(radioButton);
            }


            // create a dialog via class Windows
            var dialogWindow = new Window
            {
                Title = "Choose a Coodinate to update!",
                Content = radioButtonDialog,
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Application.Current.MainWindow,
                ShowInTaskbar = false
            };

            // show dialog
            bool? result = dialogWindow.ShowDialog();



            if (result.HasValue && result.Value && result == true)
            {


                for (int i = 0; i < toSetValueNames.Count; i++)
                {
                    if (radioButtonDialog.GetRadioButtonIsChecked($"RadioButton{i}"))
                    {
                        // only one option can be update per time
                        int[] ActualCoo = CaculateActualCoo(selectedCoo, i);

                        ini.ReplaceValue("setup.ini", toSetValueNames[i], $"{ActualCoo[0]},{ActualCoo[1]},{ActualCoo[2]},{ActualCoo[3]}");
                        appLog.WriteLog($"the {toSetValueNames[i]} has been updated!");
                        MessageBox.Show($"the {toSetValueNames[i]} has been updated!");
                        break;
                    }
                }



            }
            else
            {
                appLog.WriteLog($"update has been cancel !");
                MessageBox.Show("update has been cancel !");

            }


            // get the use select result 
            //return result.HasValue && result.Value;
        }


        private int[] CaculateActualCoo(double[] ScanCoord, int type)
        {

            //MessageBox.Show($"scanCoo: {scanCooX}, {scanCooY}, {scanCooW}, {scanCooH},actual zoom: {currentZoom}," +
            //    $"Actual Scan code:{scanCooX / currentZoom},{scanCooY / currentZoom},{scanCooW / currentZoom},{scanCooH / currentZoom}");


            // get final result 
            int[] ActualCoo = new int[4];


            double avavTimes = 7.8;

            double ActualX1;
            double ActualY1;
            double ActualW;
            double ActualH;
            
            ActualX1 = (ScanCoord[0] / currentZoom) * avavTimes;
            ActualY1 = (ScanCoord[1] / currentZoom) * avavTimes;

            ActualW = (ScanCoord[2] / currentZoom) * avavTimes;
            ActualH = (ScanCoord[3] / currentZoom) * avavTimes;

            ActualCoo[0] = Convert.ToInt32(ActualX1) > 0 ? Convert.ToInt32(ActualX1) : 0;
            ActualCoo[1] = Convert.ToInt32(ActualY1) > 0 ? Convert.ToInt32(ActualY1) : 0;
            ActualCoo[2] = Convert.ToInt32(ActualW) > 0 ? Convert.ToInt32(ActualW) : 0;
            ActualCoo[3] = Convert.ToInt32(ActualH) > 0 ? Convert.ToInt32(ActualH) : 0;

            return ActualCoo;
        }







        private void HandlePdf(object sender, RoutedEventArgs e)
        {
            string defaultPdfPath = @"C:\";
            string pdfpath = GetPDFPathDialog(defaultPdfPath);

            // there should be an update for where to save image

            if (pdfpath != "")
            {
                if (MessageBox.Show("are you sure to handle the PDF which you choosen？", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    //MessageBox.Show("start process the PDF files","Tips");
                    appLog.WriteLog($"start to handle pdf {pdfpath}");
                    PdfProcessor.Process(pdfpath);
                    appLog.WriteLog($"finised process handle PDF files {pdfpath}");
                    MessageBox.Show($"finised process handle PDF files !");


                }
                else
                {
                    appLog.WriteLog($"process handle PDF file {pdfpath} has been canceled!");
                    MessageBox.Show($"process handle PDF file has been canceled!");
                }
            }


        }

        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            if (m_bLoaded)
            {

                MoonPdfPanel.ZoomOut();
                currentZoom = MoonPdfPanel.CurrentZoom;
                this.ShowZoomPercent.Content = (currentZoom * 100).ToString() + "%";

                // make the draw area is the same as PDFpage
                this.MoonPdfPanel.Width = pdfPageSize[0] * currentZoom;
                this.MoonPdfPanel.Height = pdfPageSize[1] * currentZoom;
                this.DrawRectArea.Width = this.MoonPdfPanel.Width;
                this.DrawRectArea.Height = this.MoonPdfPanel.Height;
            }

        }

        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            if (m_bLoaded)
            {

                MoonPdfPanel.ZoomIn();
                currentZoom = MoonPdfPanel.CurrentZoom;
                this.ShowZoomPercent.Content = (currentZoom * 100).ToString() + "%";

                // make the draw area is the same as PDFpage
                this.MoonPdfPanel.Width = pdfPageSize[0] * currentZoom;
                this.MoonPdfPanel.Height = pdfPageSize[1] * currentZoom;
                this.DrawRectArea.Width = this.MoonPdfPanel.Width;
                this.DrawRectArea.Height = this.MoonPdfPanel.Height;



            }

        }

        private void HandleThreading(object sender, RoutedEventArgs e)
        {
            string butttonContent = this.BackgroundRunningButton.Content.ToString();
            if (butttonContent == "StopRunning")
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                if (_pdf_task.IsCompletedSuccessfully)
                {
                    _pdf_task.Dispose();
                }



                this.BackgroundRunningButton.Content = "StartRunning";
            }
            if (butttonContent == "StartRunning")
            {
                _cancellationTokenSource = new CancellationTokenSource();
                var dueTime1 = TimeSpan.FromSeconds(1);
                var interval1 = TimeSpan.FromSeconds(1);
                //_cancellationTokenSource.Cancel();
                _pdf_task = RunPeriodicAsync(DoWork, dueTime1, dueTime1, _cancellationTokenSource.Token);
                this.BackgroundRunningButton.Content = "StopRunning";
            }
        }


        private static async Task RunPeriodicAsync(Action onTick,
                                                 TimeSpan dueTime,
                                                 TimeSpan interval,
                                                 CancellationToken token)
        {
            // Initial wait time before we begin the periodic loop.
            if (dueTime > TimeSpan.Zero)
                await Task.Delay(dueTime, token);

            // Repeat this loop until cancelled.
            while (!token.IsCancellationRequested)
            {
                // Call our onTick function.
                onTick?.Invoke();

                // Wait to repeat again.
                if (interval > TimeSpan.Zero)
                    await Task.Delay(interval, token);
            }
        }

        private void DoWork()
        {


            appLog.WriteLog($"test threading !!!!");


        }



    }
}
