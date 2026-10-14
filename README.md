# Library Management System

**Repository:** [https://github.com/MelBardoux/HIT339-Group-5-Library]

## Team Members

| Name | Student ID | Email |
|---|---|---|
| Student 1 | - | @students.cdu.edu.au |
| Student 2 | - | @students.cdu.edu.au |
| Student 3 | - | @students.cdu.edu.au |
| Student 4 | - | @students.cdu.edu.au |

## 1. Project Overview

The Library Management System is an ASP.NET Core MVC application expanded to support multiple library branches and additional access channels and integrations.

The expanded application includes role-restricted staff dashboards, multi-branch inventory management, inter-branch transfers, digital reservations, automated notifications, CSV item import, manager reporting, a public catalogue API, and a self-service kiosk.

This README explains how to navigate these features, seed and verify multi-branch data, and test the expanded functionality.

For cloning the repository, configuring the application, and running it locally, refer to the separate Installation and Running Instructions in the group portfolio (PDF).

## 2. Test Accounts

The following seeded accounts are provided for development and demonstration.

| Role | Email | Password |
|---|---|---|
| Admin | admin@library.com | Admin123! |
| Darwin Reception | darwin.receptionists@email.com | Reception123! |
| Darwin Manager | darwin.manager@email.com | Manager123! |
| Sydney Reception | sydney.receptionists@email.com | Reception123! |
| Sydney Manager | sydney.manager@email.com | Manager123! |
| Brisbane Reception | brisbane.receptionists@email.com | Reception123! |
| Brisbane Manager | brisbane.manager@email.com | Manager123! |


## 3. Navigating the Application

After starting the application, use the home page and navigation menu to access the available features. Staff must sign in with an account assigned to the relevant role. Navigation links are restricted according to user permissions.

### 3.1 Admin Dashboard

**Access:** Admin account.

- Catalogue Management (baseline from Assignment 2)

- External Book/Media Data Importer (new module)

  > **Navigation:** Admin Dashboard > Bulk Import (also accessible by Manager and Reception)
  >
  > **Module Overview**
  > An administrative utility that allows staff to bulk-import new Books and Music items via CSV file upload or by querying a mock external metadata provider.
  >
  > **Features**
  > - **CSV Upload**
  >   - Select item type (Book or Music) and destination branch
  >   - Upload a CSV file with item records
  >   - Sample CSV files available for download
  > - **External Metadata Query**
  >   - Search books by title, author, or ISBN
  >   - Search music by artist, album, or ISRC
  >   - Enrich individual or batch items with external metadata
  >
  > **Integration Test**
  > 1. Sign in as Admin.
  > 2. Navigate to Bulk Import.
  > 3. Select "Book" as the item type and choose a branch.
  > 4. Upload a sample Books CSV file.
  > 5. Verify the preview displays the parsed rows with validation results.
  > 6. Confirm the import.
  > 7. Navigate to Books in the Admin dashboard and verify the imported items appear under the selected branch.
  > 8. Open the Library Portal and verify the imported items are visible in the catalogue with the correct branch.

- Automated Multi-Channel Notification System (new module)

  > **Navigation:** Admin Dashboard > Notifications (also accessible by Manager and Reception)
  >
  > **Module Overview**
  > Simulates automated email and SMS notifications sent to borrowers when items are borrowed, nearing their due date, overdue, or fined. Notifications are logged to a dashboard table. Due-date reminders run automatically every hour via a background service.
  >
  > **Features**
  > - **Notification Log** - view records, summary cards, search, filter by type/channel, responsive layout
  > - **Manual Due-Date Check** - on-demand button, one reminder per loan per day
  > - **Automated Triggers** - item borrowed, approaching due date (within 2 days), overdue with accruing fine, fine charged on late return, notifications include branch name
  >
  > **Integration Test**
  > 1. Log in as Darwin Reception (`darwin.receptionists@email.com` / `Reception123!`)
  > 2. Navigate to **Borrowers** and locate **Jane Smith** (BRW-0001)
  > 3. Borrow an available item, e.g. **"A Brief History of Time"** (BKS-0002) - verify an "Item borrowed" notification appears in the log for both Email and SMS channels
  > 4. Navigate to **Notifications**
  > 5. Click **"Run due-date check now"** - verify reminders are generated for existing overdue loans (e.g. Alex Turner's overdue loan for "Thriller" MUS-0002)
  > 6. Use the **Type** filter to show only "Overdue" notifications - verify the overdue message includes the fine amount and days overdue
  > 7. Use the **Search** field to search "BRW-0006" - verify only Alex Turner's notifications appear
  > 8. Filter by **Channel** "SMS" - verify SMS-specific notifications are shown with phone number as recipient
  > 9. Verify summary cards update: Total sent, Sent today, Emails, SMS counts reflect the new notifications

### 3.2 Reception Dashboard

**Access:** Reception account assigned to a branch.

- Borrowers and Loans (baseline from Assignment 2)

- Multi-Branch Inventory and Inter-Branch Transfers (new module)

  > **Navigation:** Reception/Manager Dashboard > Item Transfers (accessible by Reception and Manager)
  >
  > **Module Overview**
  > Enables staff to coordinate inter-branch item transfers. Staff can request available items held by other branches, and the source branch can approve or reject requests. Approved items are marked as In Transit until received by the requesting branch. On receipt, if a waiting reservation exists at that branch, the item is automatically reserved and the borrower is notified.
  >
  > **Features**
  > - **Request Transfer** - select from available items at other branches, only items that are Available and not On Order or already in an active transfer can be requested
  > - **Approve/Reject** - source branch staff can approve (marks item as In Transit) or reject incoming requests
  > - **Receive** - requesting branch confirms receipt, item's branch is updated, status returns to Available
  > - **Reservation Integration** - on receipt, if a waiting reservation exists at the receiving branch, the item is automatically reserved and the borrower notified with 48-hour pickup window
  > - **Transfer Dashboard** - three sections: Requests Made by This Branch, Incoming Transfer Requests, Transfer History (completed and rejected)
  > - **Pending Request Highlight** - Reception dashboard tile highlights with a red-tinted background when there are pending incoming transfer requests
  >
  > **Integration Test**
  > 1. Log in as Sydney Reception (`sydney.receptionists@email.com` / `Reception123!`)
  > 2. Navigate to **Item Transfers** and click **New Transfer Request**
  > 3. Select an available item from Darwin, e.g. **"A Brief History of Time"** (BKS-0002) - confirm the request is created
  > 4. Log out. Log in as Darwin Reception (`darwin.receptionists@email.com` / `Reception123!`)
  > 5. Verify the **Item Transfers** tile on the dashboard is highlighted
  > 6. Navigate to **Item Transfers** - verify the request appears under **Incoming Transfer Requests** with **Approve** and **Reject** buttons
  > 7. Click **Approve** - verify the status changes to "InTransit"
  > 8. Log out. Log in as Sydney Reception again
  > 9. Navigate to **Item Transfers** - verify the request appears under **Requests Made by This Branch** with a **Receive** button
  > 10. Click **Receive** - verify the transfer is completed and the item is now available in Sydney inventory

- Digital Reservation and Waitlist Queue (new module)

  > **Navigation:** Reception Dashboard > Reservations (accessible by Reception and Manager); reservations are placed via Loans > Borrow (Reception only)
  >
  > **Module Overview**
  > Allows patrons to place a hold on items that are currently borrowed or unavailable. When the item is returned, the next borrower in the queue is automatically notified with a 48-hour pickup window. If the item is returned at a different branch from where the reservation was placed, a transfer request is automatically created. Expired reservations are handled by the background service, which promotes the next waiting borrower.
  >
  > **Features**
  > - **Place Reservation** - Reception places a hold via the Borrow page when an item is unavailable; shows queue position and confirms reservation
  > - **Automatic Queue Promotion** - on return, the next waiting borrower is notified with 48-hour pickup window; if no one is waiting, item becomes Available
  > - **Cross-Branch Transfer Flow** - if an item is returned at a different branch from the reservation's branch, an inter-branch transfer request is automatically created
  > - **48-Hour Expiry** - background service checks hourly; expired reservations are marked Expired, next borrower is promoted and notified
  > - **Reservation History** - view all reservations with status filter (Waiting, Ready, Collected, Cancelled, Expired)
  >
  > **Integration Test (Same-Branch Reservation)**
  > 1. Log in as Darwin Reception (`darwin.receptionists@email.com` / `Reception123!`)
  > 2. Navigate to **Loans > Borrow**, enter borrower **Tom Wilson** (BRW-0002)
  > 3. Look up **"To Kill a Mockingbird"** (BKS-0001) - item is currently Borrowed; click **Reserve**
  > 4. Confirm the reservation - verify Tom Wilson is placed in the queue
  > 5. Navigate to **Loan Records**, find Jane Smith's active loan for BKS-0001, and process the **Return**
  > 6. Verify Tom Wilson is automatically notified (check Notifications for "Your reserved item is ready" with branch name and 48-hour collection message)
  > 7. Navigate to **Reservations** - verify the status is "Ready"
  >
  > **Integration Test (Cross-Branch Reservation)**
  > 1. Log in as Darwin Reception (`darwin.receptionists@email.com` / `Reception123!`)
  > 2. Navigate to **Loan Records**, find Alex Turner's overdue loan for **"Thriller"** (MUS-0002) and process the **Return**
  > 3. Verify a **transfer request** is automatically created in Item Transfers (from Darwin to Sydney) - Olivia Patel has a waiting reservation placed at Sydney branch
  > 4. Verify Olivia's reservation remains "Waiting" until the transfer is completed

- Notifications (read only)

### 3.3 Manager Dashboard

**Access:** Manager account.

- Statistics (baseline from Assignment 2)

- Expanded Manager Analytics and Export (new module)

  > **Navigation:** Manager Dashboard > Statistics (tabs: Borrowing, Items, Fines, Reservations)
  >
  > **Module Overview**
  > An analytics dashboard that provides branch managers with reporting across four areas: borrowing activity, inventory composition, fines, and reservations. Each section displays summary cards and data tables with support for branch and date-range filtering. All four sections offer CSV and PDF export.
  >
  > **Features**
  > - **Borrowing Statistics**
  >   - Total loans, active loans, overdue count, average loan duration, return rate
  >   - Top 5 most borrowed items and top 5 busiest borrowers
  >   - Loans per month (last 12 months)
  >   - Branch and date-range filters
  > - **Item Statistics**
  >   - Total items with breakdown by type (Books, Toys, Music)
  >   - Counts by status (On Order, Reserved, Damaged, Lost, Destroyed)
  >   - Average turnover rate, top 10 popular genres, items added per month
  >   - Branch filter
  > - **Fine Statistics**
  >   - Outstanding fine total, total collected, average fine amount
  >   - Fines per month, fines per branch, borrowers with outstanding fines
  >   - Branch and date-range filters
  > - **Reservation Statistics**
  >   - Total reservations with breakdown by status (Waiting, Ready, Collected, Cancelled, Expired)
  >   - Average wait time in days, reservations by branch, top 5 most reserved items
  >   - Branch filter
  > - **Export** - each tab includes Export CSV and Export PDF buttons; PDF reports are styled with header, metrics summary, data tables, and page numbers
  >
  > **Integration Test**
  > 1. Log in as Darwin Manager (`darwin.manager@email.com` / `Manager123!`)
  > 2. Navigate to **Statistics** - verify four tabs are displayed: Borrowing, Items, Fines, Reservations
  > 3. On the **Borrowing** tab, verify summary cards show total loans, active loans, overdue count, and return rate
  > 4. Select **Darwin** from the branch filter - verify data updates to show only Darwin branch activity
  > 5. Switch to the **Fines** tab - verify outstanding fines and borrower list are displayed
  > 6. Click **Export CSV** on the Fines tab - verify a CSV file downloads with the fines data
  > 7. Click **Export PDF** on the Fines tab - verify a styled PDF report is generated
  > 8. Switch to the **Reservations** tab - verify reservation counts by status and top reserved items are shown

### 3.4 Public Library Portal (baseline, expanded with branch filtering)

### 3.5 Self-Service Kiosk (new module)

  > **Navigation:** `/Kiosk` (standalone interface, no login required, uses dedicated kiosk layout)
  >
  > **Module Overview**
  > A public self-service interface designed for in-library touch screens. Patrons scan or type their library card to check out items, return items, and view their account. Barcode scanners act as keyboard input, so the interface works with both a scanner and the on-screen keyboard. No staff login is needed.
  >
  > **Features**
  > - **Library Card Scan** - patron scans or types their library card number to begin; unrecognised cards show a help message
  > - **Self-Service Checkout** - scan item barcodes to borrow; confirmation page shows item details and due date before finalising; collects reserved items automatically; enforces a 10-item active loan limit; blocked if account is suspended or has unpaid fines
  > - **Self-Service Returns** - scan item barcodes to return; calculates and applies late fines; promotes the next reservation in the waitlist queue; warns patron to hand the item to staff if a cross-branch reservation exists
  > - **Checkout Summary** - receipt screen showing all items checked out during the current session
  > - **My Account** - summary cards (on loan, overdue, fines owing), current loans with due dates and overdue highlighting, unpaid fines list, active reservations with queue position
  > - **Log Out** - returns to welcome screen to protect patron account
  > - **Error Handling**
  >   - "Library card not recognised. Please try again or see the front desk."
  >   - "Your account has a suspension or unpaid fine. Please see the front desk."
  >   - "You already have 10 items on loan, which is the limit."
  >   - "Item code not found. Please scan again."
  >   - "Item can't be borrowed right now. Please see the front desk."
  >   - "No active loan found for this item on your account."
  >   - "Item has a reservation at another branch. Please hand this item to staff so they can arrange the transfer."
  >   - "Item returned. A late fee has been applied."
  >
  > **Integration Test**
  > 1. Navigate to `/Kiosk` - verify the welcome screen is displayed
  > 2. Enter library card **BRW-0001** - verify the menu displays "Hi, Jane Smith" with four tiles: Borrow items, Return items, My account, Log out
  > 3. Select **Borrow items**, scan item code **BKS-0002** - verify the confirmation page shows "A Brief History of Time" with the due date
  > 4. Click **Confirm** - verify the success message appears and the item is listed under "Checked out today"
  > 5. Click **Finish borrowing** - verify the receipt screen shows the item checked out
  > 6. Navigate to **My account** - verify current loans, fines, and reservations are displayed
  > 7. Navigate to **Return items**, scan item code **BKS-0002** - verify the success message "A Brief History of Time returned successfully" appears
  > 8. Click **Log out** - verify the kiosk returns to the welcome screen

### 3.6 Public API (new module)

  > **Navigation:** `GET /api/catalogue/` (external API, no UI)
  >
  > **Module Overview**
  > A read-only REST API that exposes catalogue information for external applications. All endpoints are secured with an API key passed via the `X-API-Key` HTTP header. The API returns only available items that are not on order, keeping internal data hidden from the public response.
  >
  > **Features**
  > - **Items Endpoint** (`GET /api/catalogue/items`) - returns available catalogue items that are not on order; response includes library code, name, type (Book/Toy/Music), branch, and status
  > - **Categories Endpoint** (`GET /api/catalogue/categories`) - returns book genres, toy types, and music genres
  > - **Status Endpoint** (`GET /api/catalogue/status`) - returns the library's operating status and message from application configuration
  > - **API Key Authentication** - all requests require an `X-API-Key` header; uses SHA256 hashing with fixed-time comparison; fails closed if no key is configured
  >
  > **Integration Test**
  > 1. Configure the API key in User Secrets as `"ApiSettings:ApiKey": "LibraryApi2026"` (refer to Installation and Running Instructions manual)
  > 2. Send `GET /api/catalogue/items` with header `X-API-Key: LibraryApi2026` - verify a JSON array of available items is returned with fields: LibraryCode, Name, Type, Branch, Status
  > 3. Send `GET /api/catalogue/items` without the `X-API-Key` header - verify a 401 Unauthorized response
  > 4. Send `GET /api/catalogue/categories` with header `X-API-Key: LibraryApi2026` - verify book genres, toy types, and music genres are returned
  > 5. Send `GET /api/catalogue/status` with header `X-API-Key: LibraryApi2026` - verify the library operating status and message are returned

## 4. Multi-Branch Data and Seeding

Running `Update-Database` creates the database and seeds demonstration data across all three branches via `SeedData.cs`. No manual data entry is required. Test accounts are listed in Section 2.

### Seeded Data by Branch

| | Darwin | Sydney | Brisbane |
|---|---|---|---|
| Borrowers | 6 | 3 | 3 |
| Books | 10 | 5 | 5 |
| Toys | 11 | 4 | 4 |
| Music | 9 | 4 | 4 |
| Active Loans | 7 | 3 | 2 |
| Overdue Loans | 1 | 1 | 1 |
| Reservations | 2 | 1 | 1 |

Shared reference data is also seeded: 10 book genres, 8 toy types, 8 music genres, 5 music formats, 10 authors, and 9 artists.
