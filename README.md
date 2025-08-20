# Excel via Entity Framework

This project demonstrates how to use **Entity** **Framework Core** with **Microsoft Excel files** as a backing datastore.  
It provides a simple **console user interface** for managing signups (add, update, delete, list) directly in an .xlsx file.

Inspired by [this blog](https://www.bricelam.net/2024/03/12/ef-xlsx.html) by Brice Lambson.

- - -

**✨ Features**

*   Treat an Excel worksheet as a database table.
*   CRUD operations on signups (Name, Phone number, Party size).
*   EF Core integration with custom DbContext.
*   Console UI with feedback and menu-driven commands.

- - -

**⚙️ Prerequisites**

*   **.NET 8 SDK** (or later)
*   **Microsoft ACE OLEDB 12.0 provider** installed (required for Excel/Jet access)

- - -

**▶️ Running the Project**

1.  Clone the repository.
2.  Build and run:  dotnet run --project ExcelDBviaEntityFramework

An initial Excel file will be copied in the correct folder (/Excel files/).

3.  Use the console menu to access the CRUD functionality

- - -

**🗄 Data Persistence**

*   **Insert** **/ Update** → Executed via raw SQL into Excel file.
*   **Delete** → Soft delete (marks row as Deleted = true).

- - -

**🧩 Tech Highlights**

*   **Entity** **Framework Core** + custom repository pattern for Excel.
*   **Dependency** **Injection** via Microsoft.Extensions.DependencyInjection.
*   **ClosedXML** for low-level Excel file manipulation (row removal).
*   **Custom** **SaveChanges() override** to intercept EF Core state tracking and translate into Excel SQL.

- - -

**🚀 Possible Extensions**

*   Add import/export to CSV.
*   Web API layer exposing the same Excel-backed database.

- - -

**📜 License**

MIT License – feel free to use and adapt.

- - -

<!-- <img width="500" alt="image" src="https://github.com/user-attachments/assets/21296658-58bb-4b62-a68b-7a9eab8afdb8" /> -->
<img width="500" alt="image" src="https://github.com/user-attachments/assets/945380a5-0891-4785-8c6e-6717ea633fd3" />

