using ZenlyAPI.Domain.Entities;
using ZenlyAPI.Domain.Entities.Complaints;
using ZenlyAPI.Domain.Entities.Shared;

namespace ZenlyAPI.Domain.User.Students;

public class Student: BaseUser
{
    public string MatricNo { get; set; }
    public virtual ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
    public virtual ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();

}
