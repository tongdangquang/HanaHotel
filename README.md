# Hana Hotel – Overview and Guide

## 1. Product Overview

Hana Hotel is a web-based hotel management system designed to support room management, services, bookings, and administrative content. The project is organized with a clear layered architecture, making it suitable for learning purposes and real-world development.

The system includes the following main components:

- **FrontEnd/HanaHotel.WebUI**  
  User interface built with ASP.NET Razor Pages.  
  The admin area uses CKEditor and related plugins located in the `wwwroot/adminweb/plugins` directory.

- **ApiConsume/HanaHotel.WebApi**  
  Web API that provides data endpoints for the entire system, including Rooms, Services, Bookings, and related business logic.

- **HanaHotel.DataAccessLayer**  
  Data access layer using Entity Framework Core.  
  Includes `DataInitializer` to seed sample data such as Rooms and Services.

### Technical Information

- Language: C# 12  
- Platform: .NET 8  
- Architecture: Razor Pages + Web API  
- ORM: Entity Framework Core  

The goal of this project is to provide a sample hotel management application that can be extended with features such as online booking, content management, and system administration.

---

## 2. Installation and Running Guide

### 2.1. Environment Requirements

- .NET 8 SDK  
  <https://dotnet.microsoft.com>

- Visual Studio (version supporting .NET 8)  
  or Visual Studio Code (with C# extension installed)

- Database management systems:
  - SQL Server
  - LocalDB
  - SQLite (depending on configuration)

---

### 2.2. Clone the Source Code

```bash
git clone <repository-url>
cd <solution-folder>
```

---

### 2.3. Configure the Connection String

Edit the following file:

```
FrontEnd/HanaHotel.WebUI/appsettings.json
```

Configure the `ConnectionStrings` section to match your running environment.

---

### 2.4. Apply Migrations and Create the Database

If the repository already contains Entity Framework Core migrations, follow these steps:

- Open Visual Studio
- Go to **Tools > NuGet Package Manager > Package Manager Console**
- Set the default project to **HanaHotel.DataAccessLayer**
- Run the command:

```powershell
Update-Database
```

---

### 2.5. Build and Run the Application

#### Run with Visual Studio

- Open the solution file (`.sln`)
- Right-click **Solution** → **Set Startup Projects**
- Select **Multiple startup projects** to run both WebUI and WebAPI
- Press:
  - `F5` to run in Debug mode
  - `Ctrl + F5` to run without Debug

#### Run with CLI

```bash
dotnet restore
dotnet build
dotnet run
```
