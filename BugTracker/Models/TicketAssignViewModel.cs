using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models
{
    public class TicketAssignViewModel
    {
        public int Id { get; set; }
        public SelectList Developer { get; set; }
        public string[] SelectedUsers { get; set; }
        
    }
}