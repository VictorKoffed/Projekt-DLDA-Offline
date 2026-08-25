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
            new Question { Category = "1. Lärande och att tillämpa kunskap", QuestionText = "Förmåga att se", IsActive = true },
            new Question { Category = "1. Lärande och att tillämpa kunskap", QuestionText = "Förmåga att höra", IsActive = true },
            new Question { Category = "1. Lärande och att tillämpa kunskap", QuestionText = "Förmåga att läsa", IsActive = true },
            new Question { Category = "1. Lärande och att tillämpa kunskap", QuestionText = "Förmåga att skriva", IsActive = true },
            new Question { Category = "1. Lärande och att tillämpa kunskap", QuestionText = "Förmåga att räkna", IsActive = true },
            new Question { Category = "1. Lärande och att tillämpa kunskap", QuestionText = "Förmåga att lära sig nya saker", IsActive = true },
            new Question { Category = "1. Lärande och att tillämpa kunskap", QuestionText = "Koncentrationsförmåga", IsActive = true },
            new Question { Category = "1. Lärande och att tillämpa kunskap", QuestionText = "Förmåga att lösa problem i vardagen", IsActive = true },

            // 2. Allmänna krav i vardagen
            new Question { Category = "2. Allmänna krav i vardagen", QuestionText = "Förmåga att göra vardagliga sysslor", IsActive = true },
            new Question { Category = "2. Allmänna krav i vardagen", QuestionText = "Förmåga att arbeta i grupp", IsActive = true },
            new Question { Category = "2. Allmänna krav i vardagen", QuestionText = "Förmåga att hantera stress", IsActive = true },

            // 3. Kommunikation
            new Question { Category = "3. Kommunikation", QuestionText = "Förmåga att prata med andra", IsActive = true },
            new Question { Category = "3. Kommunikation", QuestionText = "Förmåga att kommunicera med andra genom att skriva", IsActive = true },
            new Question { Category = "3. Kommunikation", QuestionText = "Förmåga att använda telefon genom att ringa", IsActive = true },
            new Question { Category = "3. Kommunikation", QuestionText = "Förmåga att använda appar", IsActive = true },
            new Question { Category = "3. Kommunikation", QuestionText = "Förmåga att använda dator", IsActive = true },

            // 4. Förflyttning
            new Question { Category = "4. Förflyttning", QuestionText = "Förmåga att gå och förflytta sig mellan olika platser", IsActive = true },
            new Question { Category = "4. Förflyttning", QuestionText = "Förmåga att använda transportmedel som passagerare (t.ex. bil, buss, taxi, färdtjänst)", IsActive = true },

            // 5a. Personlig vård
            new Question { Category = "5a. Personlig vård", QuestionText = "Förmåga att sköta sin hygien", IsActive = true },
            new Question { Category = "5a. Personlig vård", QuestionText = "Förmåga att sköta sin tandvård", IsActive = true },
            new Question { Category = "5a. Personlig vård", QuestionText = "Förmåga att sköta sina läkemedel", IsActive = true },
            new Question { Category = "5a. Personlig vård", QuestionText = "Förmåga att sköta sin klädsel", IsActive = true },
            new Question { Category = "5a. Personlig vård", QuestionText = "Förmåga att tillgodose sitt behov av motion", IsActive = true },
            new Question { Category = "5a. Personlig vård", QuestionText = "Förmåga att tillgodose sitt behov av sömn", IsActive = true },

            // 5b. Substansbruk
            new Question { Category = "5b. Substansbruk", QuestionText = "Användande av tobak", IsActive = true },
            new Question { Category = "5b. Substansbruk", QuestionText = "Användande av alkohol", IsActive = true },
            new Question { Category = "5b. Substansbruk", QuestionText = "Användande av droger", IsActive = true },

            // 6. Hemliv
            new Question { Category = "6. Hemliv", QuestionText = "Förmåga att planera och laga måltider", IsActive = true },
            new Question { Category = "6. Hemliv", QuestionText = "Förmåga att sköta sin bostad", IsActive = true },

            // 7. Mellanmänskliga relationer
            new Question { Category = "7. Mellanmänskliga relationer", QuestionText = "Förmåga till kontakter med vänner, grannar, bekanta", IsActive = true },
            new Question { Category = "7. Mellanmänskliga relationer", QuestionText = "Förmåga till kontakter med arbetsgivare, vård och sociala myndigheter", IsActive = true },
            new Question { Category = "7. Mellanmänskliga relationer", QuestionText = "Förmåga till familjerelationer (t.ex. föräldrar, barn, syskon, släkt)", IsActive = true },
            new Question { Category = "7. Mellanmänskliga relationer", QuestionText = "Förmåga till känslomässiga relationer (t.ex. personliga, romantiska, äktenskapliga, sexuella)", IsActive = true },

            // 8. Viktiga livsområden
            new Question { Category = "8. Viktiga livsområden", QuestionText = "Förmåga att genomföra studier", IsActive = true },
            new Question { Category = "8. Viktiga livsområden", QuestionText = "Förmåga att arbeta", IsActive = true },
            new Question { Category = "8. Viktiga livsområden", QuestionText = "Förmåga att sköta sin ekonomi", IsActive = true },

            // 9. Samhällsgemenskap
            new Question { Category = "9. Samhällsgemenskap", QuestionText = "Förmåga att delta i aktiviteter på fritiden", IsActive = true },
            new Question { Category = "9. Samhällsgemenskap", QuestionText = "Förmåga att tillfredsställa andliga behov (t.ex. känna välbefinnande genom tro, religion, fridfulla naturupplevelser etc.)", IsActive = true }
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
    // 5. DEV-VERKTYG: Skapa två realistiska mock-bedömningar (Tidslinje)
    // ==========================================
    [HttpPost("dev-seed-mock-assessment")]
    public IActionResult SeedMockAssessment()
    {
        var patient = _context.Users.FirstOrDefault(u => u.Username == "patient");
        var questions = _context.Questions.OrderBy(q => q.QuestionID).ToList();

        if (patient == null || !questions.Any())
            return BadRequest("❌ Se till att köra dev-seed-users och dev-seed-questions först!");

        var random = new Random(42); // Samma "slump" varje gång för stabila grafer

        // ---------------------------------------------------------
        // BEDÖMNING 1: För 30 dagar sedan 
        // ---------------------------------------------------------
        var oldAssessment = new Assessment
        {
            UserId = patient.UserID,
            ScaleType = "DLDA",
            IsComplete = true,
            IsStaffComplete = true,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow.AddDays(-30)
        };
        
        _context.Assessments.Add(oldAssessment);
        _context.SaveChanges(); 

        var oldItems = new List<AssessmentItem>();
        int order1 = 1;
        foreach (var q in questions)
        {
            var item = new AssessmentItem
            {
                AssessmentID = oldAssessment.AssessmentID, 
                QuestionID = q.QuestionID,
                Order = order1++,
                SkippedByPatient = false,
                Flag = false,
                AnsweredAt = oldAssessment.CreatedAt.Value.AddMinutes(order1 * 2)
            };

            if (q.Category != null && q.Category.Contains("Mellanmänskliga")) {
                item.PatientAnswer = 4; item.StaffAnswer = 4;
                item.StaffComment = "Patienten drar sig undan och isolerar sig mycket på rummet.";
            } else if (q.QuestionText != null && q.QuestionText.Contains("sömn")) {
                item.PatientAnswer = 4; item.StaffAnswer = 4;
                item.PatientComment = "Sover max 2 timmar per natt.";
            } else if (q.Category != null && q.Category.Contains("Substansbruk")) {
                item.SkippedByPatient = true; item.PatientAnswer = null; item.StaffAnswer = 2; item.Flag = true;
            } else if (q.Category != null && q.Category.Contains("Hemliv")) {
                // Hemlivet fungerade jättebra för en månad sen!
                item.PatientAnswer = 0; item.StaffAnswer = 0; 
            } else {
                item.PatientAnswer = random.Next(2, 5); 
                item.StaffAnswer = item.PatientAnswer == 4 ? 3 : item.PatientAnswer; // Små åsiktsskillnader
            }
            oldItems.Add(item);
        }
        _context.AssessmentItems.AddRange(oldItems);

        // ---------------------------------------------------------
        // BEDÖMNING 2: För 2 dagar sedan 
        // ---------------------------------------------------------
        var newAssessment = new Assessment
        {
            UserId = patient.UserID,
            ScaleType = "DLDA",
            IsComplete = true,
            IsStaffComplete = true,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow.AddDays(-2)
        };
        
        _context.Assessments.Add(newAssessment);
        _context.SaveChanges(); 

        var newItems = new List<AssessmentItem>();
        int order2 = 1;
        foreach (var q in questions)
        {
            var item = new AssessmentItem
            {
                AssessmentID = newAssessment.AssessmentID, 
                QuestionID = q.QuestionID,
                Order = order2++,
                SkippedByPatient = false,
                Flag = false,
                AnsweredAt = newAssessment.CreatedAt.Value.AddMinutes(order2 * 2)
            };

            if (q.Category != null && q.Category.Contains("Mellanmänskliga")) {
                item.PatientAnswer = 2; item.StaffAnswer = 2; // Tydlig FÖRBÄTTRING
                item.StaffComment = "Börjat sitta med i dagrummet korta stunder. Tydlig förbättring.";
            } else if (q.QuestionText != null && q.QuestionText.Contains("sömn")) {
                item.PatientAnswer = 1; item.StaffAnswer = 1; // Tydlig FÖRBÄTTRING
                item.PatientComment = "Melatoninet fungerar jättebra nu.";
            } else if (q.Category != null && q.Category.Contains("Substansbruk")) {
                item.PatientAnswer = 0; item.StaffAnswer = 0; item.Flag = false; 
            } else if (q.Category != null && q.Category.Contains("Hemliv")) {
                // Tydlig FÖRSÄMRING + ÅSIKTSSKILLNAD
                // Patienten tycker det funkar okej (2), personalen ser att det är kaos (4)
                item.PatientAnswer = 2; item.StaffAnswer = 4; 
                item.StaffComment = "Har slutat städa helt. Diskberget växer och patienten slänger sopor på golvet.";
                item.Flag = true;
            } else {
                item.PatientAnswer = random.Next(0, 3); 
                item.StaffAnswer = item.PatientAnswer == 2 ? 1 : item.PatientAnswer;
            }
            newItems.Add(item);
        }
        _context.AssessmentItems.AddRange(newItems);

        _context.SaveChanges();

        return Ok($"✅ Tidslinje skapad! Två bedömningar (en från en månad sedan, en från nyligen) är inlagda för patienten.");
    }
}
