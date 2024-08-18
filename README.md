# My Accounts

![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/donpotts/myaccounts/myaccounts.yml?logo=github)

Simple financial account management application.

## Overview

This repository contains an ASP.NET Core application with a Blazor WebAssembly (WASM) and MAUI UI applications in .NET 8. It also includes user authentication using ASP.NET Core 8 Identity, uses Entity Framework Core SQLite as the database, and supports OData for efficient querying.

## Features

- ASP.NET Core Kestrel web server: A robust and high-performance server.
- Blazor WASM UI: A modern web UI framework for .NET.
- Maui Blazor UI: A modern cross platform UI framework for .NET.
- MudBlazor components: Using side and top navigation.
- Swagger UI: An interactive documentation for your API.
- ASP.NET Core 8 Identity: A membership system that adds login functionality to your application.
- Entity Framework Core SQLite: A lightweight database provider for Entity Framework Core.
- OData Support: A standard for building and consuming RESTful APIs.
- Import CSV (Quicken Export All Transactions)

All Transactions Report Created: 2024-08-16 11:13:23 -0500
,
﻿Filter Criteria:,All Dates
,All Accounts
,Any Status
,All Accounts
,
﻿,"Scheduled","Split","Date","Payee","Category","Amount","Account"
﻿,,,"8/6/2024","Food Store","Groceries","-7.61","My Checking"
﻿,,"S","8/5/2024","All Stuff","Clothing","-19.71","His Debit"
﻿,,"S","8/5/2024","All Stuff","Groceries","-49.30","His Debit"
﻿,,"S","8/5/2024","All Stuff","Stupid Stuff","-5.99","His Debit"
﻿,,"S","8/5/2024","All Stuff","Cash","-100.00","His Debit"
﻿,,,"8/1/2024","Food Store","Groceries","-15.92","My Checking"
﻿,,,"8/2/2024","Food Store","Groceries","-25.92","My Checking"
﻿,,,"8/3/2024","Food Store","Groceries","-35.92","My Checking"

## Getting Started

### Prerequisites

- Visual Studio 2022
- .NET 8
- ASP.NET Core
- Blazor WASM
- Swagger UI
- ASP.NET Core 8 Identity
- MudBlazor Components
- Entity Framework Core SQLite
- OData
- MAUI

### Installation

1. Clone the repo
  git clone https://github.com/donpotts/MyAccounts.git
2. Install .NET packages
3. Install MudBlazor packages
4. Install any missing packages
5. dotnet restore
   
## API Documentation

You can access the API documentation at your Swagger UI endpoint (using `/swagger` on your application's URL).

Example:  https://myaccounts.azurewebsites.net/swagger

## Authentication

This application uses ASP.NET Core 8 Identity for user authentication. To log in, navigate to the login page and enter your credentials.

Administrator

Username:  adminUser@example.com

Password:  testUser123!

Normal user

Username:  normalUser@example.com

Password:  testUser123!

## OData Support

This application supports OData for efficient querying. You can use OData query options on the API endpoints.

## Contact

Don.Potts@DonPotts.com

