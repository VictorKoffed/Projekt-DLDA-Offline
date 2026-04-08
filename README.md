# 🌐 Projekt DLDA Offline (MVC + API)

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-brightgreen)](#-arkitektur)
[![Entity Framework](https://img.shields.io/badge/EF%20Core-SQL%20Server-blue)](#-databashantering)
[![Säkerhet](https://img.shields.io/badge/Security-RBAC%20%26%20BCrypt-red)](#-användarroller--säkerhet)

## 📝 Introduktion
Detta projekt är en digitalisering av det psykiatriska skattningsverktyget **DLDA**. Verktyget är utformat för att hjälpa patienter och vårdpersonal att utvärdera vardagsfunktioner genom strukturerade frågor inom områden som hygien, hälsa och dagliga aktiviteter.

### Syfte och Begränsningar
Projektet fungerar som en prototyp för att demonstrera hur en övergång från pappersbaserade verktyg till en digital lösning kan effektivisera vården. Målet är att visa hur digitaliseringen kan ge vårdpersonal bättre förutsättningar att assistera patienter och förstärka deras utveckling genom tydligare uppföljning och datavisualisering.

---

## Innehåll
- [Projektstruktur](#-projektstruktur)
- [Mappstruktur](#-mappstruktur)
- [Kom igång (Build & Run)](#-kom-igång-build--run)
- [Användarroller & Säkerhet](#-användarroller--säkerhet)
- [Funktioner](#-funktioner)
- [Arkitektur](#-arkitektur)
- [Tekniska koncept som används](#-tekniska-koncept-som-används)
- [Katalog över viktiga filer](#-katalog-över-viktiga-filer)

---

## 📁 Projektstruktur

Lösningen är uppdelad i två samverkande huvudprojekt för att främja Separation of Concerns:

| Projekt | Typ | Syfte |
|:---|:---|:---|
| **DLDA.API** | Web API | Hanterar affärslogik, databasåtkomst via EF Core och säker autentisering. |
| **DLDA.GUI** | MVC Web App | Webbgränssnitt som konsumerar API:et och hanterar användarsessioner. |

---

## 🧱 Mappstruktur

```text
Projekt-DLDA-Offline/
├─ DLDA.API/
│  ├─ Controllers/             # API-endpoints (Assessment, Auth, User, m.fl.)
│  ├─ Data/                    # AppDbContext för SQL Server-koppling
│  ├─ DTOs/                    # Data Transfer Objects för säker dataöverföring
│  └─ Models/                  # Datamodeller (User, Question, Assessment)
├─ DLDA.GUI/
│  ├─ Authorization/           # RoleAuthorizeAttribute för RBAC-logik
│  ├─ Controllers/             # MVC-logik för Patient, Staff och Admin
│  ├─ Services/                # API-klienter (AccountService, QuizService, etc.)
│  ├─ Views/                   # Razor-vyer (HTML/C#)
│  └─ wwwroot/                 # CSS, JS och Bilder
└─ Projekt DLDA Offline.sln    # Solution-fil

```

---

## 🚀 Kom igång (Build & Run)

### Förutsättningar
- .NET 8 SDK  
- SQL Server  
- Visual Studio 2022 (rekommenderas)  

### Installera och kör

1. Klona arkivet till din lokala maskin  
2. Öppna lösningen i Visual Studio:  
   `Projekt DLDA Offline.sln`  
3. Kontrollera anslutningssträngen i:  
   `DLDA.API/appsettings.json`  
4. Högerklicka på solution → **Set Startup Projects**  
5. Välj:
   - Multiple startup projects  
   - Starta både **DLDA.API** och **DLDA.GUI**  
6. Tryck **F5**

👉 Applikationen startar på inloggningssidan.

---

## 🔐 Användarroller & Säkerhet

Projektet implementerar ett robust säkerhetstänk för att skydda känslig data:

- **RBAC (Role-Based Access Control)**  
  En anpassad `RoleAuthorizeAttribute` styr åtkomst till:
  - Admin  
  - Staff  
  - Patient  

- **Lösenordssäkerhet**  
  Använder **BCrypt.Net** för hashing och verifiering  

- **Session State**  
  Användar-ID och roll lagras säkert i session  

- **CORS-policy**  
  API:et tillåter endast anrop från GUI-applikationen  

---

## ⚙️ Funktioner

- **Inloggningssystem** – Säker autentisering och rollhantering  
- **Digitala formulär** – Interaktiva skattningsformulär (quiz)  
- **Statistik & uppföljning** – Visualisering av förändring över tid  
- **Administrationspanel** – Hantering av användare och frågor (CRUD)  
- **Responsiv design** – Fungerar på både mobil och desktop  

---

## 🧱 Arkitektur

- **Separerad frontend/backend** – MVC konsumerar API  
- **IHttpClientFactory** – Effektiv hantering av HTTP-anrop  
- **Session-baserad autentisering** – Enkel men säker lösning för prototyp  

---

## 🧩 Tekniska koncept som används

| Område | Implementation | Förklaring |
|:---|:---|:---|
| **Security** | RBAC & BCrypt | Hanterar roller och säkra lösenord |
| **API Communication** | HttpClient / Async | Icke-blockerande anrop till API |
| **Data Access** | EF Core / SQL Server | ORM för databasinteraktion |
| **Architecture** | MVC + API | Tydlig separation mellan lager |

---

## 📚 Katalog över viktiga filer

<details>
<summary><strong>Kärnfiler</strong></summary>

- `Program.cs` – Konfiguration av middleware och services  
- `AppDbContext.cs` – Databasstruktur via EF Core  
- `appsettings.json` – Konfiguration (t.ex. connection strings)  

</details>

<details>
<summary><strong>Logik & Controllers</strong></summary>

- `AuthController` – Hanterar inloggning  
- `AssessmentController` – Hanterar formulärdata  
- `UserController` – CRUD för användare  

</details>

---

## 👥 Projektgrupp & Kontext

Detta projekt utvecklades som en del av kursen:

**Projektarbete och projektmetodik (7,5 hp)**  
(*Work and Project Methodology, 7.5 credits*)

Projektet genomfördes i en grupp om sju studenter med fokus på att tillämpa projektmetodik i praktiken, där vi tillsammans planerade, genomförde och levererade en fungerande prototyp.

### 🎯 Fokus i projektet

Arbetet inkluderade:

- Planering och strukturering av projekt (projektdirektiv, tidsplan)  
- Riskanalys och hantering av potentiella problem  
- Samarbete i grupp mot gemensamma mål  
- Löpande uppföljning och iteration av lösningen  
- Dokumentation, presentation och slutleverans  

### 🧠 Lärandeperspektiv

Projektet gav praktisk erfarenhet inom:

- Projektarbete som arbetsform inom IT  
- Gruppdynamik och samarbete  
- Problemlösning i komplexa system  
- Koppling mellan teknisk utveckling och verksamhetsbehov  

---

## 🤖 AI-assistans och kodgenerering

Delar av denna kodbas har utvecklats med stöd av AI-verktyg.

### Verktyg som använts
- ChatGPT – arkitektur, struktur och dokumentation  
- Gemini – felsökning och förbättring av kod  

### Omfattning

AI har använts för:
- Strukturering av backend-arkitektur  
- Implementation av rollbaserad säkerhet  
- Refaktorisering och förbättrad kodläsbarhet  

### Mänsklig granskning

All AI-genererad kod har granskats, testats och validerats manuellt för att säkerställa korrekt funktion och uppfyllande av säkerhetskrav.
