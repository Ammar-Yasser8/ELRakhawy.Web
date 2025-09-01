using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.EL.Models
{
    public class YarnItem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الصنف مطلوب")]
        [MaxLength(200, ErrorMessage = "اسم الصنف يجب ألا يتجاوز 200 حرف")]
        [Display(Name = "الصنف")]
        public string Item { get; set; }

        [Display(Name = "الغزل المكون")]
        public int? OriginYarnId { get; set; } // Self FK to YarnItem
        public virtual YarnItem? OriginYarn { get; set; } // الغزل المكون (أب)

        [InverseProperty("OriginYarn")]
        public virtual ICollection<YarnItem> DerivedYarns { get; set; } = new List<YarnItem>(); // الأصناف المشتقة (أبناء)

        public virtual ICollection<Manufacturers> Manufacturers { get; set; } = new List<Manufacturers>();


        [Required(ErrorMessage = "الحالة مطلوبة")]
        [Display(Name = "الحالة")]
        public bool Status { get; set; } = true;

        [Display(Name = "بيان")]
        [MaxLength(1000)] // Added max length for Comment
        public string? Comment { get; set; }

        [InverseProperty("YarnItem")]
        public virtual ICollection<YarnTransaction> YarnTransactions { get; set; } = new List<YarnTransaction>();

        // Helper method to check if deletion is safe
        public bool CanBeDeleted()
        {
            return !YarnTransactions.Any() && !DerivedYarns.Any();
        }

        // Helper method to get hierarchy level
        public int GetHierarchyLevel()
        {
            int level = 0;
            var current = this;
            var visited = new HashSet<int>();

            while (current?.OriginYarnId.HasValue == true && !visited.Contains(current.Id))
            {
                visited.Add(current.Id);
                current = current.OriginYarn;
                level++;

                // Prevent infinite loops (circular references)
                if (level > 10) break;
            }

            return level;
        }

        // Helper method to get full hierarchy path
        public string GetHierarchyPath()
        {
            var path = new List<string>();
            var current = this;
            var visited = new HashSet<int>();

            while (current != null && !visited.Contains(current.Id))
            {
                visited.Add(current.Id);
                path.Insert(0, current.Item);
                current = current.OriginYarn;

                if (path.Count > 10) break; // Prevent infinite loops
            }

            return string.Join(" ← ", path);
        }

        // Helper method to check for circular reference before setting origin
        public bool WouldCreateCircularReference(int? newOriginYarnId)
        {
            if (!newOriginYarnId.HasValue) return false;
            if (newOriginYarnId.Value == this.Id) return true;

            // Check if the new origin yarn has this item in its hierarchy
            var visited = new HashSet<int> { this.Id };
            return CheckCircularReference(newOriginYarnId.Value, visited);
        }

        private bool CheckCircularReference(int yarnId, HashSet<int> visited)
        {
            if (visited.Contains(yarnId)) return true;

            visited.Add(yarnId);
            // This would need to be implemented with repository access
            // For now, return false - implement in service layer
            return false;
        }
    }


}
