using Microsoft.EntityFrameworkCore;
using DLDA.API.Data;
using DLDA.API.DTOs;
using DLDA.API.Models;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Manages questionnaire templates, administrative CRUD operations for assessment questions,
/// and sequential quiz flows for both patients and clinical staff.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class QuestionController : ControllerBase
{
    private readonly AppDbContext _context;

    public QuestionController(AppDbContext context)
    {
        _context = context;
    }

    // --------------------------
    // [ADMIN] – Question Definition Management
    // --------------------------

    // GET: api/Question
    /// <summary>Retrieves all questions from the database.</summary>
    [HttpGet]
    public ActionResult<IEnumerable<QuestionDto>> GetQuestions()
    {
        return _context.Questions
            .Select(q => new QuestionDto
            {
                QuestionID = q.QuestionID,
                QuestionText = q.QuestionText ?? "",
                Category = q.Category ?? "",
                IsActive = q.IsActive
            }).ToList();
    }

    // GET: api/Question/category/{category}
    /// <summary>Retrieves all questions belonging to a specific category.</summary>
    [HttpGet("category/{category}")]
    public ActionResult<IEnumerable<QuestionDto>> GetQuestionsByCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            return BadRequest("Kategori får inte vara tom.");

        var questions = _context.Questions
            .Where(q => (q.Category ?? "").ToLower() == category.ToLower())
            .Select(q => new QuestionDto
            {
                QuestionID = q.QuestionID,
                QuestionText = q.QuestionText ?? "",
                Category = q.Category ?? "",
                IsActive = q.IsActive
            }).ToList();

        if (!questions.Any()) return NotFound($"Inga frågor hittades för kategori: {category}");

        return Ok(questions);
    }

    // GET: api/Question/5
    /// <summary>Retrieves a specific question by its unique identifier.</summary>
    [HttpGet("{id}")]
    public ActionResult<QuestionDto> GetQuestion(int id)
    {
        var q = _context.Questions.Find(id);
        if (q == null) return NotFound();

        return new QuestionDto
        {
            QuestionID = q.QuestionID,
            QuestionText = q.QuestionText ?? "",
            Category = q.Category ?? "",
            IsActive = q.IsActive
        };
    }

    // POST: api/Question
    /// <summary>Creates a new assessment question template.</summary>
    [HttpPost]
    public IActionResult CreateQuestion(QuestionDto dto)
    {
        var question = new Question
        {
            QuestionText = dto.QuestionText,
            Category = dto.Category,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };
        _context.Questions.Add(question);
        _context.SaveChanges();
        return CreatedAtAction(nameof(GetQuestion), new { id = question.QuestionID }, dto);
    }

    // PUT: api/Question/5
    /// <summary>Updates an existing question definition.</summary>
    [HttpPut("{id}")]
    public IActionResult UpdateQuestion(int id, QuestionDto dto)
    {
        if (id != dto.QuestionID) return BadRequest();

        var question = _context.Questions.Find(id);
        if (question == null) return NotFound();

        question.QuestionText = dto.QuestionText;
        question.Category = dto.Category;
        question.IsActive = dto.IsActive;
        _context.SaveChanges();
        return NoContent();
    }

    // DELETE: api/Question/5
    /// <summary>Deletes a question definition from the database.</summary>
    [HttpDelete("{id}")]
    public IActionResult DeleteQuestion(int id)
    {
        var question = _context.Questions.Find(id);
        if (question == null) return NotFound();

        _context.Questions.Remove(question);
        _context.SaveChanges();
        return NoContent();
    }

    // --------------------------
    // [QUIZ – PATIENT]
    // --------------------------

    // GET: api/Question/quiz/patient/next/{assessmentId}
    /// <summary>Retrieves the next unanswered and unskipped question for the patient wizard.</summary>
    [HttpGet("quiz/patient/next/{assessmentId}")]
    public async Task<ActionResult<QuestionDto>> GetNextUnansweredQuestion(int assessmentId)
    {
        // 1. Attempt to locate the next item that has neither been answered nor intentionally skipped by the patient
        var item = await _context.AssessmentItems
            .Include(ai => ai.Question)
            .Where(ai =>
                ai.AssessmentID == assessmentId &&
                (ai.PatientAnswer == null || ai.PatientAnswer == -1) &&
                !ai.SkippedByPatient)
            .OrderBy(ai => ai.Order)
            .FirstOrDefaultAsync();

        // 2. If all remaining items are either completed or skipped, the questionnaire sequence is finished for the user
        if (item == null)
        {
            return NotFound(new { message = "Alla frågor är besvarade eller överhoppade." });
        }

        var total = await _context.AssessmentItems.CountAsync(ai => ai.AssessmentID == assessmentId);
        var assessment = await _context.Assessments.FirstOrDefaultAsync(a => a.AssessmentID == assessmentId);

        return Ok(new QuestionDto
        {
            AssessmentID = item.AssessmentID,
            AssessmentItemID = item.ItemID, 
            QuestionID = item.QuestionID,
            QuestionText = item.Question?.QuestionText ?? "Frågetext saknas",
            Category = item.Question?.Category ?? "Okänd",
            IsActive = item.Question?.IsActive ?? false,
            Order = item.Order,
            Total = total,
            ScaleType = assessment?.ScaleType ?? "Numerisk",

            // Include previously persisted patient response context if available for pre-population
            PatientAnswer = item.PatientAnswer,
            PatientComment = item.PatientComment
        });
    }




    // POST: api/Question/quiz/patient/submit
    /// <summary>Saves the patient's score and optional commentary for a specific questionnaire item.</summary>
    [HttpPost("quiz/patient/submit")]
    public IActionResult SubmitPatientAnswer([FromBody] SubmitAnswerDto dto)
    {
        var item = _context.AssessmentItems.Find(dto.ItemID);
        if (item == null) return NotFound();

        item.PatientAnswer = dto.Answer;
        item.PatientComment = dto.Comment;
        item.AnsweredAt = DateTime.UtcNow;
        _context.SaveChanges();

        return Ok();
    }

    // POST: api/Question/quiz/patient/skip
    /// <summary>Marks an assessment item as skipped by the patient.</summary>
    [HttpPost("quiz/patient/skip")]
    public IActionResult SkipPatientQuestion([FromBody] SkipQuestionDto dto)
    {
        var item = _context.AssessmentItems.Find(dto.ItemID);
        if (item == null) return NotFound();

        item.SkippedByPatient = true; 
        item.AnsweredAt = DateTime.UtcNow;

        _context.SaveChanges();

        return Ok();
    }


    // GET: api/Question/quiz/patient/progress/{assessmentId}/{questionId}
    /// <summary>Returns the current step index and progress metrics for the patient wizard.</summary>
    [HttpGet("quiz/patient/progress/{assessmentId}/{questionId}")]
    public IActionResult GetPatientQuestionProgress(int assessmentId, int questionId)
    {
        var allItems = _context.AssessmentItems
            .Where(i => i.AssessmentID == assessmentId)
            .Include(i => i.Question)
            .OrderBy(i => i.QuestionID)
            .ToList();

        var index = allItems.FindIndex(i => i.QuestionID == questionId);
        if (index == -1) return NotFound();

        var item = allItems[index];

        return Ok(new
        {
            QuestionNumber = index + 1,
            TotalQuestions = allItems.Count,
            QuestionText = item.Question?.QuestionText,
            Category = item.Question?.Category
        });
    }

    [HttpGet("quiz/patient/previous/{assessmentId}/{currentOrder}")]
    public async Task<ActionResult<QuestionDto>> GetPreviousQuestion(int assessmentId, int currentOrder)
    {
        if (currentOrder <= 0)
            return NotFound("Ingen tidigare fråga.");

        var item = await _context.AssessmentItems
            .Include(i => i.Question)
            .Where(i => i.AssessmentID == assessmentId && i.Order == currentOrder - 1)
            .FirstOrDefaultAsync();

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
            QuestionText = item.Question.QuestionText ?? "",
            Category = item.Question.Category ?? "",
            IsActive = item.Question.IsActive,
            Order = item.Order,
            Total = total,
            ScaleType = assessment?.ScaleType ?? "Numerisk",
            PatientAnswer = item.PatientAnswer, 
            PatientComment = item.PatientComment 
        });
    }


    // --------------------------
    // [QUIZ – STAFF]
    // --------------------------

    // GET: api/Question/quiz/staff/next/{assessmentId}
    /// <summary>Retrieves the next unanswered question for the healthcare professional's review wizard.</summary>
    [HttpGet("quiz/staff/next/{assessmentId}")]
    public async Task<ActionResult<StaffQuestionDto>> GetNextUnansweredStaffQuestion(int assessmentId)
    {
        Console.WriteLine($"[DEBUG] Staff next frågehämtning för assessment {assessmentId}");

        var item = await _context.AssessmentItems
            .Include(ai => ai.Question)
            .Where(ai =>
                ai.AssessmentID == assessmentId &&
                ai.StaffAnswer == null) 
            .OrderBy(ai => ai.Order)
            .FirstOrDefaultAsync();

        if (item == null)
        {
            Console.WriteLine("[DEBUG] Alla frågor besvarade av personalen");
            return NotFound(new { message = "Alla frågor är besvarade." });
        }

        var total = await _context.AssessmentItems.CountAsync(ai => ai.AssessmentID == assessmentId);
        var assessment = await _context.Assessments.FirstOrDefaultAsync(a => a.AssessmentID == assessmentId);

        if (assessment == null)
            return NotFound(new { message = "Bedömning hittades inte." });

        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == assessment.UserId);
        if (user == null)
            return NotFound(new { message = "Patienten hittades inte." });

        return Ok(new StaffQuestionDto
        {
            ItemID = item.ItemID,
            AssessmentID = item.AssessmentID,
            QuestionID = item.QuestionID,
            QuestionText = item.Question?.QuestionText ?? "Frågetext saknas",
            Category = item.Question?.Category ?? "Okänd",
            Order = item.Order,
            Total = total,
            ScaleType = assessment.ScaleType ?? "Numerisk",
            PatientAnswer = item.PatientAnswer,
            PatientComment = item.PatientComment,
            StaffAnswer = item.StaffAnswer,
            StaffComment = item.StaffComment,
            Flag = item.Flag,
            UserID = assessment.UserId,
            PatientName = user.Username
        });
    }



    // GET: api/Question/quiz/staff/previous/{assessmentId}/{currentOrder}
    /// <summary>Retrieves the previous assessment item for clinical review navigation.</summary>
    [HttpGet("quiz/staff/previous/{assessmentId}/{currentOrder}")]
    public async Task<ActionResult<StaffQuestionDto>> GetPreviousStaffQuestion(int assessmentId, int currentOrder)
    {
        if (currentOrder <= 0)
            return NotFound("Ingen tidigare fråga.");

        var item = await _context.AssessmentItems
            .Include(i => i.Question)
            .Where(i => i.AssessmentID == assessmentId && i.Order == currentOrder - 1)
            .FirstOrDefaultAsync();

        if (item == null || item.Question == null)
            return NotFound();

        var total = await _context.AssessmentItems.CountAsync(i => i.AssessmentID == assessmentId);
        var assessment = await _context.Assessments.FirstOrDefaultAsync(a => a.AssessmentID == assessmentId);

        return Ok(new StaffQuestionDto
        {
            ItemID = item.ItemID,
            AssessmentID = item.AssessmentID,
            QuestionID = item.Question.QuestionID,
            QuestionText = item.Question.QuestionText ?? "",
            Category = item.Question.Category ?? "",
            Order = item.Order,
            Total = total,
            ScaleType = assessment?.ScaleType ?? "Numerisk",
            PatientAnswer = item.PatientAnswer,
            PatientComment = item.PatientComment,
            StaffAnswer = item.StaffAnswer,
            StaffComment = item.StaffComment,
            Flag = item.Flag,
            UserID = assessment?.UserId ?? 0
        });
    }




    // POST: api/Question/quiz/staff/submit
    /// <summary>Saves the staff evaluation response, clinical commentary, and risk flags for an item.</summary>
    [HttpPost("quiz/staff/submit")]
    public IActionResult SubmitStaffAnswer([FromBody] SubmitStaffAnswerDto dto)
    {
        var item = _context.AssessmentItems.Find(dto.ItemID);
        if (item == null) return NotFound();

        item.StaffAnswer = dto.Answer;
        item.StaffComment = dto.Comment;
        item.Flag = dto.Flag ?? false;
        item.AnsweredAt = DateTime.UtcNow;

        // 🔽 Extract assessment ID for subsequent aggregate completion checks
        var assessmentId = item.AssessmentID;

        _context.SaveChanges();

        // 🔍 Verify if all questions have been addressed by staff – keeping finalization strictly manual via explicit sign-off
        var allAnswered = _context.AssessmentItems
            .Where(i => i.AssessmentID == assessmentId)
            .All(i => i.StaffAnswer.HasValue);

        if (allAnswered)
        {
            Console.WriteLine($"[DEBUG] Alla frågor för assessment {assessmentId} är besvarade av personal – redo för manuell bekräftelse.");
        }

        return Ok();
    }

    // GET: api/Question/quiz/staff/progress/{assessmentId}/{questionId}
    /// <summary>Returns the current step index and clinical comparison metrics for the staff wizard.</summary>
    [HttpGet("quiz/staff/progress/{assessmentId}/{questionId}")]
    public IActionResult GetStaffQuestionProgress(int assessmentId, int questionId)
    {
        var allItems = _context.AssessmentItems
            .Where(i => i.AssessmentID == assessmentId)
            .Include(i => i.Question)
            .OrderBy(i => i.QuestionID)
            .ToList();

        var index = allItems.FindIndex(i => i.QuestionID == questionId);
        if (index == -1) return NotFound();

        var item = allItems[index];

        return Ok(new
        {
            QuestionNumber = index + 1,
            TotalQuestions = allItems.Count,
            QuestionText = item.Question?.QuestionText,
            Category = item.Question?.Category,
            PatientAnswer = item.PatientAnswer,
            PatientComment = item.PatientComment,
            StaffComment = item.StaffComment,
            Flag = item.Flag
        });
    }
}