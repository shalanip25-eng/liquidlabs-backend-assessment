

##  Architectural Decisions
Pure ADO.NET (Microsoft.Data.SqlClient)

What: No ORMs (like EF Core or Dapper) are used. Connections and commands are handled manually using raw SQL.

Why: Demonstrates low-level database mastery. Parameterized queries completely prevent SQL Injection.

Optimization: Centralized database operations inside a custom helper (ExecuteCommandAsync) to cleanly manage connection lifetimes.

Typed HTTP Client Factories

What: HttpClient is registered and configured directly within Program.cs specifically for our service.

Why: Prevents OS socket exhaustion by letting the framework manage connection pooling, while keeping external URLs decoupled from business logic.

Data Transfer Objects (DTO Isolation)

What: Internal DTO classes handle raw data from the third-party API before mapping it to internal models.

Why: Insulates the database and controller from breaking changes if the external API updates its response structure.


## API Endpoints & Caching Logic

The application exposes the following RESTful endpoints under `ExternalDataController`:

* **`GET /api/ExternalData`** - Retrieves the entire collection of programming records.
* **`GET /api/ExternalData/{id}`** - Retrieves a specific record matching the unique external ID parameter.

### Cache-Aside (Lazy Loading) Implementation Detail
Both endpoints strictly implement the required local caching lifecycle:
1. **Cache Interception:** The business service layer first queries the local MS SQL Server repository via raw SQL read operations.
2. **Cache Hit:** If the requested data records exist locally, the system skips the remote third-party network entirely and instantly returns the persistent local data.
3. **Cache Miss:** If the database contains no matching records, the system triggers the typed `HttpClient` to call the public Open Library API. The fetched dataset is seamlessly mapped, asynchronously cached into the SQL table using an `IF NOT EXISTS` raw insert guard, and returned to the consumer.

---

## Build, Configuration & Execution Instructions (Point 8 Compliance)

### 1. Database Initialization
Execute the following schema initialization script inside SQL Server Management Studio (SSMS) or Azure Data Studio to construct your local environment storage maps:

```sql
CREATE DATABASE LiquidLabsDb;
GO

USE LiquidLabsDb;
GO

CREATE TABLE ExternalData (
    ExternalId INT PRIMARY KEY,         
    UserId INT NOT NULL,             
    Title NVARCHAR(255) NOT NULL,
    Body NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE() 
);
GO

```
### 2. Connection Settings Check

Open appsettings.json and ensure your local Express connection values match your operational environment properties:

"ConnectionStrings": {
  "DefaultConnection": "Data Source=DESKTOP-5L450E9\\SQLEXPRESS;Initial Catalog=LiquidLabsDb;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;"
}
### 3. Build & Execution Steps via Command Line

Open your preferred terminal console in the root structural directory housing your solution file and run the following commands:

1.  Restore all required solution dependencies
  
command -    dotnet restore
      
2.   Compile and verify project status configurations

command -    dotnet build

4. Spin up the application engine hosting pipelines
   
command -    dotnet run --project LiquidLabsAssessment


Once the compilation run completes, open your browser to the automatically mapped local routing dashboard portal:

Interactive Swagger Interface Portal: https://localhost:44316/swagger

Raw API Collection Payload Streams: https://localhost:44316/api/ExternalData
