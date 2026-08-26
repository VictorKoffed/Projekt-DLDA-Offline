using BCrypt.Net;
using DLDA.API.Data;
using DLDA.API.DTOs;
using DLDA.API.DTOs.Patient;
using DLDA.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Manages user accounts, administrative user administration, role assignments,
/// and clinical patient listings enriched with latest assessment metrics.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
        _context = context;
    }

    // --------------------------
    // [ADMIN] – User Definition Management
    // --------------------------

    // GET: api/User
    [HttpGet]
    public ActionResult<IEnumerable<UserDto>> GetUsers()
    {
        return _context.Users
            .Select(u => new UserDto
            {
                UserID = u.UserID,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role
            }).ToList();
    }

    // GET: api/User/patients?search=anna
    // Retrieves all patients, applying optional partial name filtering if a search query is specified.
    [HttpGet("patients")]
    public ActionResult<IEnumerable<UserDto>> GetPatients([FromQuery] string? search)
    {
        var query = _context.Users
            .Where(u => u.Role.ToLower() == "patient");

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u => u.Username.ToLower().Contains(search.ToLower()));
        }

        return query
            .Select(u => new UserDto
            {
                UserID = u.UserID,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role
            }).ToList();
    }

    // GET: api/User/5
    // Retrieves a specific user profile by its primary key identifier.
    [HttpGet("{id}")]
    public ActionResult<UserDto> GetUser(int id)
    {
        var user = _context.Users.Find(id);
        if (user == null) return NotFound();

        return new UserDto
        {
            UserID = user.UserID,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role
        };
    }

    // POST: api/User
    // Instantiates a new user account with secure password hashing and fallback defaults.
    [HttpPost]
    public IActionResult CreateUser(UserDto dto)
    {
        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password ?? "password"), // Fallback to "password" default if no explicit password payload is provided.
            Role = dto.Role,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        return CreatedAtAction(nameof(GetUser), new { id = user.UserID }, new UserDto
        {
            UserID = user.UserID,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role
        });
    }

    // PUT: api/User/5
    // Updates user profile details and conditionally re-hashes passwords if modified.
    [HttpPut("{id}")]
    public IActionResult UpdateUser(int id, UserDto dto)
    {
        if (id != dto.UserID) return BadRequest();

        var user = _context.Users.Find(id);
        if (user == null) return NotFound();

        user.Username = dto.Username;
        user.Email = dto.Email;
        user.Role = dto.Role;

        // Conditionally update password hash if a new non-empty password is supplied
        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        }

        _context.SaveChanges();
        return NoContent();
    }

    // DELETE: api/User/5
    // Deletes a user profile from the database.
    [HttpDelete("{id}")]
    public IActionResult DeleteUser(int id)
    {
        var user = _context.Users.Find(id);
        if (user == null) return NotFound();

        _context.Users.Remove(user);
        _context.SaveChanges();
        return NoContent();
    }

    // --------------------------
    // [STAFF] – Clinical Patient Directory Listing
    // --------------------------

    // GET api/user/5
    // Exposes patient usernames paired with their most recent assessment status for clinical dashboards.
    [HttpGet("with-latest-assessment")]
    public async Task<ActionResult<IEnumerable<PatientWithAssessmentStatusDto>>> GetUsersWithLatestAssessment(
    [FromQuery] string? search,
    [FromQuery] bool? ongoing,
    [FromQuery] bool? notOngoing,
    [FromQuery] string? recent)
    {
        var patients = await _context.Users
            .Where(u => u.Role.ToLower() == "patient" &&
                        (string.IsNullOrWhiteSpace(search) || u.Username.ToLower().Contains(search.ToLower())))
            .Select(u => new PatientWithAssessmentStatusDto
            {
                UserID = u.UserID,
                Username = u.Username,
                LastAssessment = u.Assessments
                    .OrderByDescending(a => a.CreatedAt)
                    .Select(a => new AssessmentDto
                    {
                        AssessmentID = a.AssessmentID,
                        UserId = a.UserId,
                        CreatedAt = a.CreatedAt ?? DateTime.MinValue,
                        IsComplete = a.IsComplete,
                        IsStaffComplete = a.IsStaffComplete,
                        ScaleType = a.ScaleType,
                        HasStarted = a.AssessmentItems.Any(i => i.PatientAnswer != null || i.SkippedByPatient),
                        AnsweredCount = a.AssessmentItems.Count(i => i.PatientAnswer != null || i.SkippedByPatient),
                        TotalQuestions = a.AssessmentItems.Count()
                    })
                    .FirstOrDefault()
            })
            .ToListAsync();

        var filtered = patients.Where(p =>
        {
            var a = p.LastAssessment;

            // 🟡 Edge case: Patient has no assessment history
            if (a == null)
            {
                // Display only if 'not ongoing' filter is explicitly checked and ' ongoing' is not
                if (notOngoing == true && ongoing != true)
                    return true;

                // Otherwise, exclude unassessed patients if any other status filters are active
                return ongoing != true && notOngoing != true && string.IsNullOrWhiteSpace(recent);
            }

            // If both ongoing and not-ongoing filters are toggled, bypass status filters to show all assessed patients
            if (ongoing == true && notOngoing == true)
                return true;

            // Restrict to active ongoing assessments only
            if (ongoing == true && notOngoing != true && a.IsComplete)
                return false;

            // Restrict to completed assessments (❗ Ensures uncompleted forms are filtered out when looking for finished states)
            if (notOngoing == true && ongoing != true && !a.IsComplete)
                return false;

            // Time window filters based on assessment creation date
            if (recent == "week" && a.CreatedAt < DateTime.Today.AddDays(-7))
                return false;

            if (recent == "month" && a.CreatedAt < DateTime.Today.AddMonths(-1))
                return false;

            if (recent == "older" && a.CreatedAt >= DateTime.Today.AddMonths(-1))
                return false;

            return true;
        });
        return Ok(filtered);
    }
}