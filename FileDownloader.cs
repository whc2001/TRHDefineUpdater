using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace TRHDefineUpdater
{
    class FileDownloader
    {
        private WebClient client = new WebClient();
        public delegate void FileDownloadFinishedHandler(byte[] data);
        public event FileDownloadFinishedHandler FileDownloadFinished;
        public delegate void FileDownloadErrorHandler(Exception ex);
        public event FileDownloadErrorHandler FileDownloadError;
        public delegate void FileDownloadingHandler(long receivedBytes,long totalBytes);
        public event FileDownloadingHandler FileDownloading;
        public Uri URL { get; set; }

        public void DownloadFile(string fileFullPath)
        {
            client.DownloadProgressChanged += Client_DownloadProgressChanged;
            client.DownloadFileCompleted += Client_DownloadFileCompleted;
            client.DownloadFileAsync(URL, fileFullPath);
        }

        private void Client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
                FileDownloadFinished?.Invoke(null);
            else
                FileDownloadError?.Invoke(e.Error);
        }

        public void Download()
        {
            client.DownloadDataCompleted += Client_DownloadDataCompleted;
            client.DownloadProgressChanged += Client_DownloadProgressChanged;
            client.DownloadDataAsync(URL);
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            FileDownloading?.Invoke(e.BytesReceived, e.TotalBytesToReceive);
        }

        private void Client_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            if (e.Error == null)
                FileDownloadFinished?.Invoke(e.Result);
            else
                FileDownloadError?.Invoke(e.Error);
        }
        public FileDownloader(Uri url)
        {
            URL = url;
        }
    }
}
