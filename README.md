# Library Management System

## Setup Instructions

1. Open `LibrarySystem.sln` in Visual Studio 2026
2. Build the project (Ctrl+Shift+B)
3. Open Package Manager Console (Tools > NuGet Package Manager > Package Manager Console)
4. Run `Update-Database`
5. Run the application (F5 or Ctrl+F5)
6. Seed data populates automatically on first run

## Login Credentials

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@library.com | Admin123! |
| Reception | reception@library.com | Reception123! |
| Manager | manager@library.com | Manager123! |

## Role Permissions

- **Admin**: CRUD for Books, Toys, and Music. Can set item status and on order flag.
- **Reception**: CRUD for Members. Create loans, return items, pay fines, reserve items.
- **Manager**: View borrowing, item, and fine statistics dashboard.
- **Public**: Search and browse items via the Library Portal. No login required.

## Architecture

ASP.NET Core MVC with MVVM. Controllers handle routing and coordinate flow. ViewModels contain business logic including entity mapping, author resolution, genre loading, and library code generation. Models define the data structure and database schema.

## Database

Entity Framework Core with SQL Server LocalDB. TPH (Table Per Hierarchy) inheritance with an abstract Item base class and three subclasses: Book, Toy, Music, stored in a single Items table with a Discriminator column. Lookup tables use many-to-many relationships with EF Core join tables. Borrowers and Loans are separate tables with foreign key relationships.

## Authentication and Authorisation

ASP.NET Core Identity with Individual Accounts. Three roles seeded on startup: Admin, Reception, Manager. Controllers use Authorize attributes to restrict access. The public Library Portal requires no authentication.

## Modules

### Admin Module
BooksController, ToysController, MusicController. Full CRUD for each item type. Search with Select2 autocomplete. Status filter with Has Reservation and On Order options. OnOrder flag appears as a checkbox when status is Damaged, Destroyed, or Lost. Library codes auto-generate per item type (BKS, TOY, MUS).

### Reception Module
BorrowersController, LoansController. Borrower CRUD with search and status filter. Loan creation supports up to 10 items per transaction with lookup validation. Items set to Borrowed with a 14-day due date. Return calculates fines at $1 per day late and auto-suspends the borrower. Pay Fine clears the fine and reactivates the borrower if no other fines remain. Reservation system allows reserved items for a member with confirmation and status tracking.

### Manager Module
ManagerController. Dashboard with three tabbed sections. Borrowing: total, active, and overdue loans, most borrowed items, busiest borrowers, loans per month. Items: counts by type and status, on order and reserved counts, popular genres, items added per month. Fines: outstanding and collected fines, average fine, borrowers with outstanding fines.

### Public Module
PortalController. Unified search across all item types with Select2 autocomplete. Browse tabs for Books, Toys, and Music with filter and sort dropdowns. Destroyed and lost items filtered out. Responsive mobile layout with cards on small screens and tables on desktop. Detail pages with random suggestion links.

### Home Page
Welcome dashboard with live item counts using View Dependency Injection. Cards link to random item detail pages.

## Validation

- **YearRangeAttribute**: Custom validation for publication/release year (1450 to current year) with client-side support via IClientModelValidator. Supports an unknown flag that bypasses validation.
- **MinimumAgeAttribute**: Validates borrowers are at least 12 years old from date of birth.
- Standard DataAnnotations used throughout for Required, StringLength, Range, EmailAddress, Phone, and RegularExpression.

## Seed Data

SeedData.cs runs on application startup. Seeds three roles and user accounts, all lookup table data, 10 books, 11 toys, 9 music items, 6 borrowers, and loan records including active loans, overdue loans, and fines. Demonstrates all item statuses, on order flags, and reservations.

## Dependency Injection

- **Controller DI**: All controllers receive ApplicationDbContext via constructor injection.
- **View DI**: Home page and Portal view use @inject to access ApplicationDbContext directly for live item counts. _LoginPartial uses @inject for SignInManager and UserManager.

## LINQ

Used throughout all controller queries: Where, Select, Include, GroupBy, OrderBy, Any, Count, Sum, Average, FirstOrDefaultAsync, ToListAsync.