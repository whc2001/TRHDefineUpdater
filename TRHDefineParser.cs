#define NOT_DEBUG
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TRHDefineUpdater
{
    public class TRHDefineParser
    {
        private static Dictionary<string, SwordModel> dic = new Dictionary<string, SwordModel>();           //Sword list.
        private static Dictionary<string, SwordModel> orderedDic = new Dictionary<string, SwordModel>();    //Sorted sword list.

        public class SwordModel                                                                            //Json model(used to deserialize json).								
        {
            public string name;
            public int id;
            public int rarity;
            public int type;
            public int group;
            public int equip;
            public int area;
            public int upgrade;
        }


        private static int Find(string key, string source, int startIndex = 0)                              //Find string and return the index of last char.
        {
            int index = source.IndexOf(key, startIndex);
            return index + key.Length;
        }

        public static void UpdateDefine(List<string[]> swordDataList, out List<int> newSwordList, string filePath, string fileName)
        {
            bool same = true;
            string textData, textDataSource;
            List<int> newSwords = new List<int>();
            FileInfo fi;
            /***打开文件***/
            fi = new FileInfo(filePath + "\\" + fileName);

            /***读数据***/
            using (FileStream fs = fi.OpenRead())
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    textData = sr.ReadToEnd();
                    sr.Close();
                }
                fs.Close();
            }
            textDataSource = textData;

            /***找指定的区域***/
            int head = Find("exports.tohken.define.tohkens = ", textData);                                  //Start index.
            int tail = Find("};", textData, head) - 1;                                                      //End index.
            string data2 = textData.Substring(head, tail - head).Trim();                                    //Js between the two indexs.These text can be parsed as a json.
            try
            {
                /***分析js，可能导致异常***/
                dic = JsonConvert.DeserializeObject<Dictionary<string, SwordModel>>(data2);                     //Parse the text and deserialize each item into the list.
            }
            catch
            {
                throw (new FormatException("无法分析源文件"));
            }

            /***找新刀***/
            foreach (string[] nowSword in swordDataList)
            {
                if (!dic.ContainsKey(nowSword[0]))
                {
                    same = false;
                    frmMain.Print(string.Format("◆找到新刀:[{0}]{1}，请确认数据...", nowSword[0], nowSword[2]));
                    frmNewSwordData newSwordDialog = new frmNewSwordData(nowSword);
                    if (newSwordDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        dic.Add(newSwordDialog.Sword.id.ToString(), newSwordDialog.Sword);
                        newSwords.Add(newSwordDialog.Sword.id);
                        frmMain.Print(string.Format("\"{0}\"添加成功\r\n", newSwordDialog.Sword.name));
                    }
                }
            }

            if (same)
            {
                newSwordList = null;
                return;
            }

            /***排序***/
            foreach (KeyValuePair<string, SwordModel> item in dic.OrderBy(e => int.Parse(e.Key)))               //Sort.
            {
                orderedDic.Add(item.Key, item.Value);
            }

            /***替换原来的数据***/
            string newJson = JObject.Parse(JsonConvert.SerializeObject(orderedDic)).ToString();             //Convert to JObject then ToString in order to format json.
            textData = textData.Remove(head, tail - head);                                                  //Remove old js.
            textData = textData.Insert(head, newJson);                                                      //Insert new js.
            textData = textData.Replace("\r", "");

            frmMain.Print("新刀数据准备完毕\r\n");

#if NOT_DEBUG
            /***写备份，可能引发异常***/
            if (!File.Exists(Environment.CurrentDirectory + "\\nobackup.txt"))
            {
                frmMain.Print("创建备份...");
                try
                {
                    FileInfo fi2 = new FileInfo(filePath + "\\before.bak");
                    if (!fi2.Exists)
                    {
                        FileStream tempFs = File.Create(filePath + "\\before.bak");
                        tempFs.Close();
                        fi2 = new FileInfo(filePath + "\\before.bak");
                    }
                    using (FileStream fs2 = fi2.OpenWrite())
                    {
                        using (StreamWriter sw2 = new StreamWriter(fs2))
                        {
                            sw2.Write(textDataSource);
                            sw2.Close();
                        }
                        fs2.Close();
                    }
                    frmMain.Print("成功\r\n");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Print(ex.Message);
                    throw (new IOException("无法创建备份文件，如果要禁用备份请在本程序根目录下创建「nobackup.txt」\r\n"));
                }
            }
            else
            {
                frmMain.Print("已禁用备份创建\r\n");
            }

            /***写原文件，可能引发异常***/
            frmMain.Print("向文件写入数据...");
            try
            {
                using (FileStream fs = fi.OpenWrite())
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.BaseStream.SetLength(0);
                        sw.Write(textData);
                        sw.Close();
                    }
                    fs.Close();
                }
                frmMain.Print("成功\r\n");
            }
            catch
            {
                throw (new IOException("无法写入原文件，请检查是否被占用（如TRH正在运行、文件只读等）\r\n"));
            }
#else
                frmMain.Print("SKIPED");
#endif
            newSwordList = newSwords;
        }
    }
}
