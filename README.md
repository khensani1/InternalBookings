# Internal Resource Booking System

This is a simple web application for managing and booking shared company resources (meeting rooms, vehicles, equipment) built with ASP.NET Core MVC, Entity Framework Core, and SQLite.

## Features
- Manage resources (CRUD)
- Book resources with conflict prevention
- View bookings and resource details
- Dashboard with stats and charts
- Responsive UI with Bootstrap

## Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- (Optional) [SQLite CLI](https://www.sqlite.org/download.html) for direct database inspection

## Setup Instructions

1. **Clone the repository:**
   ```bash
   git clone <your-repo-url>
   cd InternalResourceBooking
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Apply database migrations:**
   ```bash
   dotnet ef database update --project InternalResourceBooking/InternalResourceBooking.csproj
   ```
   > This will create a `resourcebooking.db` SQLite database file in the project directory.

4. **Run the application:**
   ```bash
   dotnet run --project InternalResourceBooking/InternalResourceBooking.csproj
   ```

5. **Open in your browser:**
   Navigate to `https://localhost:5001` or the URL shown in the terminal.

## Notes
- The database is pre-populated with sample resources for demo purposes.
- You can inspect or edit the database using any SQLite client.
- For development, you can edit connection strings in `appsettings.Development.json`.

## Project Structure
- `InternalResourceBooking/` - Main project folder
  - `Controllers/` - MVC controllers
  - `Models/` - Entity models
  - `Views/` - Razor views
  - `Migrations/` - EF Core migrations
  - `wwwroot/` - Static files (CSS, JS)

## Troubleshooting
- If you encounter migration or database errors, try deleting `resourcebooking.db` and re-running migrations.
- Ensure you have the correct .NET SDK version installed.
