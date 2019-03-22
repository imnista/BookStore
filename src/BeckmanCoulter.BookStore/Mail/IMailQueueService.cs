using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SendGrid.Helpers.Mail;

namespace BeckmanCoulter.BookStore.Mail
{
    public interface IMailQueueService
    {
        Task SendMessage(SendGridMessage message);
    }
}
