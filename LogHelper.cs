using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;



namespace SenitronPrintHandler
{
    internal class LogHelper
    {
        //private string logFileName;

        string createdLogFileName;
        string logPath = @$"c:/senitron/printhandler/";

        public LogHelper(string logFileName)
        {
            //string logFilePath = @$"c:/senitron/printhandler/";
            string logFilePath = this.logPath;

            // check the outout image path
            if (!Directory.Exists(logFilePath))
            {
                Directory.CreateDirectory(logFilePath);
            }

            // create log file
            StreamWriter writer = File.AppendText(logFilePath + logFileName + ".log");
            writer.Dispose();
            writer.Close();
            createdLogFileName = logFileName;
        }


        public void WriteLog(string logContent)
        {
            //string logFilePath = @"c:/senitron/printhandler/";
            string logFilePath = this.logPath;
            // check the outout image path
            if (!Directory.Exists(logFilePath))
            {
                Directory.CreateDirectory(logFilePath);
            }

            if (createdLogFileName == null)
            {
                throw new FileNotFoundException("log file not found.", logFilePath + createdLogFileName);

            }
            else
            {
                DateTime now = DateTime.Now;
                string formattedDateTime = now.ToString("yyyy-MM-dd h:mm tt");
                string toWirteContent = $"{formattedDateTime}  {logContent}";


                // create log files
                using (StreamWriter writer = File.AppendText(logFilePath + createdLogFileName + ".log"))
                {
                    //string logMessage = $"{DateTime.Now} - This is a log message.";
                    writer.WriteLine(toWirteContent);
                    writer.Dispose();
                    writer.Close();
                }
                
            }

        }




    }
}
