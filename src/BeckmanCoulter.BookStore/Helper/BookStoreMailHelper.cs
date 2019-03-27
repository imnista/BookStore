using BeckmanCoulter.BookStore.Extensions;
using Microsoft.AspNetCore.Hosting;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace BeckmanCoulter.BookStore.Helper
{
  public static class BookStoreMailHelper
  {
    public static void SendMail(IHostingEnvironment env)
    {
      var configuration = env.GetAppConfiguration();
      var apiKey = configuration["App:SendGrid:ApiKey"];
      var sendGridClient = new SendGridClient(apiKey);
      var from = new EmailAddress("lfu01@beckman.com", "Lynn");
      var subject = "Sending with SendGrid is Fun";
      var to = new EmailAddress("lfu01@beckman.com", "Lynn");
      var plainTextContent = "and easy to do anywhere, even with C#";
      var htmlContent = "<strong>and easy to do anywhere, even with C#</strong>";
      var mail = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
      var response = sendGridClient.SendEmailAsync(mail).Result;
    }
  }
}
