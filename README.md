# CCSWebKySearch

The CCSWebKySearch API is a .NET Core-based API for searching and retrieving document information from a MySQL database. It provides endpoints for checking the service's live status, retrieving notebook entries, searching documents by book and page, kind, name, and fetching document files in PDF and TIFF formats.


## Prerequisites

- .NET SDK (version 6.0 or higher)
- MySQL
- Visual Studio (recommended) or any other IDE

## Getting Started

```bash
git clone https://github.com/yourusername/CCSWebKySearch.git
cd CCSWebKySearch
```

## Configuration
Copy and paste the .env_example file and name it .env, configure the variables as needed.
Example:

```bash
CONNECTION_STRING="Server=localhost;Database=db;User=xxxx;Password=xxxx;"
DOCUMENTS_PATH="C:\\Path"
SEQ_SERVER_URL="http://localhost:5341"
```

## Usage
Run the API:
```bash
dotnet run
```

Open your browser and navigate to https://localhost:7087/swagger/index.html
