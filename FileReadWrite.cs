using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TRHDefineUpdater
{
    class FileReadWrite
    {
        public static void Write(string data,string fileName)
        {
            FileInfo fi = new FileInfo(fileName);
            if(!fi.Exists)
            {
                FileStream tempfs = fi.Create();
                tempfs.Close();
                tempfs.Dispose();
                fi = new FileInfo(fileName);
            }
            using (FileStream fs = fi.OpenWrite())
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(data);
                    sw.Flush();
                    sw.Close();
                }
                fs.Close();
            }
        }
        public static string Read(string fileName)
        {
            string data;
            FileInfo fi = new FileInfo(fileName);
            if (!fi.Exists)
                return "";
            using (FileStream fs = fi.OpenRead())
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    data = sr.ReadLine();
                    sr.Close();
                }
                fs.Close();
            }
            return data;
        }
    }
}
