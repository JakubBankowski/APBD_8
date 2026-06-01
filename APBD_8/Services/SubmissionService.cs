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

    public async Task<(bool IsSuccess, string ErrorMessage)> GradeSubmissionAsync(
        int idSubmission, GradeSubmissionDTO dto)
    {
        var submission = await _context.Submissions
            .Include(s => s.Assignment)
            .FirstOrDefaultAsync(s => s.SubmissionId == idSubmission);

        if (submission == null) return (false, "Didnt find submission");

        if (dto.Score < 0) return (false, "Score cannot be lower than 0");
        if (dto.Score > submission.Assignment.MaxPoints)
        {
            return (false, "Score cannot be higher than MaxPoints");
        }

        submission.Score = dto.Score;
        submission.Feedback = dto.Feedback;
        submission.Status = "Graded";

        await _context.SaveChangesAsync();
        return (true, string.Empty);
    }

    public async Task<(int StatusCode, string ErrorMessage)> DeleteSubmissionAsync(int idSubmission)
    {
        var submission = await _context.Submissions.FindAsync(idSubmission);
        if (submission == null) return (404, "Not found");

        if (submission.Status == "Graded")
        {
            return (400, "Graded submission cannot be deleted");
        }

        _context.Submissions.Remove(submission);
        await _context.SaveChangesAsync();
        
        return (204, string.Empty);
    }
}