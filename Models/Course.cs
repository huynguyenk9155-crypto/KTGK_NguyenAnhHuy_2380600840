using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KTGK_LapTrinhWeb.Models
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên học phần không được để trống")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Image { get; set; } = string.Empty;

        [Range(1, 10, ErrorMessage = "Số tín chỉ phải từ 1 đến 10")]
        public int Credits { get; set; }

        [Required(ErrorMessage = "Tên giảng viên không được để trống")]
        [StringLength(100)]
        public string Lecturer { get; set; } = string.Empty;

        [Required(ErrorMessage = "Danh mục học phần là bắt buộc")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
