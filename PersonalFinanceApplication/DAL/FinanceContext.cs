using PersonalFinanceApplication.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace PersonalFinanceApplication.DAL
{
    public class PersonalFinanceApplicationContext : DbContext
    {
        public PersonalFinanceApplicationContext() : base("PersonalFinanceApplicationContext")
        {

        }

        //Create the DbSets for the different models
        //public DbSet<Character> Characters { get; set; }
        //public DbSet<Question> Questions { get; set; }
    }
}