using APBD_8.DTOs;
using APBD_8.Models;
using APBD_8.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD_8.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubmissionsController : ControllerBase
{
    private readonly SubmissionService _service;
    public SubmissionsController(SubmissionService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> CreateSubmission([FromBody] CreateSubmissionDTO dto)
    {
        var result = await _service.CreateSubmission(dto);
        
        if (!result.isSuccess) return BadRequest(result.ErrorMessage);

        return CreatedAtAction(
            nameof(StudentsController.GetStudentDashboard),
            "Students",
            new { idStudent = dto.StudentId },
            result.Submission
        );
    }

    [HttpPut("{idSubmission}/grade")]
    public async Task<IActionResult> GradeSubmission(int idSubmission, [FromBody] GradeSubmissionDTO dto)
    {
        var result = await _service.GradeSubmissionAsync(idSubmission, dto);
        
        if (!result.IsSuccess) return BadRequest(result.ErrorMessage);
        
        return Ok("Graded successfully");
    }

    [HttpDelete("{idSubmission}")]
    public async Task<IActionResult> DeleteSubmission(int idSubmission)
    {
        var result = await _service.DeleteSubmissionAsync(idSubmission);

        if (result.StatusCode == 404) return NotFound(result.ErrorMessage);
        if (result.StatusCode==400) return BadRequest(result.ErrorMessage);

        return NoContent();
    }
}