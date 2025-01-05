using System;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace StudentManagement.Module.BusinessObjects
{
    public class EmailHelper
    {
        // Cấu hình email mặc định
        private static readonly string SmtpHost = "smtp.gmail.com";
        private static readonly int SmtpPort = 587;
        private static readonly string SenderEmail = "luuquan232003@gmail.com";
        private static readonly string SenderPassword = "oiel itig mkpf dvlh";
        private static readonly string SenderName = "Phòng Đào Tạo";

        // Phương thức gửi email đơn giản và đồng bộ
        public static void SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                using (var mail = new MailMessage())
                {
                    mail.From = new MailAddress(SenderEmail, SenderName);
                    mail.To.Add(toEmail);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;
                    mail.BodyEncoding = Encoding.UTF8;
                    mail.SubjectEncoding = Encoding.UTF8;

                    using (var smtp = new SmtpClient(SmtpHost))
                    {
                        smtp.Port = SmtpPort;
                        smtp.Credentials = new NetworkCredential(SenderEmail, SenderPassword);
                        smtp.EnableSsl = true;
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                        // Gửi email đồng bộ
                        smtp.Send(mail);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi gửi email: " + ex.Message);
            }
        }

        // Phương thức hỗ trợ gửi HTML email
        public static void SendHtmlEmail(string toEmail, string subject, string htmlBody)
        {
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <div style='background-color: #f8f9fa; padding: 20px; text-align: center; margin-bottom: 20px;'>
                            <h2>Thông Báo</h2>
                        </div>
                        <div style='padding: 20px;'>
                            {htmlBody}
                        </div>
                        <div style='text-align: center; padding: 20px; font-size: 12px; color: #6c757d;'>
                            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
                            <p>© {DateTime.Now.Year} Phòng Đào Tạo</p>
                        </div>
                    </div>
                </body>
                </html>";

            SendEmail(toEmail, subject, body);
        }
    }
}