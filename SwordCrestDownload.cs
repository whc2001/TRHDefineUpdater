﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TRHDefineUpdater
{
    public class SwordCrestDownload
    {
        public delegate void AllDownloadFinishedHandler();
        public static event AllDownloadFinishedHandler AllDownloadFinished;

        private static List<int> swordTable;
        private static string baseDir;
        private static int errCount;

        private static void Next()
        {
            swordTable.RemoveAt(0);
            if (swordTable.Count != 0)
                Download(swordTable[0]);
            else
                AllDownloadFinished.Invoke();
        }
        private static void Failed(string exMsg)
        {
            if (errCount < 3)
            {
                frmMain.Print("错误:" + exMsg + ",正在重试\r\n");
                errCount++;
                Download(swordTable[0]);
            }
            else
            {
                frmMain.Print("错误:" + exMsg + ",重试次数过多放弃下载\r\n");
                errCount = 0;
                Next();
            }
        }
        private static string ByteArrayToHexString(byte[] input)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in input)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }
        private static string MD5(string input)
        {
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            return ByteArrayToHexString(md5.ComputeHash(Encoding.UTF8.GetBytes(input))).ToLower();
        }

        public static void DownloadSwordCrest(List<int> swordList, string dir)
        {
            swordTable = swordList;
            baseDir = dir;
            Download(swordTable[0]);
        }

        private static void Download(int swordId)
        {
            frmMain.Print("正在下载刀纹#" + swordId.ToString() + "...\r\n");
            string fileName = "swr_crest_s_" + string.Format("{0:000000}", swordId) + "u0xq7npke";
            string fileNameMD5 = MD5(fileName);
            string url = "http://static.touken-ranbu.jp/c/c/" + fileNameMD5 + ".png";
            FileDownloader fd = new FileDownloader(new Uri(url));
            fd.FileDownloadFinished += Fd_FileDownloadFinished;
            fd.FileDownloading += new FileDownloader.FileDownloadingHandler((received, total) => frmMain.Print(string.Format("Downloading:{0}/{1}\r\n", received, total)));
            fd.FileDownloadError += Fd_FileDownloadError;
            fd.DownloadFile(baseDir + "\\" + swordId + ".png");
        }

        private static void Fd_FileDownloadError(Exception ex)
        {
            Failed(ex.Message);
        }

        private static void Fd_FileDownloadFinished(byte[] data)
        {
            frmMain.Print("成功\r\n");
            Next();
        }
    }
}
