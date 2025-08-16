# Excel via Entity Framework

This project demonstrates how to use **Entity** **Framework Core** with **Microsoft Excel files** as a backing datastore.  
It provides a simple **console user interface** for managing signups (add, update, delete, list) directly in an .xlsx file.

- - -

**✨ Features**

*   Treat an Excel worksheet as a database table.
*   CRUD operations on signups (Name, Phone number, Party size).
*   EF Core integration with custom DbContext.
*   Console UI with colorized feedback and menu-driven commands.

- - -

**⚙️ Prerequisites**

*   **.NET 8 SDK** (or later)
*   **Microsoft ACE OLEDB 12.0 provider** installed (required for Excel/Jet access)
*   Excel file placed in the folder:
*   ./Excel files/YourExcelFile.xlsx

**Excel file requirements**

*   File name = Constants.ExcelFileName
*   Must contain a sheet named Constants.SheetName
*   First row contains headers:

*   Id\_ý
*   Deleted\_ý
*   Name
*   Phone number
*   Party size

- - -

**▶️ Running the Project**

1.  Clone the repository.
2.  Ensure the Excel file exists in the correct folder (/Excel files/).
3.  Build and run:
4.  dotnet run --project ExcelDBviaEntityFramework
5.  Use the console menu:
6.  ########################################
7.  # ExcelDBviaEntityFramework #
8.  ########################################

10.  Make a choice:
11.  \- Add sign up (a)
12.  \- Update sign up (u)
13.  \- Delete sign up (d)
14.  \- Print sign ups (p)
15.  \- Help (h)
16.  \- Quit (q)

- - -

**🗄 Data Persistence**

*   **Insert** **/ Update** → Executed via raw SQL into Excel file.
*   **Delete** → Soft delete (marks row as Deleted\_ý = true).
*   **Cleanup** → Rows physically removed by ExcelHelper.RemoveDeletedRows.

- - -

**🧩 Tech Highlights**

*   **Entity** **Framework Core** + custom repository pattern for Excel.
*   **Dependency** **Injection** via Microsoft.Extensions.DependencyInjection.
*   **ClosedXML** for low-level Excel file manipulation (row removal).
*   **Custom** **SaveChanges() override** to intercept EF Core state tracking and translate into Excel SQL.

- - -

**🚀 Possible Extensions**

*   Support multiple entities / sheets.
*   Add import/export to CSV.
*   Web API layer exposing the same Excel-backed database.
*   Improve concurrency handling on locked files.

- - -

**📜 License**

Inspired by [this blog](https://www.bricelam.net/2024/03/12/ef-xlsx.html) by Brice Lambson.

MIT License – feel free to use and adapt.
