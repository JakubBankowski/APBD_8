using APBD_8.Data;
using APBD_8.DTOs;
using APBD_8.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APBD_8.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly UniversityTasksDbContext _context;
    
    public CoursesController(UniversityTasksDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetCourses([FromQuery] bool activeOnly = true)
    {
        var query = _context.Courses.AsNoTracking();

        if (activeOnly) query = query.Where(c => c.IsActive);

        var courses = await query.Select(c => new CourseDTO
        {
            IdCourse = c.CourseId,
            Code = c.Code,
            Name = c.Name,
            Credits = c.Credits,
            AssignmentCount = c.Assignments.Count
        }).ToListAsync();
        
        return Ok(courses);
    }

    public async Task<IActionResult> GetAssignmentsForCourse(int idCourse, [FromQuery] bool publishedOnly = true)
    {
        var courseExists = await _context.Courses.AnyAsync(c => c.CourseId == idCourse);
        if (!courseExists) return NotFound("Course was not found");
        
        var query = _context.Assignments.AsNoTracking().Where(a => a.CourseId == idCourse);

        if (publishedOnly) query = query.Where(a => a.IsPublished);

        var assignments = await query.Select(a => new AssignmentDTO
        {
            IdAssignment = a.AssignmentId,
            Title = a.Title,
            DueDate = a.DueDate,
            MaxPoints = a.MaxPoints,
            IsPublished = a.IsPublished,
            SubmissionCount = a.Submissions.Count
        }).ToListAsync();

        return Ok(assignments);
    }
}