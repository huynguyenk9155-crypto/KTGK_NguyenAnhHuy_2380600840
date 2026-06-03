using System.ComponentModel.DataAnnotations;

namespace KTGK_LapTrinhWeb.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
