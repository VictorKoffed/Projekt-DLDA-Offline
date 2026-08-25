# 🌐 Projekt DLDA Offline (MVC + API)

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/Docker-DevContainers-2496ED)](https://www.docker.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-brightgreen)](#-arkitektur)
[![Entity Framework](https://img.shields.io/badge/EF%20Core-SQL%20Server-blue)](#-tekniska-koncept-som-används)
[![Säkerhet](https://img.shields.io/badge/Security-RBAC%20%26%20BCrypt-red)](#-användarroller--säkerhet)

## 📝 Introduktion

Detta projekt är en digitalisering av det psykiatriska skattningsverktyget **DLDA**. Verktyget är utformat för att hjälpa patienter och vårdpersonal att utvärdera vardagsfunktioner genom strukturerade frågor inom områden som hygien, hälsa och dagliga aktiviteter.

### Syfte och Begränsningar

Projektet fungerar som en prototyp för att undersöka hur ett pappersbaserat skattningsverktyg kan digitaliseras. Målet är att göra skattningarna mer lättillgängliga och samtidigt ge vårdpersonal bättre möjligheter att följa upp resultat och förändringar över tid.

Projektet är utvecklat som en prototyp och ska därför inte betraktas som ett färdigt vårdsystem. Funktioner, säkerhet och datalagring är anpassade efter projektets tekniska och utbildningsmässiga förutsättningar.

---

## Innehåll

- [Projektstruktur](#-projektstruktur)
- [Mappstruktur](#-mappstruktur)
- [Kom igång (Build & Run)](#-kom-igång-build--run)
- [Användarroller & Säkerhet](#-användarroller--säkerhet)
- [Funktioner](#-funktioner)
- [Arkitektur](#-arkitektur)
- [Tekniska koncept som används](#-tekniska-koncept-som-används)
- [Projektgrupp & Kontext](#-projektgrupp--kontext)
- [AI-assistans](#-ai-assistans)
- [Skärmbilder](#-skärmbilder)

---

## 📁 Projektstruktur

Lösningen är uppdelad i två samverkande huvudprojekt. Projekten körs i separata utvecklingscontainrar för att hålla utvecklingsmiljöerna isolerade och reproducerbara.

| Projekt | Typ | Syfte |
|:---|:---|:---|
| **DLDA.API** | Web API | Hanterar affärslogik, databasåtkomst via EF Core, autentisering och API-endpoints. |
| **DLDA.GUI** | MVC Web App | Webbgränssnitt som kommunicerar med API:et och hanterar användarflöden och sessioner. |

---

## 🧱 Mappstruktur

```text
Projekt-DLDA-Offline/
├─ DLDA.API/
│  ├─ .devcontainer/           # Konfiguration för isolerad Docker-utvecklingsmiljö
│  ├─ Controllers/             # API-endpoints (Assessment, Auth, User, m.fl.)
│  ├─ Data/                    # AppDbContext för SQL Server-koppling
│  ├─ DTOs/                    # Data Transfer Objects för dataöverföring
│  └─ Models/                  # Datamodeller (User, Question, Assessment)
├─ DLDA.GUI/
│  ├─ .devcontainer/           # Konfiguration för isolerad Docker-utvecklingsmiljö
│  ├─ Authorization/           # RoleAuthorizeAttribute för RBAC-logik
│  ├─ Controllers/             # MVC-logik för Patient, Staff och Admin
│  ├─ Services/                # API-klienter (AccountService, QuizService, etc.)
│  ├─ Views/                   # Razor-vyer (HTML/C#)
│  └─ wwwroot/                 # CSS, JS och bilder
└─ docker-compose.yml          # Docker-konfiguration
```

---

## 🚀 Kom igång (Build & Run)

Projektet använder **Docker DevContainers** för utvecklingsmiljön. Det innebär att .NET-miljön körs i isolerade containrar istället för direkt på den lokala datorn.

### Förutsättningar

- Docker Desktop
- JetBrains Rider (rekommenderas) eller Visual Studio Code
- SQL Server, exempelvis via Docker eller Portainer

### Starta utvecklingsmiljön

1. Klona arkivet till din lokala maskin.
2. Öppna mappen `DLDA.API` som ett separat projekt i Rider eller Visual Studio Code.
3. Välj **Reopen in Container** / **Start DevContainer**.
4. Kontrollera anslutningssträngen i:

   `DLDA.API/appsettings.json`

5. Kontrollera att `ConnectionStrings` pekar mot rätt SQL Server-instans.
6. Kör API-projektet.

API:et och Swagger startar på:

```text
http://localhost:5001/swagger
```

7. Öppna mappen `DLDA.GUI` som ett separat projekt i en ny fönsterinstans.
8. Välj **Reopen in Container** / **Start DevContainer**.
9. Kör GUI-projektet.

Webbapplikationen startar på:

```text
http://localhost:5000
```

### Skapa testdata

När API:et är igång kan Swagger användas för att skapa grundläggande testdata.

Följande utvecklings-endpoints kan användas:

```text
POST /api/Auth/dev-update-admin
POST /api/Auth/dev-seed-questions
POST /api/Auth/dev-seed-users
```

Endpoints används för att bland annat skapa en administratör, lägga in skattningsfrågor samt skapa testpatient och testpersonal.

> **Obs:** Dessa endpoints är avsedda för utvecklingsmiljön och bör inte exponeras i en produktionsmiljö.

---

## 🔐 Användarroller & Säkerhet

Projektet använder rollbaserad åtkomst för att skilja mellan olika typer av användare.

### Roller

- **Admin** – Hanterar användare och skattningsfrågor.
- **Staff** – Kan arbeta med patienter och följa deras skattningar.
- **Patient** – Kan genomföra sina egna skattningar.

### Säkerhetslösningar

- **RBAC (Role-Based Access Control)**  
  En anpassad `RoleAuthorizeAttribute` styr åtkomst baserat på användarens roll:
  - Admin
  - Staff
  - Patient

- **Lösenordssäkerhet**  
  Använder **BCrypt.Net** för hashing och verifiering av lösenord.

- **Session State**  
  Användar-ID och roll används i sessionen för att hantera den inloggade användaren.

- **CORS-policy**  
  API:et är konfigurerat för att begränsa vilka klienter som får kommunicera med API:et.

---

## ⚙️ Funktioner

- **Inloggningssystem** – Autentisering och rollhantering.
- **Digitala formulär** – Interaktiva skattningsformulär för DLDA.
- **Patientöversikt** – Personal kan se och följa patienters resultat.
- **Statistik & uppföljning** – Visualisering av förändringar över tid.
- **Jämförelse mellan patient och personal** – Visar skillnader mellan skattningar.
- **Administrationspanel** – Hantering av användare och frågor (CRUD).
- **Responsiv design** – Anpassat för både mobil och desktop.

---

## 🧱 Arkitektur

Projektet består av en separat MVC-applikation och ett Web API.

```text
┌─────────────────┐
│    DLDA.GUI     │
│   ASP.NET MVC   │
└────────┬────────┘
         │
         │ HTTP
         ▼
┌─────────────────┐
│    DLDA.API     │
│  ASP.NET Core   │
└────────┬────────┘
         │
         │ EF Core
         ▼
┌─────────────────┐
│    SQL Server   │
└─────────────────┘
```

- **DLDA.GUI** – Ansvarar för presentation, användarflöden och kommunikation med API:et.
- **DLDA.API** – Ansvarar för API-endpoints, autentisering, affärslogik och databaskommunikation.
- **SQL Server** – Används för lagring av användare, frågor och skattningsresultat.

Kommunikationen mellan GUI och API sker via `HttpClient`. Dataåtkomst i API:et hanteras med Entity Framework Core.

---

## 🧩 Tekniska koncept som används

| Område | Implementation | Förklaring |
|:---|:---|:---|
| **Framework** | .NET 9 / ASP.NET Core | Grund för API och webbapplikation |
| **Frontend** | ASP.NET Core MVC / Razor | Webbgränssnitt och användarflöden |
| **Backend** | ASP.NET Core Web API | API och backendlogik |
| **Development Environment** | Docker / DevContainers | Isolerade och reproducerbara utvecklingsmiljöer |
| **Security** | RBAC & BCrypt | Hanterar roller och lösenord |
| **API Communication** | HttpClient / Async | Kommunikation mellan GUI och API |
| **Data Access** | EF Core / SQL Server | ORM och databashantering |
| **API Documentation** | Swagger | Dokumentation och testning av API-endpoints |
| **Architecture** | MVC + API | Separation mellan presentation och backend |

---

## 👥 Projektgrupp & Kontext

Detta projekt utvecklades som en del av kursen:

**Projektarbete och projektmetodik (7,5 hp)**  
(*Work and Project Methodology, 7.5 credits*)

Projektet genomfördes i en grupp om sju studenter med fokus på att kombinera teknisk utveckling med projektmetodik. Gruppen planerade, utvecklade och levererade tillsammans en fungerande prototyp.

### 🎯 Fokus i projektet

Arbetet inkluderade:

- Planering och strukturering av projektet.
- Framtagning av projektdirektiv och tidsplan.
- Riskanalys och hantering av potentiella problem.
- Samarbete och ansvarsfördelning inom gruppen.
- Löpande uppföljning och iteration av lösningen.
- Dokumentation, presentation och slutleverans.

### 🧠 Lärandeperspektiv

Projektet gav praktisk erfarenhet inom:

- Projektarbete som arbetsform inom IT.
- Utveckling av system med separerad frontend och backend.
- Hybridutveckling med Docker och DevContainers.
- API-baserad kommunikation.
- Gruppdynamik och problemlösning.
- Koppling mellan teknisk utveckling och verksamhetsbehov.

---

## 🤖 AI-assistans och kodgenerering

Delar av denna kodbas har utvecklats med stöd av AI-verktyg.

### Verktyg som använts

- **ChatGPT** – idéarbete, arkitektur, struktur, felsökning och dokumentation.
- **Gemini** – felsökning, refaktorisering och förbättring av kod.

### Omfattning

AI har bland annat använts som stöd för:

- Strukturering av backend-arkitektur.
- Konfiguration av DevContainers.
- Implementation av rollbaserad säkerhet.
- Felsökning.
- Refaktorisering och förbättring av kodläsbarhet.
- Dokumentation.

### Mänsklig granskning

AI har använts som ett stöd i utvecklingsprocessen och inte som en ersättning för utvecklingsarbetet.

AI-genererad kod har granskats, anpassats och testats manuellt innan den inkluderats i projektet. Projektgruppen ansvarar för den slutliga implementationen och funktionaliteten.

---

### 🎬 Demo
[Screencast_20260825_111134.webm](https://github.com/user-attachments/assets/7ad0865f-3987-4fe5-b5c5-7c3d60af3725)

---

## 🖼️ Skärmbilder

### 💻 Webbgränssnitt (Desktop)

**DLDA – Bedömning för patient**
![Patient Bedömning](Pictures/BedomingForPatientOversikt.png)

**DLDA – Förändring över tid**
![Förändring över tid](Pictures/ForandringOverTid.png)

**DLDA – Patientöversikt & Jämförelse**
![Patientöversikt](Pictures/PatientOversikt.png)
![Skillnad mellan patient och personal](Pictures/SkillnadMellanPatientOchPersonalSvar.png)

### 📱 Responsiv design (Mobilvy)

Gränssnittet är fullt anpassat för att vårdpersonal och patienter ska kunna använda systemet on-the-go.

| Patientöversikt | Bedömning | Förändring | Quiz-gränssnitt |
| :---: | :---: | :---: | :---: |
| <img src="Pictures/PatientOversikt_Mobil.png" alt="Patientöversikt Mobil" width="220"/> | <img src="Pictures/BedomingForPatientOversikt_Mobil.png" alt="Bedömning Mobil" width="220"/> | <img src="Pictures/ForandringOverTid_Mobil.png" alt="Förändring Mobil" width="220"/> | <img src="Pictures/Quiz_Mobil.png" alt="Quiz Mobil" width="220"/> |

---

---
