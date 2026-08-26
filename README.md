# 🌐 Project DLDA

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/Docker-DevContainers-2496ED?style=flat&logo=docker&logoColor=white)](https://www.docker.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-brightgreen?style=flat&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/apps/aspnet)
[![Entity Framework](https://img.shields.io/badge/EF%20Core-SQL%20Server-blue?style=flat&logo=microsoftsqlserver&logoColor=white)](https://learn.microsoft.com/en-us/ef/core/)
[![Security](https://img.shields.io/badge/Security-RBAC%20%26%20BCrypt-red?style=flat)](https://github.com/BcryptNet/bcrypt.net)

## 📝 Introduction

**DLDA** is a web-based prototype that digitizes the psychiatric assessment tool **DLDA (Daily Life Dialogue Assessment in Psychiatric Care)**.

The project consists of an ASP.NET Core MVC application and a separate Web API that together handle the user interface, authentication, assessments, and data management. The system uses Entity Framework Core and SQL Server for data storage and runs in a Docker-based environment.

The goal is to make the assessment easier to complete digitally while giving healthcare professionals the ability to track results, compare assessments, and monitor changes over time.

---

## 🎬 Demo

[Screencast_20260825_122507.webm](https://github.com/user-attachments/assets/e1e9070c-7691-4007-aa5d-4db576f3f531)

---

## 🖼️ Screenshots

### 💻 Web Interface (Desktop)

**DLDA – Patient Assessment**  
![Patient Assessment](Pictures/BedomingForPatientOversikt.png)

**DLDA – Change Over Time**  
![Change Over Time](Pictures/ForandringOverTid.png)

**DLDA – Patient Overview**  
![Patient Overview](Pictures/PatientOversikt.png)

**DLDA – Quiz Interface**  
![Quiz](Pictures/Quiz.png)

### 📱 Responsive Design (Mobile View)

The interface is designed so that healthcare professionals and patients can use the system on both mobile and desktop.

| Overview | Assessment | Change | Quiz | Comparison |
| :---: | :---: | :---: | :---: | :---: |
| <img src="Pictures/PatientOversikt_Mobil.png" alt="Patient Overview Mobile" width="160"/> | <img src="Pictures/BedomingForPatientOversikt_Mobil.png" alt="Assessment Mobile" width="160"/> | <img src="Pictures/ForandringOverTid_Mobil.png" alt="Change Mobile" width="160"/> | <img src="Pictures/Quiz_Mobil.png" alt="Quiz Mobile" width="160"/> | <img src="Pictures/SkillnadMellanPatientOchPersonalSvar_Mobil.png" alt="Difference Mobile" width="160"/> |

---

## 📋 Contents

- [What is DLDA?](#what-is-dlda)
- [Purpose](#-purpose)
- [Limitations](#️-limitations)
- [Project Structure](#-project-structure)
- [Folder Structure](#-folder-structure)
- [Getting Started (Build & Run)](#-getting-started-build--run)
- [User Roles & Security](#-user-roles--security)
- [Features](#️-features)
- [Architecture](#-architecture)
- [Technical Concepts Used](#-technical-concepts-used)
- [Project Team & Context](#-project-team--context)
- [AI Assistance](#-ai-assistance)

---

## What is DLDA?

**DLDA (Daily Life Dialogue Assessment in Psychiatric Care)** is an assessment tool based on the World Health Organization's (WHO) ICF classification. The tool is used in psychiatric care to provide an overview, together with the patient and healthcare professional, of how different aspects of the patient's everyday life are functioning.

The assessment consists of questions covering nine areas, including personal care, communication, domestic life, and relationships. Both the patient and healthcare professional provide their own assessment on a scale from **0 to 4**, where 0 indicates no problem and 4 indicates a very significant problem.

The results can then be used as a basis for discussions between the patient and healthcare professional, for example regarding needs, support, and continued care.

---

## 🎯 Purpose

The purpose of the project is to explore how DLDA could work in a digital environment instead of on paper.

The aim is to make it easier for patients to complete their assessments while giving healthcare professionals a clearer overview of the results. The system makes it possible to compare patient and staff assessments, visualize results, and track changes over time.

---

## ⚠️ Limitations

This project is a prototype developed for educational purposes and is not intended to be used as a finished clinical healthcare system.

Its features, security, and data storage are adapted to the technical requirements of the project. The project must therefore not be used to handle real patient data or in clinical practice.

---

## 📁 Project Structure

The solution is divided into two cooperating main projects:

| Project | Type | Purpose |
|:---|:---|:---|
| **DLDA.API** | Web API | Handles business logic, database access through EF Core, authentication, and API endpoints. |
| **DLDA.GUI** | MVC Web App | Web interface that communicates with the API and handles user flows and sessions. |

The project can be run in two different ways depending on the use case:

- **Docker Compose** is used to run the different parts of the project as a single environment.
- **DevContainers** are used as development environments for each project. Both `DLDA.API` and `DLDA.GUI` have their own DevContainer configurations, making it possible to develop in isolated and reproducible environments.

---

## 🧱 Folder Structure

```text
Projekt-DLDA-Offline/
├─ DLDA.API/
│  ├─ .devcontainer/           # Docker development environment configuration
│  ├─ Controllers/             # API endpoints (Assessment, Auth, User, etc.)
│  ├─ Data/                    # AppDbContext for SQL Server connection
│  ├─ DTOs/                    # Data Transfer Objects for data transfer
│  └─ Models/                  # Data models (User, Question, Assessment)
├─ DLDA.GUI/
│  ├─ .devcontainer/           # Docker development environment configuration
│  ├─ Authorization/           # RoleAuthorizeAttribute for RBAC logic
│  ├─ Controllers/             # MVC logic for Patient, Staff, and Admin
│  ├─ Services/                # API clients (AccountService, QuizService, etc.)
│  ├─ Views/                   # Razor views (HTML/C#)
│  └─ wwwroot/                 # CSS, JS, and images
└─ docker-compose.yml          # Docker Compose configuration
```

---

## 🚀 Getting Started (Build & Run)

The project can be run in two ways:

- **Docker Compose** – to run the different parts of the project as a single environment.
- **DevContainers** – for development in separate and reproducible development environments.

### Prerequisites

- Docker Desktop
- JetBrains Rider (recommended) or Visual Studio Code
- SQL Server

### 🐳 Option 1 – Docker Compose

1. Clone the repository to your local machine.
2. Make sure SQL Server is available and that the connection string is configured correctly.
3. Start the project with Docker Compose:

```bash
docker compose up --build
```

4. Once the containers have started, the application can be accessed through the ports specified in `docker-compose.yml`.

> Check `docker-compose.yml` for the current ports and environment variables.

### 🛠️ Option 2 – DevContainers

The project contains separate DevContainer configurations for `DLDA.API` and `DLDA.GUI`.

#### Start the API

1. Open the `DLDA.API` folder in Rider or Visual Studio Code.
2. Select **Reopen in Container** / **Start DevContainer**.
3. Check the connection string in:

```text
DLDA.API/appsettings.json
```

4. Make sure `ConnectionStrings` points to the correct SQL Server instance.
5. Run the API project.

The API and Swagger are available at:

```text
http://localhost:5001/swagger
```

#### Start the GUI

1. Open the `DLDA.GUI` folder in a separate window.
2. Select **Reopen in Container** / **Start DevContainer**.
3. Run the GUI project.

The web application is available at:

```text
http://localhost:5000
```

### Create Test Data

Once the API is running, Swagger can be used to create basic test data.

The following development endpoints are available:

```text
POST /api/Auth/dev-update-admin
POST /api/Auth/dev-seed-questions
POST /api/Auth/dev-seed-users
```

These endpoints can be used to create an administrator, seed assessment questions, and create test patient and staff users.

> **Note:** These endpoints are intended for the development environment and should not be exposed in a production environment.

---

## 🔐 User Roles & Security

The project uses role-based access control to distinguish between different types of users.

### Roles

- **Admin** – Manages users and assessment questions.
- **Staff** – Can work with patients and track their assessments.
- **Patient** – Can complete their own assessments.

### Security Measures

- **RBAC (Role-Based Access Control)**  
  A custom `RoleAuthorizeAttribute` controls access based on the user's role:
  - Admin
  - Staff
  - Patient

- **Password Security**  
  Uses **BCrypt.Net** for password hashing and verification.

- **Session State**  
  The user ID and role are stored in the session to manage the authenticated user.

- **CORS Policy**  
  The API is configured to restrict which clients are allowed to communicate with it.

---

## ⚙️ Features

- **Login System** – Authentication and role management.
- **Digital Forms** – Interactive assessment forms for DLDA.
- **Patient Overview** – Staff can view and track patient results.
- **Statistics and Follow-up** – Visualization of changes over time.
- **Patient vs. Staff Comparison** – Shows differences between assessments.
- **Administration Panel** – Management of users and questions (CRUD).
- **Responsive Design** – Adapted for both mobile and desktop.

---

## 🧱 Architecture

The project consists of a separate MVC application and a Web API.

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

- **DLDA.GUI** – Handles presentation, user flows, and communication with the API.
- **DLDA.API** – Handles API endpoints, authentication, business logic, and database communication.
- **SQL Server** – Used to store users, questions, and assessment results.

Communication between the GUI and API is handled through `HttpClient`. Data access in the API is managed with Entity Framework Core.

---

## 🧩 Technical Concepts Used

| Area | Implementation | Description |
|:---|:---|:---|
| **Framework** | .NET 9 / ASP.NET Core | Foundation for the API and web application |
| **Frontend** | ASP.NET Core MVC / Razor | Web interface and user flows |
| **Backend** | ASP.NET Core Web API | API and backend logic |
| **Development Environment** | Docker / DevContainers | Isolated and reproducible development environments |
| **Security** | RBAC & BCrypt | Handles roles and passwords |
| **API Communication** | HttpClient | Communication between the GUI and API |
| **Data Access** | EF Core / SQL Server | ORM and database management |
| **API Documentation** | Swagger | Documentation and testing of API endpoints |
| **Architecture** | MVC + API | Separation between presentation and backend |

---

## 👥 Project Team & Context

This project was developed as part of the course:

**Project Work and Project Methodology (7.5 credits)**  
(*Work and Project Methodology, 7.5 credits*)

The project was completed by a team of seven students, where we jointly planned, developed, and delivered a working prototype.

### 👨‍💻 My Role

I served as **project manager and backend developer**.

As project manager, I was responsible for:

- Interpreting the project brief and planning the initial project work.
- Assigning roles and tasks within the team.
- Scheduling and tracking project milestones.
- Communication within the team and with the client.
- Identifying and managing technical and practical risks.

As a backend developer, I primarily worked on:

- Authentication and user roles.
- Question structures and quiz flow.
- Statistics and result presentation.
- Staff assessment workflows.
- Database structure and database connections.
- Controllers, services, and DTOs in the backend.
- Technical decisions, troubleshooting, and implementation.

I also contributed to creating wireframes in Figma and worked on technical solutions for areas such as PDF generation and user experience.

### 🎯 Project Focus

The project work included:

- Project planning and scheduling.
- Risk analysis and management of potential issues.
- Responsibility allocation and collaboration within the team.
- Ongoing follow-up and iteration of the solution.
- Documentation, presentation, and final delivery.

### 💡 Experience Gained

The project provided practical experience in:

- Project management and working in development teams.
- Developing with a separated frontend and backend.
- ASP.NET Core MVC and Web API.
- API-based communication.
- Docker and DevContainers.
- Teamwork, problem-solving, and technical decision-making.
- Connecting technical solutions to business needs.

---

## 📜 License

This project is distributed under the **MIT License**.

```text
MIT License

Copyright (c) 2025 DLDA

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

---

## 🤖 AI Assistance

AI tools such as **ChatGPT** and **Gemini** were used as support during development, primarily for brainstorming, troubleshooting, refactoring, and documentation.

AI-generated code was reviewed, adapted, and manually tested before being included in the project. The project team is responsible for the final implementation and functionality.
