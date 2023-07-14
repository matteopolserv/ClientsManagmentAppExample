using ClientsManagmentAppExample.Interfaces;
using MimeKit;
using MimeKit.Text;
using MailKit.Security;
using MailKit.Net.Smtp;
using ClientsManagmentAppExample.Models;
using System.Text;
using System.Security.Cryptography;


namespace ClientsManagmentAppExample.Helpers
{
    public class Helpers : IHelpers
    {
        public async Task SendSMTPEmailAsync(string receiver, string subject, string body)
        {
            try
            {
                await Task.Run(() =>
                {
                    var email = new MimeMessage();
                    email.From.Add(new MailboxAddress("Wiadomość z systemu CRM ", "kontakt@example.pl"));
                    email.To.Add(MailboxAddress.Parse(receiver));
                    email.ReplyTo.Add(MailboxAddress.Parse("kontakt@example.pl"));
                    email.Subject = subject;
                    email.Body = new TextPart(TextFormat.Html) { Text = body };

                    // send email

                    using var smtp = new SmtpClient();
                    smtp.Connect("smtp.gmail.com", 465, SecureSocketOptions.SslOnConnect);
                    smtp.Authenticate("kontakt@example.pl", "passwordFromExternalStorage");
                    smtp.Send(email);
                    smtp.Disconnect(true);
                    
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);


            }
        }
        public async Task<string> FormMessageBuilder(FormModel model)
        {
            StringBuilder message = new();
            if (model.Service == "SiteWeb")
            {
                await Task.Run(() =>
                {

                message.Append("<p>Usługa: Budowa strony internetowej</p><p>Imię i nazwisko osoby do kontaktu lub nazwa firmy: " + model.ClientName + "</p><p>Adres Email: " + model.ClientEmail + "</p><p>Telefon: " + model.ClientPhone + "</p>");
                if (model.HasWebSite == true) { message.Append("<p>Adres obecnej strony: " + model.WebSiteAddress + "</p>"); }
                if (model.HasDomain == true) { message.Append("<p>Nazwa i operator domeny: " + model.DomainName + "</p>"); }
                if (model.HasHosting == true) { message.Append("<p>Dostawca i pakiet hostingowy: " + model.HostingName + "</p>"); }
                message.Append("<p>Projekt graficzny: " + model.VisualProjectDesc + "</p><p>Charakterystyka strony: ");
                if (model.WebSiteProfile == "visitpage") { message.Append("<span>Strona wizytówka - modyfikacje są przewidywane rzadko lub wcale</span>"); }
                if (model.WebSiteProfile == "cms") { message.Append("<span> Portal, na którym regularnie będą będą zamieszczane nowe informacje: newsy, ogłoszenia, artykuły</span>"); };
                if (model.WebSiteProfile == "webapp") { message.Append("<span>Strona z dodatkowymi funkcjami</span>"); }
                message.Append("</p><p>Termin realizacji: " + model.Deadline + "<p>Opis: " + model.CMSDesc + "</p><p>Uwagi: " + model.Notices + "</p>");

                });
            }
            if (model.Service == "Shop")
            {
                await Task.Run(() =>
                {

                    message.Append("<p>Usługa: Budowa sklepu internetowego</p><p>Imię i nazwisko osoby do kontaktu lub nazwa firmy: " + model.ClientName + "</p><p>Adres Email: " + model.ClientEmail + "</p><p>Telefon: " + model.ClientPhone + "</p>");
                    if (model.HasWebSite == true) { message.Append("<p>Adres obecnego sklepu: " + model.WebSiteAddress + "</p>"); }
                    if (model.HasDomain == true) { message.Append("<p>Nazwa i operator domeny: " + model.DomainName + "</p>"); }
                    if (model.HasHosting == true) { message.Append("<p>Dostawca i pakiet hostingowy: " + model.HostingName + "</p>"); }
                    message.Append("<p>Projekt graficzny: " + model.VisualProjectDesc);
                   
                    message.Append("</p><p>Termin realizacji: " + model.Deadline + "<p>Opis sklepu: " + model.ShopDesc + "</p><p>Uwagi: " + model.Notices + "</p>");

                });
            } 
            if (model.Service == "WebApp")
                {
                    await Task.Run(() =>
                    {

                        message.Append("<p>Usługa: Budowa aplikacji internetowej</p><p>Imię i nazwisko osoby do kontaktu lub nazwa firmy: " + model.ClientName + "</p><p>Adres Email: " + model.ClientEmail + "</p><p>Telefon: " + model.ClientPhone + "</p>");
                        
                        if (model.HasDomain == true) { message.Append("<p>Nazwa i operator domeny: " + model.DomainName + "</p>"); }
                        if (model.HasHosting == true) { message.Append("<p>Dostawca i pakiet hostingowy: " + model.HostingName + "</p>"); }
                        message.Append("<p>Projekt graficzny: " + model.VisualProjectDesc);
                   
                        message.Append("</p><p>Termin realizacji: " + model.Deadline + "<p>Opis aplikacji: " + model.WebAppDesc + "</p><p>Uwagi: " + model.Notices + "</p>");

                    });
            } 
            if (model.Service == "DeskApp")
                {
                    await Task.Run(() =>
                    {

                        message.Append("<p>Usługa: Budowa aplikacji internetowej</p><p>Imię i nazwisko osoby do kontaktu lub nazwa firmy: " + model.ClientName + "</p><p>Adres Email: " + model.ClientEmail + "</p><p>Telefon: " + model.ClientPhone + "</p>");
                        
                        message.Append( "</p><p>System operacyjny: " + model.OperatingSystem + "</p><p>Język: " + model.Language + "</p><p>Aplikacja konsolowa czy okienkowa: " + model.DeskForm + "</p><p>Projekt graficzny w przypadku aplikacji okienkowej: " + model.VisualProjectDesc);
                   
                        message.Append("</p><p>Termin realizacji: " + model.Deadline + "</p><p>Opis aplikacji: " + model.DeskAppDesc + "</p><p>Uwagi: " + model.Notices + "</p>");

                    });
            }  
            if (model.Service == "MobileApp")
                {
                    await Task.Run(() =>
                    {

                        message.Append("<p>Usługa: Budowa aplikacji internetowej</p><p>Imię i nazwisko osoby do kontaktu lub nazwa firmy: " + model.ClientName + "</p><p>Adres Email: " + model.ClientEmail + "</p><p>Telefon: " + model.ClientPhone + "</p>");
                        
                        message.Append( "</p><p>System operacyjny: " + model.OperatingSystem + "</p><p>Język: " + model.Language + "</p><p>Projekt graficzny w przypadku aplikacji okienkowej: " + model.VisualProjectDesc);
                   
                        message.Append("</p><p>Termin realizacji: " + model.Deadline + "</p><p>Opis aplikacji: " + model.DeskAppDesc + "</p><p>Uwagi: " + model.Notices + "</p>");

                    });
            }
            return message.ToString();
        }

       

        public async Task<string> EncryptString(string text)
        {
            string result = "";
            await Task.Run(() =>
            {
                var keyBytes = new byte[16];
                var skeyBytes = Encoding.UTF8.GetBytes("VeryStrongPasswordFromExternalSource");
                Array.Copy(skeyBytes, keyBytes, Math.Min(keyBytes.Length, skeyBytes.Length));

                Aes aes = Aes.Create();
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.KeySize = 128;
                aes.Key = keyBytes;
                aes.IV = keyBytes;
                var b = Encoding.UTF8.GetBytes(text);
                var encrypted = aes.CreateEncryptor().TransformFinalBlock(b, 0, b.Length);
                result = Convert.ToBase64String(encrypted);
            });
            return result;
        }
    

        public async Task<string> DecryptString(string encryptedText)
        {
            string result = "";
            
            await Task.Run(() =>
            {
                var keyBytes = new byte[16];
                var skeyBytes = Encoding.UTF8.GetBytes("VeryStrongPasswordFromExternalSource");
                Array.Copy(skeyBytes, keyBytes, Math.Min(keyBytes.Length, skeyBytes.Length));

                Aes aes = Aes.Create();
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.KeySize = 128;
                aes.Key = keyBytes;
                aes.IV = keyBytes;
                var b = Convert.FromBase64String(encryptedText);
                var decrypted = aes.CreateDecryptor().TransformFinalBlock(b, 0, b.Length);
                result = Encoding.UTF8.GetString(decrypted);
            });
            return result;
        }

        public async Task<bool> CheckPassword(string text, string name)
        {
            
            bool contains = false;
            await Task.Run(async () =>
            {
                text = text.ToLower();
                List<string> words = new()
            {
                "hasło",
                "hasła",
                "hasłu",
                "hasłem",
                "haśle",
                "hasełko",
                "hasłeko",
                "hasleko",
                "haslo",
                "hasla",
                "haslu",
                "haslem",
                "hasle",
                "haselko",
                "halso",
                "halsa",
                "halsu",
                "halsem",
                "halse",
                "hasleko",
                "hałso",
                "hałsa",
                "hałsu",
                "hałsem",
                "halśe",
                "password",
                "pass",
                "pwd",
                "pasword",
                                
            };
                if (words.Any(s => text.Contains(s, StringComparison.CurrentCultureIgnoreCase))) 
                    {
                        contains = false;
                        
                        
                        //await SendSMTPEmailAsync("matteo@lotier.pl", "Użycie hasła w aktualizacjach", "Tekst wpisany do aktualizacji: " + text + " Nastąpiło naruszenie polityki prywatności przez: " + name);
                    }
                

            });
            return contains;
        }
    }
}
