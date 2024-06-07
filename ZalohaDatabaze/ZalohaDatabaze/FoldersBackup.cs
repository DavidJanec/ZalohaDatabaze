using Serilog;
using System.IO.Compression;

public static class FoldersBackup
{
    public static void BackupFolders(string foldersToBackup, string localBackupPath, string networkBackupPath, string ftpServer, string ftpFolder, string ftpUsername, string ftpPassword)
    {
        if (!string.IsNullOrEmpty(foldersToBackup))
        {
            string[] folders = foldersToBackup.Split(";");
            foreach (string folder in folders)
            {
                if (Directory.Exists(folder))
                {
                    // Název složky
                    string folderName = new DirectoryInfo(folder).Name;

                    string zipFileName;

                    if (!string.IsNullOrEmpty(localBackupPath))
                    {
                        zipFileName = Path.Combine(localBackupPath, $"{folderName}_{DateTime.Now.ToString("dd.MM.yyyy_HHmm")}.zip");
                        // Vytvoření zip souboru
                        ZipFile.CreateFromDirectory(folder, zipFileName);
                        Log.Information($"Složka '{folderName}' byla úspěšně uložena na místní úložiště.");
                    }
                    else
                    {
                        Log.Information("Ukládání na mistní uložiště uložiště přeskočeno.");
                        zipFileName = Path.Combine(localBackupPath, $"{folderName}_{DateTime.Now.ToString("dd.MM.yyyy_HHmm")}.zip");
                        ZipFile.CreateFromDirectory(folder, zipFileName);
                    }
                    // Kopírování zip souboru na síťové úložiště
                    if (!string.IsNullOrEmpty(networkBackupPath))
                    {
                        string networkZipFileName = Path.Combine(networkBackupPath, Path.GetFileName(zipFileName));
                        File.Copy(zipFileName, networkZipFileName, true);
                        Log.Information($"Složka '{folderName}' byla úspěšně uložena na síťové úložiště.");
                    }
                    else
                    {
                        Log.Information("Ukládání na síťové uložiště uložiště přeskočeno.");
                    }


                    // Nahrání zip souboru na FTP server
                    if (!string.IsNullOrEmpty(ftpServer))
                    {
                        FtpService.UploadFileToFtp(zipFileName, ftpServer, ftpFolder, ftpUsername, ftpPassword);
                        Log.Information($"Složka '{folderName}' byla úspěšně uložena na FTP server.");
                    }
                    else
                    {
                        Log.Information("Ukládání na ftp server přeskočeno.");
                    }


                    Log.Information($"Složka '{folderName}' byla úspěšně zálohována!\n");
                }
                else
                {
                    Log.Warning($"Složka '{folder}' neexistuje!");

                }
            }
        }
        else
        {
            Log.Warning("Složky k zálohování nezadány!");
            return;
        }
    }
}