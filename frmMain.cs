using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Xml;

namespace TRHDefineUpdater
{
    public partial class frmMain : Form
    {
        private string filePath,definePath,defineFullPath,defineFilePath,picPath,picFullPath;
        private List<int> newSwords;
        public static frmMain _frmMain;

        public void Failed(string ExMsg)
        {
            Print("失败:" + ExMsg);
            ToExit();
        }
        private string ByteArrayToHexString(byte[] input)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte nowByte in input)
            {
                sb.Append(string.Format("{0:X2}", nowByte));
            }
            return sb.ToString();
        }
        public static void Print(string text)
        {
            if (_frmMain.txtOutput.InvokeRequired)
            {
                _frmMain.txtOutput.Invoke(new EventHandler((sender,e) => {
                    _frmMain.txtOutput.Text += text;
                    _frmMain.txtOutput.SelectionStart = _frmMain.txtOutput.TextLength;
                }));
            }
            else
            {
                _frmMain.txtOutput.Text += text;
                _frmMain.txtOutput.SelectionStart = _frmMain.txtOutput.TextLength;
            }
        }
        
        private void ToExit()
        {
            Print("\r\n按任意键退出...");
            txtOutput.KeyPress += new KeyPressEventHandler((_sender, _e) => { Application.Exit(); });
        }

        private void txtOutput_TextChanged(object sender, EventArgs e)
        {
            txtOutput.SelectionStart = txtOutput.TextLength;
            txtOutput.ScrollToCaret();
        }

        private List<string[]> ParseSwordTable(string dat)
        {
            List<string[]> swordTable = new List<string[]>();
            Regex r = new Regex(@"[\u0000-\uFFFF]+\[sword_master\][\s\r\n]*([\u0000-\uFFFF]+?)[\s\r\n]*\[end\]", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            Match match = r.Match(dat);                                                                //找到对应字段
            string[] _swords = match.Groups[1].Value.Replace("\r", "").Split('\n');                      //读出所有值
            foreach (string _nowSword in _swords)                                                       //遍历
            {
                if (_nowSword != "")                                                                     //非空
                    if (int.Parse((_nowSword.Split(',')[0].ToString())) < 100000)                        //非敌刀
                        swordTable.Add(_nowSword.Split(','));                                            //按数组加值
            }
            return swordTable;
        }
        public frmMain()
        {
            InitializeComponent();
            _frmMain = this;
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            this.Show();
            /*if (!File.Exists(Environment.CurrentDirectory + @"\DecryptCore.swf"))
                Failed("找不到解密工具DecryptCore.swf");
            else
                axShockwaveFlash1.Movie = Environment.CurrentDirectory + @"\DecryptCore.swf";*/
            Print("请选择TRH的位置...\r\n");
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                filePath = folderBrowserDialog1.SelectedPath;
                definePath = @"\devtools\panel\app";
                defineFullPath = filePath + definePath;
                defineFilePath = defineFullPath + @"\define.js";
                picPath = @"\assets\sword";
                picFullPath = filePath + picPath;

                if (File.Exists(defineFilePath))
                {
                    Print("找到define.js...\r\n");
                    Print("正在下载官方数据，请稍后...\r\n");
                    try
                    {
                        FileDownloader fd = new FileDownloader(new Uri("http://static.touken-ranbu.jp/d/1f1fa3d5e6cdc140f3c493457940e761.bin"));
                        fd.FileDownloadFinished += Fd_FileDownloadFinished;
                        fd.FileDownloading += new FileDownloader.FileDownloadingHandler((received, total) => Print(string.Format("Downloading:{0}/{1}\r\n",received,total)));
                        fd.FileDownloadError += Fd_FileDownloadError;
                        fd.Download();
                        /*FileStream fs = new FileStream(@"E:\Download\1f1fa3d5e6cdc140f3c493457940e761_3.bin", FileMode.Open, FileAccess.Read);
                        byte[] b = new byte[fs.Length];
                        fs.Read(b, 0, (int)fs.Length);
                        Fd_FileDownloadFinished(b);*/
                    }
                    catch(Exception ex)
                    {
                        Failed(ex.Message);
                    }
                }
                else
                {
                    Failed("未找到define.js...\r\n请确认文件是否存在:" + defineFilePath + "\r\n");
                }
            }
            else
            {
                Failed("用户取消\r\n");
            }

        }

        private void Fd_FileDownloadError(Exception ex)
        {
            Failed("定义文件下载失败");
        }

        private void Fd_FileDownloadFinished(byte[] data)
        {
            Print("完成\r\n");
            Print("正在解密数据...");
            string hex = ByteArrayToHexString(data);
            axShockwaveFlash1.FlashCall += AxShockwaveFlash1_FlashCall;
            axShockwaveFlash1.CallFunction("<invoke name =\"Decrypt\" returntype=\"xml\"><arguments><string>" + hex + "</string></arguments></invoke>");
        }

        private void AxShockwaveFlash1_FlashCall(object sender, AxShockwaveFlashObjects._IShockwaveFlashEvents_FlashCallEvent e)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(e.request);
            XmlAttributeCollection attr = doc.FirstChild.Attributes;
            string decryptedData = doc.InnerText;
            if (decryptedData != "")
            { 
                try
                {
                    List<string[]> swordTable = ParseSwordTable(decryptedData);
                    Print("成功\r\n");
                    Print("开始分析与插入...\r\n");
                    TRHDefineParser.UpdateDefine(swordTable, out newSwords, defineFullPath,"define.js");
                    if (newSwords != null)
                    {
                        Print("下载刀纹...\r\n");
                        SwordCrestDownload.AllDownloadFinished += SwordCrestDownload_AllDownloadFinished;
                        SwordCrestDownload.DownloadSwordCrest(newSwords, picFullPath);
                    }
                    else
                    {
                        throw (new FormatException("文件已是最新，无需更改"));
                    }
                }
                catch(Exception ex)
                {
                    Failed(ex.Message + "\r\n");
                }
            }
            else
            {
                Failed("解密失败\r\n");
            }
        }

        private void SwordCrestDownload_AllDownloadFinished()
        {
            Print("全部下载已完成\r\n所有操作成功结束!");
            ToExit();
        }

        protected override void WndProc(ref Message msg)
        {
            if (msg.Msg == 0xA3)
            {
                return;
            }
            base.WndProc(ref msg);
        }
    }
}
