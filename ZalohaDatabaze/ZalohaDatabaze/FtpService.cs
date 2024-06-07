using System.Net;

public static class FtpService
{
    public static void UploadFileToFtp(string fileName, string ftpServer, string ftpFolder, string ftpUsername, string ftpPassword)
    {
        // Sestavení cesty na FTP Server
        string ftpFilePath = $"{ftpServer}/{ftpFolder}/{Path.GetFileName(fileName)}";
        // Vytvoření relace, který bude použita k nahrávání souboru na FTP Server
        using (WebClient client = new WebClient())
        {
            // Nastavení přihlašovacích údajů
            client.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
            // Nahrání souboru na FTP Server
            client.UploadFile(ftpFilePath, WebRequestMethods.Ftp.UploadFile, fileName);
        }
    }
}