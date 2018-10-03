namespace BugTracker.Migrations
{
    using BugTracker.Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<BugTracker.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(BugTracker.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.
            //create a few roles (admin and moderator)
            var roleManager = new RoleManager<IdentityRole>(
                new RoleStore<IdentityRole>(context));

            

            if (!context.Roles.Any(r => r.Name == "Admin"))
            {
                roleManager.Create(new IdentityRole { Name = "Admin" });
            }

            if (!context.Roles.Any(r => r.Name == "Project Manager"))
            {
                roleManager.Create(new IdentityRole { Name = "Project Manager" });
            }

            if (!context.Roles.Any(r => r.Name == "Developer"))
            {
                roleManager.Create(new IdentityRole { Name = "Developer" });
            }

            if (!context.Roles.Any(r => r.Name == "Submitter"))
            {
                roleManager.Create(new IdentityRole { Name = "Submitter" });
            }

            
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            if (!context.Users.Any(u => u.Email == "hohoanhao94@gmail.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "hohoanhao94@gmail.com",
                    Email = "hohoanhao94@gmail.com",
                    Name ="Hao",
                    FirstName = "HoanHao",
                    LastName = "Ho",
                    DisplayName = "HaoHo",
                }, "Hohoanhao123456@");
            }
            var adminId = userManager.FindByEmail("hohoanhao94@gmail.com").Id;
            userManager.AddToRole(adminId, "Admin");

            if (!context.Users.Any(u => u.Email == "ho_hoan_hao94@yahoo.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "ho_hoan_hao94@yahoo.com",
                    Email = "ho_hoan_hao94@yahoo.com",
                    Name="Hao2",
                    FirstName = "HaoHoan",
                    LastName = "Ho",
                    DisplayName = "HaoHo",
                }, "Hohoanhao123456@");
            }
            var projectManagerId = userManager.FindByEmail("hohoanhao94@gmail.com").Id;
            userManager.AddToRole(projectManagerId, "Project Manager");

        }
    }
}
