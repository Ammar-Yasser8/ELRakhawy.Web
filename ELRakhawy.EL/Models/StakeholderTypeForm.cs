namespace ELRakhawy.EL.Models
{
    public class StakeholderTypeForm
    {
        public int Id { get; set; }

        public int StakeholderTypeId { get; set; }
        public StakeholderType StakeholderType { get; set; }

        public int FormId { get; set; }
        public FormStyle Form { get; set; }
    }
}