using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SenitronPrintHandler
{
    internal class IniFile
    {
        private readonly Dictionary<string, int[]> values = new Dictionary<string, int[]>();

        private readonly Dictionary<string, string> configValves = new Dictionary<string, string>();
        public void InitReadFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("INI file not found.", filePath);

            string[] lines = File.ReadAllLines(filePath);
            string to_replace_line = "";
            foreach (string line in lines)
            {
                string[] parts = line.Split(',');
                if (parts.Length >= 5)  // get the coo info
                {
                    string key = parts[0];
                    int[] coordinates = Array.ConvertAll(parts[1..], int.Parse);
                    values[key] = coordinates;
                }
                else
                {
                    string key = parts[0].Trim();
                    string value = "";
                    if (key == "ApplicationRunningTimes")
                    {
                        //// every time of the applications running, the running times will autoplus 1 in th econfig files

                        int applicationRunningCounter = Convert.ToInt32(parts[1].Trim()) + 1;
                        string applicationRunningTimes = applicationRunningCounter.ToString();
                        to_replace_line = applicationRunningTimes;
                        value = applicationRunningTimes;

                    }
                    else
                    {
                        value = parts[1].Trim();
                    }
                    configValves[key] = value;
                }


            }

            // replace the old ApplicationRunningTimes
            if (to_replace_line != "")
            {
                ReplaceValue(filePath, "ApplicationRunningTimes", to_replace_line);
            }



        }

        public int[] GetIntArray(string cookey)
        {
            if (values.TryGetValue(cookey, out int[] result))
                return result;
            else
                throw new ArgumentException($"Key '{cookey}' not found in INI file.");
        }

        public string GetConfigInfo(string configkey)
        {
            if (configValves.TryGetValue(configkey, out string rst))
                return rst;
            else
                throw new ArgumentException($"Key '{configkey}' not found in INI file.");
        }

        /// <summary>
        /// replace the specified line in the files
        /// </summary>
        /// <param name="strFilePath">the files path</param>
        /// <param name="strIndex"> the lines contains unique string  </param>
        /// <param name="newValue"> the new values  </param>
        public void ReplaceValue(string strFilePath, string strIndex, string newValue)
        {
            if (File.Exists(strFilePath))
            {
                string[] lines = File.ReadAllLines(strFilePath);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains(strIndex))
                    {
                        string[] str = lines[i].Split(",");
                        str[1] = newValue;
                        lines[i] = str[0] + "," + str[1];
                    }
                }
                File.WriteAllLines(strFilePath, lines);

            }
        }




        public bool ResetSetUpFile(IniFile ini, List<string> toSetValueNames)
        {
            bool reset = false;
            try
            {
                for (int i = 0; i < toSetValueNames.Count; i++)
                {

                    ini.ReplaceValue("setup.ini", toSetValueNames[i], $"0,0,0,0");
                }

                reset = true;

            }
            catch
            {
                reset = false;
            }

            return reset;
        }



    }
}
