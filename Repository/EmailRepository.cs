using DuploAuth.Models.Entities;
using System;
using System.Net;
using System.Net.Mail;

namespace DuploAuth.Repository
{
    public class EmailRepository
    {
        private static string fromAddress;
        private static string fromName;
        private static string smtpServer;
        private static bool smtpSSLEnabled;
        private static int smtpPort = 587;
        private static string password;
        private static ApplicationDbContext _applicationDbContext;

        public EmailRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public static void InitializeVariablesAsync()
        {
            fromAddress = _applicationDbContext.Variables.FindAsync("fromAddress").Result.Value;
            fromName = _applicationDbContext.Variables.FindAsync("from_name").Result.Value;
            smtpServer = _applicationDbContext.Variables.FindAsync("smtp_server").Result.Value;
            smtpSSLEnabled = _applicationDbContext.Variables.FindAsync("smtp_ssl").Result.Value == "true";
            password = _applicationDbContext.Variables.FindAsync("smtp_password").Result.Value;

            if (string.IsNullOrEmpty(fromAddress))
            {
                throw new ArgumentNullException("From Address cannot be null");
            }
            if (string.IsNullOrEmpty(smtpServer))
            {
                throw new ArgumentNullException("Server cannot be null");
            }
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException("Password cannot be null");
            }
            try
            {
                smtpPort = int.Parse(_applicationDbContext.Variables.FindAsync("smtp_port").Result.Value);
            }
            catch (FormatException)
            {
                smtpPort = 587;
            }
        }

        public string[] EmailProperties()
        {
            return new string[] { "fromAddress", "smtp_port", "smtp_server", "smtp_ssl", "send_email_invite", "invite_subject", "invite_message", "smtp_password", "from_name" };
        }

        public static void EmailSender(ApplicationDbContext appDbContext, string Message, string ToAddress, string ToName, string Subject = "")
        {
            _applicationDbContext = appDbContext;
            if (string.IsNullOrEmpty(Message))
            {
                return;
            }
            InitializeVariablesAsync();
            MailMessage mail = new MailMessage(new MailAddress(fromAddress, fromName), new MailAddress(ToAddress, ToName));
            mail.Subject = Subject;
            mail.Body = Message;
            mail.IsBodyHtml = true;
            SmtpClient client = new SmtpClient(smtpServer);
            client.EnableSsl = smtpSSLEnabled;
            client.Port = smtpPort;
            client.Credentials = new NetworkCredential(fromAddress, password);
            client.Send(mail);
        }
    }
}