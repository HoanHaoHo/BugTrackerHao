using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using BugTracker.Helper;
using BugTracker.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace BugTracker.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
      

        public ActionResult Index(string id)
        {
            return View(db.Tickets.Include(t => t.TicketPriority).Include(t => t.Project).Include(t => t.TicketStatus).Include(t => t.TicketType).ToList());
        }
        [Authorize(Roles = "Submitter")]
        public ActionResult SubmitterTickets()
        {
            var userId = User.Identity.GetUserId();
            var ticketsSubmitter = db.Tickets.Where(p => p.CreatorId == userId).Include(t => t.Assignee).Include(t => t.Creator).Include(t => t.Project).Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType);
            return View("Index", ticketsSubmitter.ToList());
        }
        [Authorize(Roles = "Project Manager")]
        public ActionResult ProjectManagerTickets()
        {
            var userId = User.Identity.GetUserId();
            var ticketsManager = db.Tickets.Where(ticket => ticket.Project.Users.Any(user => user.Id == userId)).Include(t => t.Assignee).Include(t => t.Creator).Include(t => t.Project).Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType);
            return View("Index", ticketsManager.ToList());
        }
        [Authorize(Roles = "Developer")]
        public ActionResult DeveloperTickets()
        {
            var userId = User.Identity.GetUserId();
            var ticketsDeveloper = db.Tickets.Where(ticket => ticket.AssigneeId == userId).Include(t => t.Assignee).Include(t => t.Creator).Include(t => t.Project).Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType);
            return View("Index", ticketsDeveloper.ToList());
        }
        public ActionResult TicketAssignToDeveloper(int id)
        {
            var model = new TicketAssignViewModel();
            model.Id = id;
            var ticket = db.Tickets.FirstOrDefault(t => t.Id == id);
            var users = RoleChange("Developer");
            model.Developer = new SelectList(users, "Id", "Name");
            return View(model);
        }

        [HttpPost]
        public ActionResult TicketAssignToDeveloper(TicketAssignViewModel model)
        {
            //STEP 1: Find the project
            var ticket = db.Tickets.FirstOrDefault(t => t.Id == model.Id);
            ////STEP 2: Remove all assigned users from ticket
            //var assignedUsers = ticket.Users.ToList();
            //foreach (var userA in assignedUsers)
            //{
            //    ticket.Users.Remove(userA);
            //}

            //STEP 3: Assign users to the ticket
            if (model.SelectedUsers != null)
            {
                foreach (var users in model.SelectedUsers)
                {
                    var user = db.Users.FirstOrDefault(t => t.Id == users);
                    ticket.Users.Add(user);
                    ticket.AssigneeId = user.Id;
                }
            }
            //STEP 4: Save changes to the database
            if(ticket.AssigneeId != null)
            {
                var user = db.Users.FirstOrDefault(p => p.Id == ticket.AssigneeId);
                //Plug in your email service here to send an email.
                var newEmailService = new PersonalEmail();
                var newMail = new MailMessage(WebConfigurationManager.AppSettings["emailto"], user.Email)
                {
                    Body = "You are assigned to new ticket, check your ticket",
                    Subject = "Check your ticket",
                    IsBodyHtml = true
                };
                newEmailService.Warning(newMail);
            }
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        // GET: Tickets/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }
        [Authorize(Roles = "Submitter")]
        // GET: Tickets/Create
        public ActionResult Create()
        {

            //ViewBag.AssigneeId = new SelectList(db.Users, "Id", "FirstName");
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name");
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name");
            ViewBag.TicketStatusId = new SelectList(db.TicketStatus, "Id", "Name");
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name");
            return View();
        }
         
        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Title,Description,TicketTypeId,TicketPriorityId,TicketStatusId,ProjectId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                ticket.Created = DateTimeOffset.Now;
                ticket.CreatorId = User.Identity.GetUserId();
                
                db.Tickets.Add(ticket);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            
            //ViewBag.AssigneeId = new SelectList(db.Users, "Id", "FirstName", ticket.AssigneeId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatus, "Id", "Name", ticket.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }
        [Authorize(Roles ="Admin,Project Manager,Developer")]
        // GET: Tickets/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }

       
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatus, "Id", "Name", ticket.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);


            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,Description,Created,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,AssigneeId")] Ticket ticket)
        {
           
            if (ModelState.IsValid)
            {
                var dateChanged = DateTimeOffset.Now;
                var changes = new List<TicketHistories>();
                var dbTicket = db.Tickets.First(p => p.Id == ticket.Id);
                dbTicket.Title = ticket.Title;
                dbTicket.Description = ticket.Description;
                dbTicket.TicketTypeId = ticket.TicketTypeId;
                dbTicket.Updated = dateChanged;
                var originalValues = db.Entry(dbTicket).OriginalValues;
                var currentValues = db.Entry(dbTicket).CurrentValues;
                foreach (var property in originalValues.PropertyNames)
                {
                    var originalValue = originalValues[property]?.ToString();
                    var currentValue = currentValues[property]?.ToString();
                    if (originalValue != currentValue)
                    {
                        var history = new TicketHistories();
                        history.Changed = dateChanged;
                        history.NewValue = currentValue;
                        history.OldValue = originalValue;
                        history.Property = property;
                        history.TicketId = dbTicket.Id;
                        history.UserId = User.Identity.GetUserId();
                        changes.Add(history);
                    }
                }
                db.TicketHistories.AddRange(changes);
                db.SaveChanges();

                return RedirectToAction("Index");  

            }
            
            return View(ticket);
        }
        private string GetValueFromKey(string propertyName, string key)
        {
            if (propertyName == "TicketTypeId")
            {
                return db.TicketTypes.Find(Convert.ToInt32(key)).Name;
            }
            return key;
        }
        [Authorize(Roles = "Admin,Project Manager")]
        // GET: Tickets/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }
        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Ticket ticket = db.Tickets.Find(id);
            db.Tickets.Remove(ticket);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ICollection<ApplicationUser> RoleChange(string role)
        {
            var userId = db.Roles.Where(p => p.Name == role).Select(p => p.Id).FirstOrDefault();
            return db.Users.Where(p => p.Roles.Any(t => t.RoleId == userId)).ToList();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
