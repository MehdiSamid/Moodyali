# Moodyali - Full-Stack Mood Tracker MVP

Moodyali is a full-stack Minimum Viable Product (MVP) for a mood tracking application.

## Technology Stack

*   **Backend:** .NET 8 Minimal API
*   **Database:** Entity Framework Core (SQLite for local development, SQL Server for production)
*   **Authentication:** JWT Bearer Token
*   **Frontend:** Lightweight HTML, CSS, and Vanilla JavaScript

## Project Structure

*   `Moodyali.API`: The main web API project, containing services, endpoints, and the `wwwroot` for the frontend.
*   `Moodyali.Core`: Contains core entities (`User`, `Mood`) and service interfaces.
*   `Moodyali.Shared`: Contains Data Transfer Objects (DTOs) and helper classes.

## Local Setup and Run

### Prerequisites

*   [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Steps

1.  **Navigate to the API directory:**
    ```bash
    cd Moodyali/Moodyali.API
    ```

2.  **Restore dependencies:**
    ```bash
    dotnet restore
    ```

3.  **Run the application:**
    The application is configured to use **SQLite** for local development, which will automatically create a `moodyali.db` file in the `Moodyali.API` directory.
    ```bash
    dotnet run
    ```

4.  **Access the application:**
    The API will typically run on `http://localhost:5000` or `http://localhost:5001` (HTTPS).
    *   **Frontend:** Open `http://localhost:<port>` in your browser.
    *   **Swagger UI:** Access the API documentation at `http://localhost:<port>/swagger`.

## Azure Deployment (SQL Server)

The application is configured to be deployable on Azure App Service with SQL Server.

### Configuration

The following settings must be configured as **Application Settings** in your Azure App Service instance. These settings will override the values in `appsettings.json`.

| Setting Name | Value Example | Description |
| :--- | :--- | :--- |
| `ConnectionStrings:DefaultConnection` | `Server=tcp:<server>.database.windows.net,...` | The connection string for your Azure SQL Database. |
| `Jwt:Secret` | `Your_Long_And_Secure_JWT_Secret_Key` | A long, complex secret key for signing JWTs. |
| `Jwt:Issuer` | `Moodyali` | The issuer of the JWT token. |
| `Jwt:Audience` | `MoodyaliClient` | The audience of the JWT token. |

## API Endpoints

| Method | Path | Description | Authentication |
| :--- | :--- | :--- | :--- |
| `POST` | `/auth/register` | Registers a new user. | None |
| `POST` | `/auth/login` | Logs in a user and returns a JWT token. | None |
| `POST` | `/mood` | Logs or updates today's mood. | Required |
| `GET` | `/mood/today` | Returns today's logged mood. | Required |
| `GET` | `/mood/week` | Returns mood data for the last 7 days. | Required |
| `GET` | `/mood/stats` | Returns mood statistics (average score, happy/sad days). | Required |

## Emoji Scoring

| Emoji | Score Range |
| :--- | :--- |
| 😢 | 0-2 (Mapped to 1) |
| 🙁 | 3-4 (Mapped to 3) |
| 😐 | 5 (Mapped to 5) |
| 🙂 | 6-7 (Mapped to 7) |
| 😄 | 8-10 (Mapped to 9) |
