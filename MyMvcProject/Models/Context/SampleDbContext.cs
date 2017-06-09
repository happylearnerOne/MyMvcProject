using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using MyMvcProject.Models.Context;
using MyMvcProject.Models.Table;

namespace MyMvcProject.Models.Context
{
    public class SampleDbContext : DbContext
    {
        public SampleDbContext() : base("Conn") {
            Database.SetInitializer<SampleDbContext>(null);
        }
        public virtual DbSet<sys_user> SYS_USER { get; set; }
    }
}