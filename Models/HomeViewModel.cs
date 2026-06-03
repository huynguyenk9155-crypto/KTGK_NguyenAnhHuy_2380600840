using System.Collections.Generic;
using KTGK_LapTrinhWeb.Models;

namespace KTGK_LapTrinhWeb.Models
{
    public class HomeViewModel
    {
        public List<Course> Courses { get; set; } = new List<Course>();
        public string SearchTerm { get; set; } = string.Empty;
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public List<int> EnrolledCourseIds { get; set; } = new List<int>();
    }
}
