namespace ZenlyAPI.Domain.Entities.Shared;

public enum CourseType
{
    Compulsory = 1,
    Elective = 2,
}

public enum SemesterType {
    First = 1,
    Second = 2,
}

public enum ComplaintStatus
{
    Pending = 1,
    Ongoing = 2,
    Resolved = 3,
    Canceled = 4,
}

public enum ComplaintType
{
    AssessmentsAndExams = 1,
    Projects = 2, 
    Presentations = 3,
    Other = 4
}

public enum ComplaintActionType
{
    Create = 1,
    Update = 2,
    Delete = 3,
    Other = 4
}