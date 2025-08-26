using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.EL.Models
{
    public class PackagingStyleForms
    {
        public int PackagingStyleId { get; set; }
        public PackagingStyles PackagingStyle { get; set; }

        public int FormId { get; set; }
        public FormStyle Form { get; set; }

    }
}
