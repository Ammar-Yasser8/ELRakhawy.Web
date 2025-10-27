using ELRakhawy.EL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.EL.ViewModels
{
    public class UserRoleGroupViewModel
    {
        public UserRole Role { get; set; }
        public string RoleName { get; set; }
        public int Count { get; set; }
    }
}
