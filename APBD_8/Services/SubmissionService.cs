using APBD_8.Data;
using APBD_8.DTOs;
using APBD_8.Models;
using Microsoft.EntityFrameworkCore;

namespace APBD_8.Services;

public class SubmissionService
{
    private readonly UniversityTasksDbContext _context;
    public SubmissionService(UniversityTasksDbContext context)
    {
        _context = context;
    }

    public async Task<(bool isSuccess, string ErrorMessage, Submission? Submission)> CreateSubmission(
        CreateSubmissionDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.RepositoryUrl) ||
            !dto.RepositoryUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            return (false, "Repository URL cannot be blank and must start with 'https://'.", null);
        }

        var student = await _context.Students.FindAsync(dto.StudentId);
        if (student == null) return (false, "Student does not exist.", null);
        if (!student.IsActive) return (false, "Student is not active", null);

        var assignment = await _context.Assignments.FindAsync(dto.AssignmentId);
        if (assignment == null) return (false, "Assignment does not exist.", null);
        if (!assignment.IsPublished) return (false, "Assignment is not published", null);

        var isEnrolled = await _context.Enrollments.AnyAsync(e =>
            e.StudentId == dto.StudentId &&
            e.CourseId == assignment.CourseId &&
            (e.Status == "Active" || e.Status == "Completed"));

        if (!isEnrolled)
        {
            return (false, "Student is not actively enrolled in this course", null);
        }

        var alreadySubmitted = await _context.Submissions.AnyAsync(s =>
            s.StudentId == dto.StudentId && s.AssignmentId == dto.AssignmentId);

        if (alreadySubmitted) return (false, "Student has already submitted", null);

        var now = DateTime.UtcNow;
        string status = assignment.IsOverdue(now) ? "Late" : "Submitted";

        var newSubmission = new Submission
        {
            StudentId = dto.StudentId,
            AssignmentId = dto.AssignmentId,
            RepositoryUrl = dto.RepositoryUrl,
            Status = status,
            SubmittedAt = now
        };

        _context.Submissions.Add(newSubmission);
        await _context.SaveChangesAsync();
        
        return (true, string.Empty, newSubmission);
    }
}