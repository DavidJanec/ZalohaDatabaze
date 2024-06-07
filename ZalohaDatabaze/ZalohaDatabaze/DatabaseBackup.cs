using Serilog;
using System.Data.SqlClient;
using System.IO.Compression;
public static class DatabaseBackup
{
    public static void BackupDatabases(string connectionString, string localBackupPath, string networkBackupPath, string ftpServer, string ftpFolder, string ftpUsername, string ftpPassword)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            // Otevření připojení k databázi
            connection.Open();

            // Získání seznamu všech databází kromě systémových databází pomocí dotazu
            SqlCommand command = new SqlCommand("SELECT TOP 10 name FROM sys.databases WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb');", connection);
            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                // Jména databází, které zálohujeme
                string databaseName = reader["name"].ToString();
                // Informování o aktuálně zálohované databázi
                Console.WriteLine();
                Log.Information($"Databáze '{databaseName}' nalezena. Probíhá záloha...");

                // Nastavení cesty pro zálohu této konkrétní databáze
                string backupFileName;

                if (!string.IsNullOrWhiteSpace(localBackupPath))
                {
                    backupFileName = Path.Combine(localBackupPath, $"{databaseName}_{DateTime.Now.ToString("dd.MM.yyyy_HHmm")}.bak");
                }
                else if (!string.IsNullOrEmpty(networkBackupPath))
                {                     
                    backupFileName = Path.Combine(networkBackupPath, $"{databaseName}_{DateTime.Now.ToString("dd.MM.yyyy_HHmm")}.bak");
                }
                else
                {
                    Log.Warning("Nebylo určeno ani místní ani sítově uložiště! Ukončuji zálohování databází!");
                    return;
                }

                // Zálohování databáze
                using (SqlConnection backupConnection = new SqlConnection(connectionString))
                {
                    // Otevření připojení k databázi
                    backupConnection.Open();

                    // Dotaz pro zálohovaní
                    string backupQuery = $"BACKUP DATABASE [{databaseName}] TO DISK = N'{backupFileName}' WITH NOFORMAT, NOINIT, NAME = N'{databaseName}-Full Database Backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10";

                    // Provádění tohoto dotazu
                    using (SqlCommand backupCommand = new SqlCommand(backupQuery, backupConnection))
                    {
                        backupCommand.CommandTimeout = 90000;
                        backupCommand.ExecuteNonQuery();
                        Log.Information("Databáze úspěšně uložena na místní úložiště!");
                    }
                }
                if (!string.IsNullOrEmpty(backupFileName))
                {
                    // Cesta k zip souboru
                    string zipFileName = Path.Combine(Path.GetDirectoryName(backupFileName), $"{databaseName}_{DateTime.Now.ToString("dd.MM.yyyy_HHmm")}.zip");

                    // Vytvoření zip souboru obsahujícího jeden .bak soubor
                    using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Create))
                    {
                        zip.CreateEntryFromFile(backupFileName, Path.GetFileName(backupFileName));
                    }

                    // Smazání prozatimních souboru .bak
                    File.Delete(backupFileName);

                    if (!string.IsNullOrEmpty(networkBackupPath))
                    {
                        // Kopírování zip souboru na síťové úložiště
                        string networkZipFileName = Path.Combine(networkBackupPath, Path.GetFileName(zipFileName));
                        File.Copy(zipFileName, networkZipFileName, true);
                        Log.Information("Databáze úspěšně uložena na síťové úložiště!");
                    }
                    else
                    {
                        Log.Information("Ukládání na síťové uložiště uložiště přeskočeno.");
                    }

                    if (!string.IsNullOrEmpty(ftpServer))
                    {
                        // Nahrání zip souboru na FTP server
                        FtpService.UploadFileToFtp(zipFileName, ftpServer, ftpFolder, ftpUsername, ftpPassword);
                        Log.Information("Databáze úspěšně uložena na FTP server!");
                    }
                    else
                    {
                        Log.Information("Ukládání na ftp server přeskočeno.");
                    }
                    Log.Information($"Databáze '{databaseName}' byla úspěšně zálohována!");
                }
            }
        }
    }
}
