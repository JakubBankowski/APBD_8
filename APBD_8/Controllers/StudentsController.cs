using APBD_8.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APBD_8.DTOs;
namespace APBD_8.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly UniversityTasksDbContext _context;
    
    public StudentsController(UniversityTasksDbContext context)
    {
        _context = context;
    }

    [HttpGet("{idStudent}/dashboard")]
    public async Task<IActionResult> GetStudentDashboard(int idStudent)
    {
        var dashboard = await _context.Students
            .AsNoTracking()
            .Where(s => s.StudentId == idStudent)
            .Select(s => new StudentDashboardDTO{
                IdStudent = s.StudentId,
                IndexNumber = s.IndexNumber,
                FullName = $"{s.FirstName} {s.LastName}", 
                IsActive = s.IsActive,
                Enrollments = s.Enrollments.Select(e => e.Course.Name).ToList(),
                Submissions = s.Submissions.Select(sub => new SubmissionDTO{
                    SubmissionId = sub.SubmissionId,
                    StudentName = $"{s.FirstName} {s.LastName}",
                    AssignmentTitle = sub.Assignment.Title,
                    RepositoryUrl = sub.RepositoryUrl,
                    Status = sub.Status,
                    Score = sub.Score,
                    Feedback = sub.Feedback
                }).ToList() 
            })
            .FirstOrDefaultAsync();

        if (dashboard == null) return NotFound($"Student with ID {idStudent} not found.");

        return Ok(dashboard);
    }
}