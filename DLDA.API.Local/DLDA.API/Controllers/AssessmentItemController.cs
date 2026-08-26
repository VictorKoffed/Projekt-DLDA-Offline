using Microsoft.EntityFrameworkCore;
using DLDA.API.Data;
using DLDA.API.DTOs;
using DLDA.API.Models;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Manages individual questionnaire items within an assessment, handling answer submissions,
/// skips, and item-level data for both patients and healthcare professionals.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AssessmentItemController : ControllerBase
{
    private readonly AppDbContext _context;

    public AssessmentItemController(AppDbContext context)
    {
        _context = context;
    }

    // ----------------------
    // [PATIENT] – Retrieval and update of personal answers
    // ----------------------

    // GET: api/AssessmentItem/patient/user/{userId}
    // Returns a summary list of assessments belonging to a specific patient for navigation.
    [HttpGet("patient/user/{userId}")]
    public ActionResult<IEnumerable<object>> GetPatientAssessmentList(int userId)
    {
        var assessments = _context.Assessments
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new
            {
                a.AssessmentID,
                a.CreatedAt,
                a.ScaleType,
                a.IsComplete
            })
            .ToList();

        return Ok(assessments);
    }

    // GET: api/AssessmentItem/patient/assessment/{assessmentId}
    // Returns strictly patient-facing answers and states for a specific assessment form.
    [HttpGet("patient/assessment/{assessmentId}")]
    public ActionResult<IEnumerable<object>> GetPatientAnswers(int assessmentId)
    {
        var items = _context.AssessmentItems
            .Where(ai => ai.AssessmentID == assessmentId)
            .Include(ai => ai.Question)
            .OrderBy(ai => ai.QuestionID)
            .Select(ai => new
            {
                ai.ItemID,
                ai.AssessmentID,
                ai.QuestionID,
                QuestionText = ai.Question != null ? ai.Question.QuestionText : "",
                PatientAnswer = ai.PatientAnswer,
                Flag = ai.Flag,
                SkippedByPatient = ai.SkippedByPatient
            })
            .ToList();

        return Ok(items);
    }

    // PUT: api/AssessmentItem/patient/{id}
    // Updates a patient's personal answer for a specific question item.
    [HttpPut("patient/{id}")]
    public IActionResult UpdatePatientAnswer(int id, [FromBody] PatientAnswerDto dto)
    {
        Console.WriteLine($"[INFO] PUT patient/{id} – incoming answer: {dto.Answer}, comment: {dto.Comment}");

        var item = _context.AssessmentItems.Find(id);
        if (item == null)
        {
            Console.WriteLine($"[ERROR] Could not find AssessmentItem with ID={id}");
            return NotFound();
        }

        // Persist patient response data and clear skip flags upon receiving an active answer
        item.PatientAnswer = dto.Answer;
        item.PatientComment = dto.Comment;
        item.AnsweredAt = DateTime.UtcNow;
        item.SkippedByPatient = false;

        // Reset potential previous skip state to ensure data consistency when updating an answer
        if (item.SkippedByPatient)
        {
            Console.WriteLine($"[DEBUG] Question was previously skipped – resetting SkippedByPatient to false.");
            item.SkippedByPatient = false;
        }

        try
        {
            _context.SaveChanges();
            Console.WriteLine($"[SUCCESS] Answer saved for ItemID={id}: Answer={item.PatientAnswer}");
            return NoContent();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to save answer for ItemID={id}: {ex.Message}");
            return StatusCode(500, "Could not save the answer.");
        }
    }


    // PUT: api/AssessmentItem/skip/{itemId}
    // Marks a question item as intentionally skipped by the patient.
    [HttpPut("skip/{itemId}")]
    public IActionResult SkipQuestion(int itemId)
    {
        var item = _context.AssessmentItems.Find(itemId);
        if (item == null) return NotFound();

        item.SkippedByPatient = true;
        item.AnsweredAt = DateTime.UtcNow;

        _context.SaveChanges();
        return NoContent();
    }

    // GET: api/AssessmentItem/patient/assessment/{assessmentId}/overview
    // Returns a comprehensive overview of a patient's assessment progress and questions.
    [HttpGet("patient/assessment/{assessmentId}/overview")]
    public async Task<ActionResult<AssessmentOverviewDto>> GetAssessmentOverview(int assessmentId)
    {
        var assessment = await _context.Assessments
            .Where(a => a.AssessmentID == assessmentId)
            .FirstOrDefaultAsync();

        if (assessment == null)
            return NotFound();

        var items = await _context.AssessmentItems
            .Where(ai => ai.AssessmentID == assessmentId)
            .Include(ai => ai.Question)
            .OrderBy(ai => ai.Order)
            .ToListAsync();

        var total = items.Count; // ✅ Tracks total scope context for UI step calculations

        var questions = items.Select(ai => new QuestionOverviewDto
        {
            ItemID = ai.ItemID, // ✅ Required for targeted client PUT requests
            QuestionId = ai.QuestionID,
            QuestionText = ai.Question?.QuestionText ?? "Frågetext saknas",
            PatientAnswer = ai.PatientAnswer is >= 0 and <= 4 ? ai.PatientAnswer : null,
            PatientComment = ai.PatientComment,
            Order = ai.Order,
            Total = total
        }).ToList();

        var overview = new AssessmentOverviewDto
        {
            AssessmentId = assessment.AssessmentID,
            ScaleType = assessment.ScaleType,
            IsComplete = assessment.IsComplete,
            CreatedAt = assessment.CreatedAt ?? DateTime.MinValue,
            Questions = questions
        };

        return Ok(overview);
    }




    // POST: api/AssessmentItem/assessment/{assessmentId}/complete
    // Marks an entire assessment container as fully completed by the patient.
    [HttpPost("assessment/{assessmentId}/complete")]
    public IActionResult CompleteAssessment(int assessmentId)
    {
        var assessment = _context.Assessments.Find(assessmentId);
        if (assessment == null) return NotFound();

        assessment.IsComplete = true;
        _context.SaveChanges();

        return NoContent();
    }

    // ----------------------
    // [STAFF / ADMIN] – Full access to all answers and actions
    // ----------------------

    // GET: api/AssessmentItem
    // Returns all assessment items system-wide, including both staff and patient inputs.
    [HttpGet]
    public ActionResult<IEnumerable<AssessmentItemDto>> GetItems()
    {
        return _context.AssessmentItems
            .Select(ai => new AssessmentItemDto
            {
                ItemID = ai.ItemID,
                AssessmentID = ai.AssessmentID,
                QuestionID = ai.QuestionID,
                PatientAnswer = ai.PatientAnswer,
                StaffAnswer = ai.StaffAnswer,
                Flag = ai.Flag,
                SkippedByPatient = ai.SkippedByPatient
            })
            .ToList();
    }

    // GET: api/AssessmentItem/staff/assessment/{assessmentId}/overview
    // Returns a detailed clinical comparison view containing both patient and staff metrics.
    [HttpGet("staff/assessment/{assessmentId}/overview")]
    public async Task<ActionResult<StaffResultOverviewDto>> GetStaffAssessmentOverview(int assessmentId)
    {
        var assessment = await _context.Assessments
            .Include(a => a.User) // ✅ Eagerly loads user relationship to resolve patient username for clinical logs
            .FirstOrDefaultAsync(a => a.AssessmentID == assessmentId);

        if (assessment == null)
            return NotFound();

        var items = await _context.AssessmentItems
            .Where(ai => ai.AssessmentID == assessmentId)
            .Include(ai => ai.Question)
            .OrderBy(ai => ai.Order)
            .ToListAsync();

        var dto = new StaffResultOverviewDto
        {
            AssessmentId = assessment.AssessmentID,
            UserId = assessment.UserId,
            Username = assessment.User?.Username ?? "Okänd", // ✅ Exposes user identifier property for UI display headers
            CreatedAt = assessment.CreatedAt ?? DateTime.MinValue,
            IsStaffComplete = assessment.IsStaffComplete,
            Questions = items.Select(ai => new StaffResultRowDto
            {
                ItemID = ai.ItemID,
                Order = ai.Order,
                QuestionText = ai.Question?.QuestionText ?? "Frågetext saknas",
                PatientAnswer = ai.PatientAnswer,
                StaffAnswer = ai.StaffAnswer,
                PatientComment = ai.PatientComment,
                StaffComment = ai.StaffComment,
                Flag = ai.Flag,
                SkippedByPatient = ai.SkippedByPatient
                // Difference is computed automatically by the DTO definition
            }).ToList()
        };

        return Ok(dto);
    }


    // GET: api/AssessmentItem/{id}
    // Retrieves a specific assessment item by its primary key identifier.
    [HttpGet("{id}")]
    public ActionResult<AssessmentItemDto> GetItem(int id)
    {
        var item = _context.AssessmentItems.Find(id);
        if (item == null) return NotFound();

        return new AssessmentItemDto
        {
            ItemID = item.ItemID,
            AssessmentID = item.AssessmentID,
            QuestionID = item.QuestionID,
            PatientAnswer = item.PatientAnswer,
            StaffAnswer = item.StaffAnswer,
            Flag = item.Flag,
            SkippedByPatient = item.SkippedByPatient
        };
    }

    // GET: api/AssessmentItem/patient/assessment/{assessmentId}/question/{order}
    // Retrieves a single questionnaire item matching a specific navigational sequence index.
    [HttpGet("patient/assessment/{assessmentId}/question/{order}")]
    public async Task<ActionResult<QuestionDto>> GetQuestionByOrder(int assessmentId, int order)
    {
        var item = await _context.AssessmentItems
            .Include(i => i.Question)
            .FirstOrDefaultAsync(i => i.AssessmentID == assessmentId && i.Order == order);

        if (item == null || item.Question == null)
            return NotFound();

        var total = await _context.AssessmentItems
            .CountAsync(i => i.AssessmentID == assessmentId);

        var assessment = await _context.Assessments
            .FirstOrDefaultAsync(a => a.AssessmentID == assessmentId);

        return Ok(new QuestionDto
        {
            ItemID = item.ItemID,
            AssessmentID = item.AssessmentID,
            QuestionID = item.Question.QuestionID,
            QuestionText = item.Question.QuestionText!,
            Category = item.Question.Category!,
            IsActive = item.Question.IsActive,
            Order = item.Order,
            Total = total,
            ScaleType = assessment?.ScaleType
        });
    }

    // POST: api/AssessmentItem
    // Creates a new assessment item manually (e.g., administrative structural adjustments).
    [HttpPost]
    public IActionResult CreateItem(AssessmentItemDto dto)
    {
        var item = new AssessmentItem
        {
            AssessmentID = dto.AssessmentID,
            QuestionID = dto.QuestionID,
            PatientAnswer = dto.PatientAnswer ?? -1,
            StaffAnswer = dto.StaffAnswer,
            Flag = dto.Flag,
            SkippedByPatient = false,
            AnsweredAt = DateTime.UtcNow
        };

        _context.AssessmentItems.Add(item);
        _context.SaveChanges();

        return CreatedAtAction(nameof(GetItem), new { id = item.ItemID }, dto);
    }

    // PUT: api/AssessmentItem/staff/{id}
    // Updates a healthcare professional's evaluation response for a specific assessment item.
    [HttpPut("staff/{id}")]
    public IActionResult UpdateStaffAnswer(int id, [FromBody] StaffAnswerDto dto)
    {
        var item = _context.AssessmentItems.Find(id);
        if (item == null) return NotFound();

        item.StaffAnswer = dto.Answer;
        item.StaffComment = dto.Comment;
        item.Flag = dto.Flag ?? false;
        item.AnsweredAt = DateTime.UtcNow;

        _context.SaveChanges();
        return NoContent();
    }

    // DELETE: api/AssessmentItem/{id}
    // Removes an assessment item entity from the database.
    [HttpDelete("{id}")]
    public IActionResult DeleteItem(int id)
    {
        var item = _context.AssessmentItems.Find(id);
        if (item == null) return NotFound();

        _context.AssessmentItems.Remove(item);
        _context.SaveChanges();

        return NoContent();
    }

    // POST: api/AssessmentItem/assessment/{assessmentId}/staff-complete
    /// <summary>
    /// Marks that the healthcare professional has finalized an assessment, even if certain questions remain unanswered.
    /// </summary>
    [HttpPost("assessment/{assessmentId}/staff-complete")]
    public IActionResult CompleteStaffAssessment(int assessmentId)
    {
        var assessment = _context.Assessments
            .Include(a => a.AssessmentItems)
            .FirstOrDefault(a => a.AssessmentID == assessmentId);

        if (assessment == null)
            return NotFound();

        // Calculate the number of unaddressed questions to monitor clinical completion quality
        var unansweredCount = assessment.AssessmentItems
            .Count(i => !i.StaffAnswer.HasValue);

        // Log administrative warnings for any blank items remaining upon sign-off
        if (unansweredCount > 0)
        {
            Console.WriteLine($"[INFO] Assessment {assessmentId} marked complete by staff despite {unansweredCount} unanswered items.");
        }

        // Set finalization flag regardless of empty entries to allow clinical flexibility
        assessment.IsStaffComplete = true;
        _context.SaveChanges();

        return NoContent();
    }
}