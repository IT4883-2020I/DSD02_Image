using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace IT4483Image.Models
{
    public class StreamContext : DbContext
    {
        public StreamContext(DbContextOptions<StreamContext> options): base(options)
        {
            
        }
        public DbSet<Stream> Streams { get; set; }
    }
}
