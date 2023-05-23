using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Pop3;
using MailKit.Security;
using MimeKit;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

// Definirea informațiilor de conectare pentru serverele POP3 și IMAP
string popServer = "pop.gmail.com";
int popPort = 995;
string imapServer = "imap.gmail.com";
int imapPort = 993;
string username = "rzvn332@gmail.com";
string password = "temcdxciznargrox";

bool exit = false;
while (!exit)
{
    Console.WriteLine("\nMenu:");
    Console.WriteLine("1. Listează e-mailurile din cutia poștală folosind protocolul POP3");
    Console.WriteLine("2. Lista de e-mailuri din cutia poștală utilizând protocolul IMAP");
    Console.WriteLine("3. Descărcați un e-mail cu atașamente");
    Console.WriteLine("4. Trimiteți un e-mail numai cu text");
    Console.WriteLine("5. Trimiteți un e-mail cu atașamente");
    Console.WriteLine("6. Includeți detaliile subiectului și răspunsului atunci când trimiteți un e-mail");
    Console.WriteLine("0. Ieșire");
    Console.Write("\nIntroduceți opțiunea dorită: ");
    var option = Console.ReadLine();

    switch (option)
    {
        case "1":
            ListEmailsPop3();
            break;

        case "2":
            ListEmailsImap();
            break;

        case "3":
            DownloadEmail();
            break;

        case "4":
            SendEmail(SendTextEmail);
            break;

        case "5":
            SendEmail(SendEmailWithAttachments);
            break;

        case "6":
            SendEmail(SendEmailWithDetails);
            break;

        case "0":
            exit = true;
            break;

        default:
            Console.WriteLine("Opțiune invalidă. Vă rugăm să încercați din nou.");
            break;
    }
}

// Funcție pentru listarea e-mailurilor folosind protocolul POP3
void ListEmailsPop3()
{
    using var tcpClient = new TcpClient(popServer, popPort);
    using var sslStream = new SslStream(tcpClient.GetStream());
    sslStream.AuthenticateAsClient(popServer);

    using var reader = new StreamReader(sslStream, Encoding.ASCII);
    using var writer = new StreamWriter(sslStream, Encoding.ASCII);
    string response = reader.ReadLine() ?? "";

    writer.WriteLine("USER " + username);
    writer.Flush();
    response = reader.ReadLine() ?? "";

    writer.WriteLine("PASS " + password);
    writer.Flush();
    response = reader.ReadLine() ?? "";

    writer.WriteLine("LIST");
    writer.Flush();
    response = reader.ReadLine() ?? "";

    while ((response = reader.ReadLine() ?? "") != "." && response != null)
    {
        Console.WriteLine(response);
    }

    writer.WriteLine("QUIT");
    writer.Flush();
    response = reader.ReadLine() ?? "";

    Console.WriteLine("Lista de e-mailuri POP3 a fost recuperată cu succes.");
}

// Funcție pentru listarea e-mailurilor folosind protocolul IMAP
void ListEmailsImap()
{
    using var client = new ImapClient();
    client.Connect(imapServer, imapPort, SecureSocketOptions.SslOnConnect);
    client.Authenticate(username, password);

    var inbox = client.Inbox;
    inbox.Open(FolderAccess.ReadOnly);

    Console.WriteLine("Total mesaje: {0}", inbox.Count);

    for (int i = 0; i < inbox.Count; i++)
    {
        var message = inbox.GetMessage(i);
        Console.WriteLine("Subiect: {0}", message.Subject);
        Console.WriteLine("De la: {0}", message.From);
        Console.WriteLine();
    }

    Console.WriteLine("Lista de e-mailuri IMAP a fost recuperată cu succes.");
}

// Funcție pentru descărcarea unui e-mail cu atașamente
void DownloadEmail()
{
    string downloadPath = "\\Users\\rzvn332\\Desktop\\";

    using (var pop3Client = new Pop3Client())
    {
        pop3Client.Connect(popServer, popPort, true);
        pop3Client.Authenticate(username, password);

        for (int i = 0; i < pop3Client.Count; i++)
        {
            var message = pop3Client.GetMessage(i);

            foreach (var attachment in message.Attachments)
            {
                var fileName = attachment.ContentDisposition?.FileName ?? attachment.ContentType.Name;

                using (var stream = File.Create(downloadPath + fileName))
                {
                    if (attachment is MessagePart part)
                        part.Message.WriteTo(stream);
                    else if (attachment is MimePart mimePart)
                        mimePart.Content.DecodeTo(stream);
                }

                Console.WriteLine("Fișier descărcat: " + fileName);
            }
        }
    }
}

