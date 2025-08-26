using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.EL.Models
{
    public class StakeholderInfoType
    {

        public int Id { get; set; }

        public int StakeholdersInfoId { get; set; }
        public StakeholdersInfo StakeholdersInfo { get; set; }

        public bool IsPrimary { get; set; } // Indicates if this is the primary type for the stakeholder

        public int StakeholderTypeId { get; set; }
        public StakeholderType StakeholderType { get; set; }

    }
}
