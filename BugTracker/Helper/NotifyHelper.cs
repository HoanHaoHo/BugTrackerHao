using BugTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Configuration;

namespace BugTracker.Helper
{
    public static class NotifyHelper
    {
        private static ApplicationDbContext db = new ApplicationDbContext();
        public static void SendEmail(this Ticket ticket, bool notification)
        {
            if(notification == true)
            {
                var user = db.Users.FirstOrDefault(p => p.Id == ticket.AssigneeId);
                //Plug in your email service here to send an email.
                var newEmailService = new PersonalEmail();
                var newMail = new MailMessage(WebConfigurationManager.AppSettings["emailto"], user.Email)
                {
                    Body = "You got new Attachment, check your details ticket",
                    Subject = "Check your details ticket",
                    IsBodyHtml = true
                };
                newEmailService.Warning(newMail);
            }
            if(notification == false)
            {
                var user = db.Users.FirstOrDefault(p => p.Id == ticket.AssigneeId);
                //Plug in your email service here to send an email.
                var newEmailService = new PersonalEmail();
                var newMail = new MailMessage(WebConfigurationManager.AppSettings["emailto"], user.Email)
                {
                    Body = "You got new Comment, check your details ticket",
                    Subject = "Check your details ticket",
                    IsBodyHtml = true
                };
                newEmailService.Warning(newMail);
            }
        }
    }
}