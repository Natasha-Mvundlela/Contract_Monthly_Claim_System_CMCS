# Contract Monthly Claim System (CMCS) 

## Overview 

The Contract Monthly Claim System (CMCS) is a comprehensive ASP.NET Core MVC web application designed to streamline the monthly claim submission and approval process for contract lecturers. The system provides a secure, efficient, and user-friendly platform for lecturers to submit claims while enabling Programme Coordinators and Academic Managers to manage the approval workflow effectively.

### Key Features 

### For Lecturers 
- **User Registration & Authentication** - Secure role-based login system with session management
- **Dashboard Overview** - Personalized dashboard with claim statistics and quick actions
- **Claim Submission** - Intuitive form with real-time amount calculation
- **Smart Calculation** - Automatic total amount calculation (Hours × Rate)
- **Document Management** - Secure file upload with validation (PDF, DOCX, XLSX up to 5MB)
- **Real-time Status Tracking** - Visual progress indicators and status updates
- **Claim History** - Complete history of all submitted claims

### For Programme Coordinators & Academic Managers
- **Approval Dashboard** - Centralized view of all pending claims
- **Quick Action Interface** - Streamlined approve/reject functionality
- **Detailed Claim Review** - Complete claim information with supporting documents
- **Audit Trail** - Timestamped approvals with processor information
- **Rejection Reasons** - Required comments for rejected claims
- **Role-based Access** - Different permission levels for PC vs Admin roles

## Technology Stack 

### Frontend
- **Framework**: ASP.NET Core MVC with Razor Pages
- **Styling**: Custom CSS with CSS Variables
- **JavaScript**: Vanilla JS for dynamic interactions
- **Responsive Design**: Mobile-first adaptive layouts

### Backend
- **Framework**: ASP.NET Core 6.0+
- **Database**: SQL Server LocalDB with automatic initialization
- **Authentication**: Session-based with custom authorization
- **Validation**: Data Annotations with server-side validation

### Key Libraries & Features
- **Session Management** - Secure user session handling
- **File Upload Validation** - Size and type restrictions
- **Real-time Calculations** - Client-side amount calculation
- **Progress Indicators** - Visual status tracking
- **Error Handling** - Comprehensive exception management

## System Requirements 

### Software Requirements
- **Web Browser**: Chrome 90+, Firefox 88+, Safari 14+, Edge 90+
- **.NET Runtime**: .NET Core 6.0 Runtime
- **Database**: SQL Server LocalDB
- **OS**: Windows 10/11, macOS, or Linux

### Hardware Requirements
- **RAM**: Minimum 4GB (8GB recommended)
- **Storage**: 500MB free space
- **Internet**: Stable broadband connection

## Installation & Setup

### Prerequisites
1. Install [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
2. Ensure SQL Server LocalDB is available
3. Modern web browser

### Quick Start
```bash
# Clone or download the project
git clone <repository-url>

# Navigate to project directory
cd ContractMonthlyClaimSystem

# Build the application
dotnet build

# Run the application
dotnet run

# Access the application
# Open browser and navigate to: https://localhost:7000 (or the port shown in console)
```

### Database Setup
The system automatically:
- Creates LocalDB instance `claim_system`
- Initializes database `contract_claims_database`
- Creates necessary tables (Users, Claims)
- Sets up default schema and relationships

## Usage Guide 

### For Lecturers

#### 1. Registration & Setup
1. Navigate to the Home page
2. Click "Register" and fill in:
   - Full Name
   - Institutional Email Address
   - Secure Password
   - Role: Select "Lecturer"
3. Submit registration

#### 2. Submitting Claims
1. **Login** with your credentials
2. **Navigate** to "Submit Claim" from dashboard
3. **Fill** the claim form:
   - Select Faculty and Module
   - Enter Hours Worked (1-200 hours)
   - Enter Hourly Rate (R1-R1000)
   - Upload supporting documents (optional)
4. **Review** automatic calculation
5. **Submit** claim for approval

#### 3. Tracking Claims
1. View "Claim Status" for all submissions
2. Monitor real-time status:
   - **Pending** - Under review
   - **Approved** - Claim approved
   - **Rejected** - Check rejection reason
3. Download supporting documents as needed

### For Programme Coordinators & Academic Managers

#### 1. Accessing Approval System
1. **Register/Login** with PC or Admin role
2. **Automatic redirect** to Approval dashboard
3. **View** all pending claims in organized cards

#### 2. Reviewing Claims
1. **Examine** each claim's details:
   - Lecturer information
   - Hours and rate breakdown
   - Calculated total amount
   - Supporting documents
   - Submission timestamp

#### 3. Processing Claims
1. **Approve Claim** - Single-click approval
2. **Reject Claim** - Provide mandatory reason
3. **Track** processed claims in system

## Security Features

- **Session-based Authentication** - Secure user sessions
- **Role-based Access Control** - Different interfaces per role
- **Input Validation** - Server and client-side validation
- **SQL Injection Protection** - Parameterized queries
- **File Upload Security** - Type and size restrictions
- **XSS Protection** - Built-in ASP.NET Core protections

## Database Schema

### Users Table
- UserID (Primary Key)
- Full_Name
- Email_Address (Unique)
- Password
- Role (Lecturer/PC/Admin)

### Claims Table
- ClaimID (Primary Key)
- LecturerID (Foreign Key)
- Email_Address
- Claim_Date
- Faculty, Module
- Hours_Worked, Hourly_Rate, Calculated_Amount
- Supporting_Documents
- Status (Pending/Approved/Rejected)
- RejectionReason
- SubmittedDate, ProcessedDate, ProcessedBy

## Testing

### Unit Tests
```bash
# Run comprehensive test suite
dotnet test

# Test specific areas
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"
```

### Manual Testing Checklist
1. User Registration & Login
2. Role-based Redirects
3. Claim Submission & Calculation
4. File Upload Validation
5. Approval Workflow
6. Status Tracking
7. Error Handling
8. Responsive Design

## Troubleshooting

### Common Issues

**Session Errors**
- Ensure `AddDistributedMemoryCache()` is in Program.cs
- Verify session middleware order

**Database Connection**
- Check LocalDB installation
- Verify instance name matches configuration

**File Upload Issues**
- Ensure file size < 5MB
- Check file type (PDF, DOCX, XLSX only)

**Login Problems**
- Verify email format
- Check role selection matches registration

### Support
For technical issues, check:
1. Application logs in console
2. Browser developer tools
3. Database connection status

## Version Control

The project maintains comprehensive commit history:
1. Foundation & Setup
2. Authentication & Security  
3. Core Features Implementation
4. User Experience Enhancements
5. Testing & Validation

---

**Version**: 2.0  
**Last Updated**: 2025  
**Compatibility**: .NET 6.0+
