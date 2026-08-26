using DLDA.API.Data;
using DLDA.API.DTOs;
using DLDA.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;

/// <summary>
/// Manages the lifecycle, creation, retrieval, and updates of psychiatric assessments (DLDA).
/// Coordinates data handling between patients and healthcare professionals while enforcing structural consistency.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AssessmentController : ControllerBase
{
    private readonly AppDbContext _context;

    public AssessmentController(AppDbContext context)
    {
        _context = context;
    }

    // --------------------------
    // [PATIENT] – Restricted access to personal assessments only
    // --------------------------

    // GET: api/Assessment/user/{userId}
    // Returns all assessments belonging to a specific user, including progress metrics for UI rendering.
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<AssessmentDto>>> GetAssessmentsForUser(int userId)
    {
        return await _context.Assessments
            .Where(a => a.UserId == userId)
            .Include(a => a.AssessmentItems) // 👈 Required to evaluate ongoing answers and completion status per item
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AssessmentDto
            {
                AssessmentID = a.AssessmentID,
                ScaleType = a.ScaleType,
                IsComplete = a.IsComplete,
                IsStaffComplete = a.IsStaffComplete,
                UserId = a.UserId,
                CreatedAt = a.CreatedAt ?? DateTime.MinValue,
                HasStarted = a.AssessmentItems.Any(i => i.PatientAnswer != null || i.SkippedByPatient),
                AnsweredCount = a.AssessmentItems.Count(i => i.PatientAnswer != null || i.SkippedByPatient),
                TotalQuestions = a.AssessmentItems.Count
            })
            .ToListAsync();
    }

    // GET: api/Assessment/{id}
    // Retrieves a specific assessment – ownership validation must be handled upstream in the client/gateway layer.
    [HttpGet("{id}")]
    public async Task<ActionResult<AssessmentDto>> GetAssessment(int id)
    {
        var assessment = await _context.Assessments.FindAsync(id);
        if (assessment == null) return NotFound();

        return new AssessmentDto
        {
            AssessmentID = assessment.AssessmentID,
            ScaleType = assessment.ScaleType,
            IsComplete = assessment.IsComplete,
            UserId = assessment.UserId,
            CreatedAt = assessment.CreatedAt ?? DateTime.MinValue
        };
    }

    /// <summary>
    /// Calculates the total number of questions in an assessment based on the maximum order index.
    /// </summary>
    [HttpGet("{id}/question-count")]
    public async Task<ActionResult<int>> GetQuestionCount(int id)
    {
        var count = await _context.AssessmentItems
            .Where(i => i.AssessmentID == id)
            .MaxAsync(i => (int?)i.Order);

        if (count == null)
            return NotFound("Inga frågor hittades.");

        return Ok(count.Value + 1);
    }

    // --------------------------
    // [STAFF] – Full administrative access to all assessments
    // --------------------------

    // POST: api/Assessment
    // Instantiates a new assessment container and maps all currently active template questions as assessment items.
    [HttpPost]
    public async Task<ActionResult<AssessmentDto>> CreateAssessment(AssessmentDto dto)
    {
        try
        {
            Console.WriteLine($"[INFO] Skapar ny assessment för UserId={dto.UserId}");

            var assessment = new Assessment
            {
                ScaleType = dto.ScaleType,
                IsComplete = dto.IsComplete,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UserId = dto.UserId
            };

            _context.Assessments.Add(assessment);
            await _context.SaveChangesAsync(); // ✅ Persists immediately to generate the primary key ID required for child relations

            Console.WriteLine($"[INFO] Assessment sparad med ID={assessment.AssessmentID}");

            var questions = await _context.Questions
                .Where(q => q.IsActive)
                .OrderBy(q => q.QuestionID)
                .ToListAsync();

            Console.WriteLine($"[INFO] Antal aktiva frågor hämtade: {questions.Count}");

            if (!questions.Any())
            {
                Console.WriteLine("[WARN] Inga aktiva frågor hittades att koppla till bedömningen.");
                return BadRequest("Inga aktiva frågor hittades att koppla till bedömningen.");
            }

            int index = 0;
            foreach (var question in questions)
            {
                var item = new AssessmentItem
                {
                    AssessmentID = assessment.AssessmentID,
                    QuestionID = question.QuestionID,
                    PatientAnswer = null,
                    StaffAnswer = null,
                    Flag = false,
                    SkippedByPatient = false, // ✅ Tracks explicit user skips separately from unanswered states
                    AnsweredAt = null,
                    Order = index++
                };
                _context.AssessmentItems.Add(item);
            }

            Console.WriteLine($"[INFO] Totalt {index} AssessmentItems skapades. Försöker spara...");

            await _context.SaveChangesAsync();

            Console.WriteLine("[SUCCESS] AssessmentItems sparades korrekt.");

            return CreatedAtAction(nameof(GetAssessment), new { id = assessment.AssessmentID }, new AssessmentDto
            {
                AssessmentID = assessment.AssessmentID,
                ScaleType = assessment.ScaleType,
                IsComplete = assessment.IsComplete,
                UserId = assessment.UserId,
                CreatedAt = assessment.CreatedAt ?? DateTime.MinValue
            });
        }
        catch (DbUpdateException dbEx)
        {
            Console.WriteLine($"[DB ERROR] {dbEx.InnerException?.Message ?? dbEx.Message}");
            return StatusCode(500, "Databasfel vid sparande av bedömning.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] {ex.Message}");
            return StatusCode(500, "Ett internt fel uppstod vid skapande av bedömning.");
        }
    }

    // GET: api/Assessment
    // Returns all system assessments for clinical oversight and auditing.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AssessmentDto>>> GetAssessments()
    {
        return await _context.Assessments
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AssessmentDto
            {
                AssessmentID = a.AssessmentID,
                ScaleType = a.ScaleType,
                IsComplete = a.IsComplete,
                UserId = a.UserId,
                CreatedAt = a.CreatedAt ?? DateTime.MinValue
            }).ToListAsync();
    }

    // GET: api/Assessment/search
    // Filters and returns patient assessments matching a given partial username query string.
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<object>>> SearchAssessmentsByPatientName([FromQuery] string name)
    {
        var results = await _context.Assessments
            .Include(a => a.User)
            .Where(a => a.User != null && a.User.Username.ToLower().Contains(name.ToLower()))
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new
            {
                a.AssessmentID,
                CreatedAt = a.CreatedAt ?? DateTime.MinValue,
                a.ScaleType,
                a.IsComplete,
                PatientName = a.User!.Username,
                UserId = a.UserId
            })
            .ToListAsync();

        if (!results.Any())
            return NotFound("Inga bedömningar hittades för angivet namn.");

        return Ok(results);
    }

    // PUT: api/Assessment/{id}
    // Updates metadata for an existing assessment container.
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAssessment(int id, AssessmentDto dto)
    {
        if (id != dto.AssessmentID) return BadRequest();

        var assessment = await _context.Assessments.FindAsync(id);
        if (assessment == null) return NotFound();

        assessment.ScaleType = dto.ScaleType;
        assessment.IsComplete = dto.IsComplete;
        assessment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/Assessment/{id}
    // Deletes an assessment entity and its cascaded relational items.
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAssessment(int id)
    {
        var assessment = await _context.Assessments.FindAsync(id);
        if (assessment == null) return NotFound();

        _context.Assessments.Remove(assessment);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // If the record was already deleted concurrently, treat it as a successful omission to maintain idempotency
            return NoContent();
        }

        return NoContent();
    }

    // POST: /StaffAssessment/Unlock
    // Reverts the completion lock on an assessment to allow modifications by clinical staff.
    [HttpPost("unlock/{assessmentId}")]
    public IActionResult UnlockAssessment(int assessmentId)
    {
        var assessment = _context.Assessments.FirstOrDefault(a => a.AssessmentID == assessmentId);
        if (assessment == null)
            return NotFound();

        assessment.IsStaffComplete = false;
        _context.SaveChanges();

        return Ok();
    }
}