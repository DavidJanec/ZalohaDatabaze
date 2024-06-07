using System.Net.Mail;
using System.Net;

public static class EmailService
{
    public static void SendEmailWithLog(string fromAddress, string fromPassword, string toAddress, string subject, string logPath)
    {
        using (MailMessage mail = new MailMessage(fromAddress, toAddress))
        {
            // Nastavení e-mailového předmětu
            mail.Subject = subject;
            string body = "";
            string logContent = "";

            // Načtení obsahu logu
            if (File.Exists(logPath))
            {
                logContent = File.ReadAllText(logPath);
            }
            else
            {
                logContent = "Logovací soubor nebyl nalezen.";
            }

            // Přidání logu do těla e-mailu
            if (logContent.Contains("ERR"))
            {
                body = "Zálohování databází nebylo úspěšné! Logovací soubor je přiložen s podrobnostmi.\n\n";
            }
            else
            {
                body = "Zálohování databází bylo úspěšné! Logovací soubor je přiložen.\n\n";
            }

            body += logContent;

            mail.Body = body;

            // Přiložení logu jako přílohy
            if (File.Exists(logPath))   
            {
                // Vytvoření přílohy z logovacího souboru
                Attachment attachment = new Attachment(logPath);

                // Přidání přílohy k e-mailu
                mail.Attachments.Add(attachment);
            }

            Console.WriteLine();
            Console.WriteLine("Probíhá přeposílání na e-mail...");

            // Nastavení SmtpClienta pro odesílání e-mailu přes Gmailový server
            using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587))
            {
                // Povolení SSL zabezpečení
                smtpClient.EnableSsl = true;
                smtpClient.Credentials = new NetworkCredential(fromAddress, fromPassword);

                // Poslání e-mailu 
                smtpClient.Send(mail);
                Console.WriteLine("Úspěšně přeposláno na e-mail!");
            }
        }
    }
}