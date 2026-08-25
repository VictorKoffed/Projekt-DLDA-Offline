using Microsoft.AspNetCore.Mvc;
using DLDA.API.Data;
using DLDA.API.DTOs;
using DLDA.API.Models;
using BCrypt.Net;

namespace DLDA.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    // ==========================================
    // 1. Vanlig Inloggning
    // ==========================================
    [HttpPost("login")]
    public ActionResult<AuthResponseDto> Login(LoginDto dto)
    {
        var user = _context.Users.FirstOrDefault(u => u.Username == dto.Username);

        if (user == null)
        {
            Console.WriteLine("❌ Ingen användare hittades.");
            return Unauthorized("Felaktigt användarnamn eller lösenord.");
        }

        var valid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
        Console.WriteLine($"🔐 Kontroll: lösenord matchar? {valid}");

        if (!valid)
            return Unauthorized("Felaktigt användarnamn eller lösenord.");

        return Ok(new AuthResponseDto
        {
            UserID = user.UserID,
            Username = user.Username,
            Role = user.Role
        });
    }

    // ==========================================
    // 2. DEV-VERKTYG: Skapa/Uppdatera Admin
    // ==========================================
    [HttpPost("dev-update-admin")]
    public IActionResult DevUpdateAdmin()
    {
        var existing = _context.Users.FirstOrDefault(u => u.Username == "admin");
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("password");

        if (existing != null)
        {
            existing.PasswordHash = passwordHash;
            existing.Role = "admin";
            existing.Email = "admin@gmail.com";
            existing.CreatedAt = DateTime.Now;
        }
        else
        {
            _context.Users.Add(new User
            {
                Username = "admin",
                Email = "admin@gmail.com",
                PasswordHash = passwordHash,
                Role = "admin",
                CreatedAt = DateTime.Now
            });
        }

        _context.SaveChanges();

        // Valfritt: rensa dubbletter
        var duplicates = _context.Users
            .Where(u => u.Username == "admin")
            .OrderBy(u => u.UserID)
            .Skip(1)
            .ToList();

        if (duplicates.Any())
        {
            _context.Users.RemoveRange(duplicates);
            _context.SaveChanges();
        }

        return Ok("✅ Admin uppdaterad/skapad.");
    }

    // ==========================================
    // 3. DEV-VERKTYG: Seeda DLDA-frågor
    // ==========================================
    [HttpPost("dev-seed-questions")]
    public IActionResult SeedQuestions()
    {
        if (_context.Questions.Any())
        {
            return Ok("✅ Frågor finns redan i databasen. Ingen åtgärd krävs.");
        }

        var questions = new List<Question>
        {
            // 1. Lärande och att tillämpa kunskap
            new Question { Category = "1. Lärande och att tillämpa kunskap", QuestionText = "Hur bedömer du personens förmåga att se?" },
            new Question { Category = "1. Lärande och att tillämpa kunskap", QuestionText = "Hur bedömer du personens förmåga att höra?" },
            new Question { Category = "1. Lärande och att tillämpa kunskap", QuestionText = "Hur bedömer du personens förmåga att läsa?" },
            new Question { Category = "1. Lärande och att tillämpa kunskap", QuestionText = "Hur bedömer du personens förmåga att skriva?" },
            new Question { Category = "1. Lärande och att tillämpa kunskap", QuestionText = "Hur bedömer du personens förmåga att räkna?" },
            new Question { Category = "1. Lärande och att tillämpa kunskap", QuestionText = "Hur bedömer du personens förmåga att lära sig nya saker?" },
            new Question { Category = "1. Lärande och att tillämpa kunskap", QuestionText = "Hur bedömer du personens koncentrationsförmåga?" },
            new Question { Category = "1. Lärande och att tillämpa kunskap", QuestionText = "Hur bedömer du personens förmåga att lösa problem i vardagen?" },

            // 2. Allmänna krav i vardagen
            new Question { Category = "2. Allmänna krav i vardagen", QuestionText = "Hur bedömer du personens förmåga att göra vardagliga sysslor?" },
            new Question { Category = "2. Allmänna krav i vardagen", QuestionText = "Hur bedömer du personens förmåga att arbeta i grupp?" },
            new Question { Category = "2. Allmänna krav i vardagen", QuestionText = "Hur bedömer du personens förmåga att hantera stress?" },

            // 3. Kommunikation
            new Question { Category = "3. Kommunikation", QuestionText = "Hur bedömer du personens förmåga att prata med andra?" },
            new Question { Category = "3. Kommunikation", QuestionText = "Hur bedömer du personens förmåga att kommunicera med andra genom att skriva?" },
            new Question { Category = "3. Kommunikation", QuestionText = "Hur bedömer du personens förmåga att använda telefon genom att ringa?" },
            new Question { Category = "3. Kommunikation", QuestionText = "Hur bedömer du personens förmåga att använda appar?" },
            new Question { Category = "3. Kommunikation", QuestionText = "Hur bedömer du personens förmåga att använda dator?" },

            // 4. Förflyttning
            new Question { Category = "4. Förflyttning", QuestionText = "Hur bedömer du personens förmåga att gå och förflytta sig mellan olika platser?" },
            new Question { Category = "4. Förflyttning", QuestionText = "Hur bedömer du personens förmåga att använda transportmedel som passagerare? (t ex. bil, buss, taxi, färdtjänst)" },

            // 5a. Personlig vård
            new Question { Category = "5a. Personlig vård", QuestionText = "Hur bedömer du personens förmåga att sköta sin hygien?" },
            new Question { Category = "5a. Personlig vård", QuestionText = "Hur bedömer du personens förmåga att sköta sin tandvård?" },
            new Question { Category = "5a. Personlig vård", QuestionText = "Hur bedömer du personens förmåga att sköta sina läkemedel?" },
            new Question { Category = "5a. Personlig vård", QuestionText = "Hur bedömer du personens förmåga att sköta sin klädsel?" },
            new Question { Category = "5a. Personlig vård", QuestionText = "Hur bedömer du personens förmåga att tillgodose sitt behov av motion?" },
            new Question { Category = "5a. Personlig vård", QuestionText = "Hur bedömer du personens förmåga att tillgodose sitt behov av sömn?" },

            // 5b. Substansbruk
            new Question { Category = "5b. Substansbruk", QuestionText = "Hur bedömer du personens användande av tobak?" },
            new Question { Category = "5b. Substansbruk", QuestionText = "Hur bedömer du personens användande av alkohol?" },
            new Question { Category = "5b. Substansbruk", QuestionText = "Hur bedömer du personens användande av droger?" },

            // 6. Hemliv
            new Question { Category = "6. Hemliv", QuestionText = "Hur bedömer du personens förmåga att planera och laga måltider?" },
            new Question { Category = "6. Hemliv", QuestionText = "Hur bedömer du personens förmåga att sköta sin bostad?" },

            // 7. Mellanmänskliga relationer
            new Question { Category = "7. Mellanmänskliga relationer", QuestionText = "Hur bedömer du personens förmåga till kontakter med vänner, grannar, bekanta?" },
            new Question { Category = "7. Mellanmänskliga relationer", QuestionText = "Hur bedömer du personens förmåga till kontakter med arbetsgivare, vård och sociala myndigheter?" },
            new Question { Category = "7. Mellanmänskliga relationer", QuestionText = "Hur bedömer du personens förmåga till familjerelationer? (t ex. föräldrar, barn, syskon, släkt)" },
            new Question { Category = "7. Mellanmänskliga relationer", QuestionText = "Hur bedömer du personens förmåga till känslomässiga relationer? (t ex. personliga, romantiska, äktenskapliga, sexuella)" },

            // 8. Viktiga livsområden
            new Question { Category = "8. Viktiga livsområden", QuestionText = "Hur bedömer du personens förmåga att genomföra studier?" },
            new Question { Category = "8. Viktiga livsområden", QuestionText = "Hur bedömer du personens förmåga att arbeta?" },
            new Question { Category = "8. Viktiga livsområden", QuestionText = "Hur bedömer du personens förmåga att sköta sin ekonomi?" },

            // 9. Samhällsgemenskap
            new Question { Category = "9. Samhällsgemenskap", QuestionText = "Hur bedömer du personens förmåga att delta i aktiviteter på fritiden?" },
            new Question { Category = "9. Samhällsgemenskap", QuestionText = "Hur bedömer du personens förmåga att tillfredsställa andliga behov? (t ex. känna välbefinnande genom: tro, religion, fridfulla naturupplevelser etc.)" }
        };

        _context.Questions.AddRange(questions);
        _context.SaveChanges();

        return Ok($"✅ {questions.Count} DLDA-frågor har skapats i databasen.");
    }

    // ==========================================
    // 4. DEV-VERKTYG: Seeda testanvändare
    // ==========================================
    [HttpPost("dev-seed-users")]
    public IActionResult SeedUsers()
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("password");
        int usersAdded = 0;

        // 1. Skapa en test-patient
        if (!_context.Users.Any(u => u.Username == "patient"))
        {
            _context.Users.Add(new User
            {
                Username = "patient",
                Email = "patient@test.com",
                PasswordHash = passwordHash,
                Role = "patient",
                CreatedAt = DateTime.Now
            });
            usersAdded++;
        }

        // 2. Skapa en test-personal (staff)
        if (!_context.Users.Any(u => u.Username == "staff"))
        {
            _context.Users.Add(new User
            {
                Username = "staff",
                Email = "staff@test.com",
                PasswordHash = passwordHash,
                Role = "staff",
                CreatedAt = DateTime.Now
            });
            usersAdded++;
        }

        if (usersAdded > 0)
        {
            _context.SaveChanges();
            return Ok($"✅ {usersAdded} testanvändare (patient och staff) har skapats. Lösenordet för båda är 'password'.");
        }

        return Ok("✅ Testanvändare (patient/staff) fanns redan i databasen.");
    }

    // ==========================================
    // 5. DEV-VERKTYG: Skapa realistisk mock-bedömning
    // ==========================================
    [HttpPost("dev-seed-mock-assessment")]
    public IActionResult SeedMockAssessment()
    {
        var patient = _context.Users.FirstOrDefault(u => u.Username == "patient");
        var questions = _context.Questions.OrderBy(q => q.QuestionID).ToList();

        if (patient == null || !questions.Any())
            return BadRequest("❌ Se till att köra dev-seed-users och dev-seed-questions först!");

        // 1. Skapa själva bedömningen (Assessment)
        var assessment = new Assessment
        {
            UserId = patient.UserID,
            ScaleType = "DLDA",
            IsComplete = true,
            IsStaffComplete = true,
            CreatedAt = DateTime.UtcNow.AddDays(-2), // Låtsas att den gjordes för 2 dagar sen
            UpdatedAt = DateTime.UtcNow.AddDays(-2)
        };
        
        _context.Assessments.Add(assessment);
        _context.SaveChanges(); // Sparar för att få ett AssessmentID

        // 2. Loopa igenom frågorna och skapa svar (AssessmentItems)
        var items = new List<AssessmentItem>();
        int order = 1;
        var random = new Random(42); // Samma "slump" varje gång så koden är förutsägbar

        foreach (var q in questions)
        {
            var item = new AssessmentItem
            {
                AssessmentID = assessment.AssessmentID, 
                QuestionID = q.QuestionID,
                Order = order++,
                SkippedByPatient = false,
                Flag = false,
                AnsweredAt = assessment.CreatedAt.Value.AddMinutes(order * 2) // Låtsas att det tog 2 min per fråga
            };

            // --- REALISTISKT SCENARIO: Patient med social ångest & sömnproblem ---

            if (q.Category != null && (q.Category.Contains("Mellanmänskliga") || q.Category.Contains("Samhällsgemenskap")))
            {
                // Patienten tycker sociala situationer är extremt jobbiga (4).
                // Personalen märker det, men skattar det aningen mildare (3).
                item.PatientAnswer = 4; 
                item.StaffAnswer = 3;
                item.StaffComment = "Patienten undviker gemensamma utrymmen.";
            }
            else if (q.QuestionText != null && q.QuestionText.Contains("sömn"))
            {
                // Båda är överens om grava sömnproblem
                item.PatientAnswer = 4;
                item.StaffAnswer = 4;
                item.PatientComment = "Kan inte sova alls på nätterna.";
            }
            else if (q.Category != null && q.Category.Contains("Substansbruk"))
            {
                // EDGE CASE: Patienten vill inte svara (hoppar över). 
                // Personalen fyller i att det finns lätta problem (1) och flaggar för samtal!
                item.SkippedByPatient = true;
                item.PatientAnswer = null;
                item.StaffAnswer = 1; 
                item.Flag = true; 
                item.StaffComment = "Vill inte prata om alkoholvanor, bör tas upp nästa vecka.";
            }
            else if (q.QuestionText != null && (q.QuestionText.Contains("hygien") || q.QuestionText.Contains("klädsel")))
            {
                // Båda överens om att detta fungerar felfritt
                item.PatientAnswer = 0;
                item.StaffAnswer = 0;
            }
            else
            {
                // Resterande frågor: Lätta till måttliga problem (1 eller 2).
                // Vi skapar en liten differens mellan patient och personal för att få en bra graf.
                item.PatientAnswer = random.Next(1, 3);
                item.StaffAnswer = item.PatientAnswer == 1 ? 2 : 1; 
            }

            items.Add(item);
        }

        _context.AssessmentItems.AddRange(items);
        _context.SaveChanges();

        return Ok($"✅ Realistisk mock-bedömning skapad! {items.Count} svar inlagda för patienten. Öppna graferna i GUI:t för att se resultatet.");
    }
}