// Funcție pentru trimiterea unui e-mail
void SendEmail(Action getEmailData)
{
    getEmailData.Invoke();

    var smtpClient = new SmtpClient("smtp.gmail.com", 587);
    smtpClient.EnableSsl = true;
    smtpClient.UseDefaultCredentials = false;
    smtpClient.Credentials = new NetworkCredential(username, password);

    try
    {
        smtpClient.Send(GetEmailMessage());
        Console.WriteLine("Email trimis cu succes!");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Eroare la trimiterea e-mailului: " + ex.Message);
    }
}

// Funcție pentru obținerea informațiilor unui e-mail
MailMessage GetEmailMessage()
{
    Console.Write("Introduceți adresa de e-mail a destinatarului: ");
    string toEmail = GetValidEmail();

    Console.Write("Introduceți subiectul e-mailului: ");
    string subject = Console.ReadLine() ?? "";

    Console.Write("Introduceți corpul e-mailului: ");
    string body = Console.ReadLine() ?? "";

    return new MailMessage(username, toEmail, subject, body);
}

// Funcție pentru obținerea unei adrese de e-mail valide
string GetValidEmail()
{
    string email;
    do
    {
        Console.Write("Introduceți adresa de e-mail: ");
        email = Console.ReadLine() ?? "";

        if (!Regex.IsMatch(email, @"^[^@\s]+@([a-zA-Z0-9]+\.)+[a-zA-Z]{2,}$"))
        {
            Console.WriteLine("Adresa de e-mail invalidă. Vă rugăm să introduceți o adresă de e-mail validă.");
            email = "";
        }
    } while (email == null);

    return email;
}

// Funcție pentru trimiterea unui e-mail doar cu text
void SendTextEmail()
{
    var message = GetEmailMessage();

    var smtpClient = new SmtpClient("smtp.gmail.com", 587);
    smtpClient.EnableSsl = true;
    smtpClient.UseDefaultCredentials = false;
    smtpClient.Credentials = new NetworkCredential(username, password);

    try
    {
        smtpClient.Send(message);
        Console.WriteLine("Email trimis cu succes!");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Eroare la trimiterea e-mailului: " + ex.Message);
    }
}

// Funcție pentru trimiterea unui e-mail cu atașamente
void SendEmailWithAttachments()
{
    var message = GetEmailMessage();

    Console.Write("Introduceți calea către fișierul atașat: ");
    string attachmentPath = Console.ReadLine() ?? "";

    var attachment = new Attachment(attachmentPath);
    message.Attachments.Add(attachment);

    var smtpClient = new SmtpClient("smtp.gmail.com", 587);
    smtpClient.EnableSsl = true;
    smtpClient.UseDefaultCredentials = false;
    smtpClient.Credentials = new NetworkCredential(username, password);

    try
    {
        smtpClient.Send(message);
        Console.WriteLine("Email trimis cu succes!");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Eroare la trimiterea e-mailului: " + ex.Message);
    }
}

// Funcție pentru trimiterea unui e-mail cu detalii despre subiect și răspuns
void SendEmailWithDetails()
{
    var message = GetEmailMessage();

    Console.WriteLine("Introduceți adresa de e-mail pentru răspuns:");
    string replyTo = GetValidEmail();

    message.ReplyToList.Add(replyTo);

    var smtpClient = new SmtpClient("smtp.gmail.com", 587);
    smtpClient.EnableSsl = true;
    smtpClient.UseDefaultCredentials = false;
    smtpClient.Credentials = new NetworkCredential(username, password);

    try
    {
        smtpClient.Send(message);
        Console.WriteLine("Email trimis cu succes!");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Eroare la trimiterea e-mailului: " + ex.Message);
    }
}
