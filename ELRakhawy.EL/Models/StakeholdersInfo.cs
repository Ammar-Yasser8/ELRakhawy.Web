using System.ComponentModel.DataAnnotations;

namespace ELRakhawy.EL.Models
{
    public class StakeholdersInfo
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        [Display(Name = "الجهة")]
        public string Name { get; set; }

        public bool Status { get; set; } // true = Active, false = Inactive

        public string? ContactNumbers { get; set; }

        public string? Comment { get; set; }


        public ICollection<StakeholderInfoType> StakeholderInfoTypes { get; set; } = new List<StakeholderInfoType>();

    }
}