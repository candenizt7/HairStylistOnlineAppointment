# Hair Stylist Online Appointment System

An ASP.NET Core MVC project developed as a portfolio piece.  
Allows customers to book, view, and cancel appointments online.  
Admins can manage services and appointments.

## 📸 Screenshots

### 🏠 Home Page
![Home Page](docs/screenshots/home.png)

### 🔑 Login Page
![Login Page](docs/screenshots/login.png)

### 📝 Register Page
![Register Page](docs/screenshots/register.png)

### 📅 Book Appointment
![Book Appointment](docs/screenshots/book1.png)
![Book Appointment](docs/screenshots/book2.png)

### 📖 My Appointments
![My Appointments](docs/screenshots/appointments.png)

### ⚙️ Admin Dashboard
![Admin Dashboard](docs/screenshots/edit1.png)
![Admin Dashboard](docs/screenshots/edit2.png)


## Features
- User authentication (ASP.NET Identity)
- Book / cancel appointments
- Service management (Admin)
- Calendar integration (FullCalendar)
- Bootstrap 5 responsive UI

## Tech Stack
- ASP.NET Core 8 MVC
- Entity Framework Core (Code First)
- SQL Server LocalDB
- Identity for auth
- Bootstrap 5, FullCalendar.js

## Setup
1. Clone the repo
2. Update **appsettings.json** with your SQL Server connection string
3. Run `update-database`
4. Run the project with `dotnet run`

## Default Admin
- Email: (see `appsettings.json:Seed:AdminEmail`)
- Password: set during seeding
