namespace APBD_8.DTOs;

public class StudentDashboardDTO
{
    public int IdStudent;
    public string IndexNumber;
    public string FullName;
    public bool IsActive;
    public List<string> Enrollments;
    public List<SubmissionDTO> Submissions;
}