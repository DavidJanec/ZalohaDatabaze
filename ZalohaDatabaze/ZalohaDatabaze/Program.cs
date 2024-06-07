using Microsoft.Extensions.Configuration;
using Serilog;
class Program
{
    static void Main()
    {
        // Načtení konfigurace z appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Nastavení parametrů
        
        // Databáze
        string? connectionString = configuration["DatabaseSettings:ConnectionString"];
        // Uložiště
        string? localBackupPath = configuration["BackupPaths:LocalBackupPath"];
        string? networkBackupPath = configuration["BackupPaths:NetworkBackupPath"];

        // FTP
        string? ftpServer = configuration["FtpSettings:FtpServer"];
        string? ftpFolder = configuration["FtpSettings:FtpFolder"];
        string? ftpUsername = configuration["FtpSettings:FtpUsername"];
        string? ftpPassword = configuration["FtpSettings:FtpPassword"];

        // Složky k zálohování
        string? foldersToBackup = configuration["FolderSettings:FoldersToBackup"];

        // E-mail
        string? fromAdress = configuration["EmailSettings:FromAddress"];
        string? fromPassword = configuration["EmailSettings:FromPassword"];
        string? toAdress = configuration["EmailSettings:ToAddress"];
        string? Subject = configuration["EmailSettings:Subject"] + DateTime.Now.ToString("dd.MM.yyyy_HHmm");


        // Nastavení loggovaní do složky logs/logs.txt
        string LogPath = $"../../../logs/log_{DateTime.Now.ToString("dd.MM.yyyy_HHmm")}.txt";
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(LogPath) // Cesta k souboru na logovani 
            .WriteTo.Console() // Jak do console tak i do logu
            .CreateLogger();

        try
        {
            Log.Information("Parametry databáze, uložišť a FTP serveru nastaveny!");
            
            // Zálohování databází
            if (!string.IsNullOrWhiteSpace(connectionString)) {
                Log.Information("Probíhá zálohování databází...\n");
                DatabaseBackup.BackupDatabases(connectionString, localBackupPath, networkBackupPath, ftpServer, ftpFolder, ftpUsername, ftpPassword);
            } else
            {
                Log.Information("Zálohování databází přeskočeno (nevyplněné parametry).");
            }

            // Zálohování složek
            if (!string.IsNullOrWhiteSpace(localBackupPath) || !string.IsNullOrWhiteSpace(networkBackupPath))
            {
                Log.Information("Probíhá zálohování složek...\n");
                FoldersBackup.BackupFolders(foldersToBackup, localBackupPath, networkBackupPath, ftpServer, ftpFolder, ftpUsername, ftpPassword);
            } else
            {
                Log.Information("Zálohování složek přeskočeno (nevyplněné parametry).");
            }
        }
        catch (Exception ex)
        {
            // Logování chyby pomocí Serilogu
            Log.Error(ex, "Záloha se nezdařila. Chybová hláška: ");
            Log.CloseAndFlush();

            if (!string.IsNullOrWhiteSpace(fromAdress) && !string.IsNullOrWhiteSpace(fromPassword) && !string.IsNullOrWhiteSpace(toAdress))
            {
                EmailService.SendEmailWithLog(fromAdress, fromPassword, toAdress, Subject, LogPath);
            } else
            {
                Console.WriteLine("Přeposílání na e-mail přeskočeno (nevyplněné parametry).");
            }
            Environment.Exit(1);
        }
        finally
        {
            Log.CloseAndFlush();

            if (!string.IsNullOrWhiteSpace(fromAdress) && !string.IsNullOrWhiteSpace(fromPassword) && !string.IsNullOrWhiteSpace(toAdress))
            {
                EmailService.SendEmailWithLog(fromAdress, fromPassword, toAdress, Subject, LogPath);
            }
            else
            {
                Console.WriteLine("Přeposílání na e-mail přeskočeno (nevyplněné parametry).");
            }
        }
    }
}
