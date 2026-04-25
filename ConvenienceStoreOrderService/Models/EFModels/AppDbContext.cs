using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace ConvenienceStoreOrderService.Models.EFModels
{
    public class AppDbContext : DbContext
    {
       
        
          public AppDbContext() : base("name=AppDbContext")
            {
            }

            public DbSet<Product> Products { get; set; }
        
    }
}