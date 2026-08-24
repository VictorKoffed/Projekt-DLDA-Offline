using Microsoft.AspNetCore.Mvc;
using DLDA.API.Data;
using DLDA.API.DTOs;
using DLDA.API.Models;
using BCrypt.Net;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    // POST: api/Auth/login
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

        return Ok("✅ Admin uppdaterad/skapat.");
    }

    [HttpPost("dev-seed-questions")]
public IActionResult SeedQuestions()
{
    if (_context.Questions.Any())
    {
        return Ok("Frågor finns redan i databasen. Ingen åtgärd krävs.");
    }

    var questions = new List<Question>
    {
        // 1. Lärande och att tillämpa kunskap
        new Question { Category = "1. Lärande och att tillämpa kunskap", Text = "Hur bedömer du personens förmåga att se?" },
        new Question { Category = "1. Lärande och att tillämpa kunskap", Text = "Hur bedömer du personens förmåga att höra?" },
        new Question { Category = "1. Lärande och att tillämpa kunskap", Text = "Hur bedömer du personens förmåga att läsa?" },
        new Question { Category = "1. Lärande och att tillämpa kunskap", Text = "Hur bedömer du personens förmåga att skriva?" },
        new Question { Category = "1. Lärande och att tillämpa kunskap", Text = "Hur bedömer du personens förmåga att räkna?" },
        new Question { Category = "1. Lärande och att tillämpa kunskap", Text = "Hur bedömer du personens förmåga att lära sig nya saker?" },
        new Question { Category = "1. Lärande och att tillämpa kunskap", Text = "Hur bedömer du personens koncentrationsförmåga?" },
        new Question { Category = "1. Lärande och att tillämpa kunskap", Text = "Hur bedömer du personens förmåga att lösa problem i vardagen?" },

        // 2. Allmänna krav i vardagen
        new Question { Category = "2. Allmänna krav i vardagen", Text = "Hur bedömer du personens förmåga att göra vardagliga sysslor?" },
        new Question { Category = "2. Allmänna krav i vardagen", Text = "Hur bedömer du personens förmåga att arbeta i grupp?" },
        new Question { Category = "2. Allmänna krav i vardagen", Text = "Hur bedömer du personens förmåga att hantera stress?" },

        // 3. Kommunikation
        new Question { Category = "3. Kommunikation", Text = "Hur bedömer du personens förmåga att prata med andra?" },
        new Question { Category = "3. Kommunikation", Text = "Hur bedömer du personens förmåga att kommunicera med andra genom att skriva?" },
        new Question { Category = "3. Kommunikation", Text = "Hur bedömer du personens förmåga att använda telefon genom att ringa?" },
        new Question { Category = "3. Kommunikation", Text = "Hur bedömer du personens förmåga att använda appar?" },
        new Question { Category = "3. Kommunikation", Text = "Hur bedömer du personens förmåga att använda dator?" },

        // 4. Förflyttning
        new Question { Category = "4. Förflyttning", Text = "Hur bedömer du personens förmåga att gå och förflytta sig mellan olika platser?" },
        new Question { Category = "4. Förflyttning", Text = "Hur bedömer du personens förmåga att använda transportmedel som passagerare? (t ex. bil, buss, taxi, färdtjänst)" },

        // 5a. Personlig vård
        new Question { Category = "5a. Personlig vård", Text = "Hur bedömer du personens förmåga att sköta sin hygien?" },
        new Question { Category = "5a. Personlig vård", Text = "Hur bedömer du personens förmåga att sköta sin tandvård?" },
        new Question { Category = "5a. Personlig vård", Text = "Hur bedömer du personens förmåga att sköta sina läkemedel?" },
        new Question { Category = "5a. Personlig vård", Text = "Hur bedömer du personens förmåga att sköta sin klädsel?" },
        new Question { Category = "5a. Personlig vård", Text = "Hur bedömer du personens förmåga att tillgodose sitt behov av motion?" },
        new Question { Category = "5a. Personlig vård", Text = "Hur bedömer du personens förmåga att tillgodose sitt behov av sömn?" },

        // 5b. Substansbruk
        new Question { Category = "5b. Substansbruk", Text = "Hur bedömer du personens användande av tobak?" },
        new Question { Category = "5b. Substansbruk", Text = "Hur bedömer du personens användande av alkohol?" },
        new Question { Category = "5b. Substansbruk", Text = "Hur bedömer du personens användande av droger?" },

        // 6. Hemliv
        new Question { Category = "6. Hemliv", Text = "Hur bedömer du personens förmåga att planera och laga måltider?" },
        new Question { Category = "6. Hemliv", Text = "Hur bedömer du personens förmåga att sköta sin bostad?" },

        // 7. Mellanmänskliga relationer
        new Question { Category = "7. Mellanmänskliga relationer", Text = "Hur bedömer du personens förmåga till kontakter med vänner, grannar, bekanta?" },
        new Question { Category = "7. Mellanmänskliga relationer", Text = "Hur bedömer du personens förmåga till kontakter med arbetsgivare, vård och sociala myndigheter?" },
        new Question { Category = "7. Mellanmänskliga relationer", Text = "Hur bedömer du personens förmåga till familjerelationer? (t ex. föräldrar, barn, syskon, släkt)" },
        new Question { Category = "7. Mellanmänskliga relationer", Text = "Hur bedömer du personens förmåga till känslomässiga relationer? (t ex. personliga, romantiska, äktenskapliga, sexuella)" },

        // 8. Viktiga livsområden
        new Question { Category = "8. Viktiga livsområden", Text = "Hur bedömer du personens förmåga att genomföra studier?" },
        new Question { Category = "8. Viktiga livsområden", Text = "Hur bedömer du personens förmåga att arbeta?" },
        new Question { Category = "8. Viktiga livsområden", Text = "Hur bedömer du personens förmåga att sköta sin ekonomi?" },

        // 9. Samhällsgemenskap
        new Question { Category = "9. Samhällsgemenskap", Text = "Hur bedömer du personens förmåga att delta i aktiviteter på fritiden?" },
        new Question { Category = "9. Samhällsgemenskap", Text = "Hur bedömer du personens förmåga att tillfredsställa andliga behov? (t ex. känna välbefinnande genom: tro, religion, fridfulla naturupplevelser etc.)" }
    };

    _context.Questions.AddRange(questions);
    _context.SaveChanges();

    return Ok($"✅ {questions.Count} DLDA-frågor har skapats i databasen.");
}

}
