# FindThatBook

**AI-Powered Book Search** - InfoTrack Technical Assessment

## Overview

FindThatBook is a .NET 9 Web API that uses AI (Google Gemini) to extract structured information from "dirty" book search queries and matches them against the Open Library API. It returns up to 5 book candidates with detailed explanations of why each book matched.

## How It Works

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           APPLICATION FLOW                                   │
└─────────────────────────────────────────────────────────────────────────────┘

  User Input                    AI Processing                 Book Search
  ──────────                    ─────────────                 ───────────
       │                              │                            │
       ▼                              ▼                            ▼
┌─────────────┐    ┌─────────────────────────────┐    ┌─────────────────────┐
│ "mark       │───▶│   Google Gemini AI          │───▶│   Open Library API  │
│ huckleberry │    │   Extracts:                 │    │   Searches for:     │
│ 1884"       │    │   - Title: Huckleberry Finn │    │   - Matching books  │
│             │    │   - Author: Mark Twain      │    │   - Cover images    │
│             │    │   - Year: 1884              │    │   - Metadata        │
└─────────────┘    └─────────────────────────────┘    └─────────────────────┘
                                                               │
                                                               ▼
                              ┌─────────────────────────────────────────────┐
                              │         Matching Algorithm                   │
                              │  Scores each result by:                      │
                              │  - Title similarity (40%)                    │
                              │  - Author similarity (35%)                   │
                              │  - Keywords match (15%)                      │
                              │  - Year match (10%)                          │
                              └─────────────────────────────────────────────┘
                                                               │
                                                               ▼
                              ┌─────────────────────────────────────────────┐
                              │              Response                        │
                              │  Returns up to 5 candidates with:            │
                              │  - Match strength (Exact/Strong/Partial)     │
                              │  - Match score (0-100)                        │
                              │  - Explanation of why it matched             │
                              └─────────────────────────────────────────────┘
```

## Features

- **AI-Powered Query Extraction**: Uses Google Gemini to extract title, author, year, and keywords from unstructured queries
- **Open Library Integration**: Searches the Open Library API for matching books
- **Smart Matching Algorithm**: Applies a hierarchical matching system (Exact > Strong > Partial > Weak)
- **Match Explanations**: Provides detailed explanations for why each book was matched
- **Simple Web UI**: HTML/Tailwind frontend for easy testing

## Architecture

```
FindThatBook/
├── src/
│   ├── FindThatBook.Domain/          # Entities, Interfaces, Enums, Value Objects
│   ├── FindThatBook.Application/     # DTOs, Services, Use Cases
│   ├── FindThatBook.Infrastructure/  # External API integrations (Gemini, Open Library)
│   └── FindThatBook.API/             # Controllers, Configuration, wwwroot
└── tests/
    └── FindThatBook.Tests/           # Unit tests with xUnit
```

### Clean Architecture Layers

1. **Domain Layer**: Core business entities and interfaces
   - `Book`, `BookCandidate` entities
   - `ExtractedBookInfo` value object
   - `MatchStrength` enum
   - Service interfaces

2. **Application Layer**: Business logic and orchestration
   - `SearchBooksUseCase`: Main use case orchestrating the search flow
   - `BookMatchingService`: Implements matching algorithm
   - DTOs for API communication

3. **Infrastructure Layer**: External service integrations
   - `GeminiExtractionService`: Gemini AI integration for query extraction
   - `OpenLibrarySearchService`: Open Library API integration

4. **API Layer**: Web API and frontend
   - `BooksController`: REST API endpoints
   - Static HTML/Tailwind frontend

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Google Gemini API Key (free tier available at [Google AI Studio](https://aistudio.google.com/))

## Quick Start

### 1. Clone the Repository

```bash
git clone https://github.com/julianrozo77-sketch/findthatbook.git
cd findthatbook
```

### 2. Configure Gemini API Key

You need a Google Gemini API key to run this application. Get one free at [Google AI Studio](https://aistudio.google.com/).

**Option A: User Secrets (Recommended for Development)**

```bash
cd src/FindThatBook.API
dotnet user-secrets init
dotnet user-secrets set "Gemini:ApiKey" "YOUR_API_KEY_HERE"
```

**Option B: Environment Variable**

Windows (CMD):
```cmd
set Gemini__ApiKey=YOUR_API_KEY_HERE
```

Windows (PowerShell):
```powershell
$env:Gemini__ApiKey="YOUR_API_KEY_HERE"
```

Linux/Mac:
```bash
export Gemini__ApiKey=YOUR_API_KEY_HERE
```

**Option C: Edit Configuration File**

> **Note**: Only use this for local testing. Never commit API keys to version control.

Edit `src/FindThatBook.API/appsettings.Development.json`:
```json
{
  "Gemini": {
    "ApiKey": "YOUR_API_KEY_HERE"
  }
}
```

### 3. Build and Run

```bash
# From the root directory
dotnet build

# Run the application
cd src/FindThatBook.API
dotnet run
```

### 4. Access the Application

Once running, open your browser:

| URL | Description |
|-----|-------------|
| http://localhost:5026 | Web UI - Interactive search interface |
| http://localhost:5026/swagger | Swagger - API documentation |
| https://localhost:7111 | HTTPS endpoint |

## API Usage

### Search Books Endpoint

**POST** `/api/books/search`

**Request:**
```json
{
  "query": "tolkien hobbit illustrated 1937",
  "maxResults": 5
}
```

**Response:**
```json
{
  "originalQuery": "tolkien hobbit illustrated 1937",
  "extractedInfo": {
    "title": "The Hobbit",
    "author": "J.R.R. Tolkien",
    "keywords": ["illustrated"],
    "year": 1937
  },
  "candidates": [
    {
      "key": "/works/OL262758W",
      "title": "The Hobbit",
      "authors": ["J.R.R. Tolkien"],
      "firstPublishYear": 1937,
      "coverUrl": "https://covers.openlibrary.org/b/id/...",
      "openLibraryUrl": "https://openlibrary.org/works/OL262758W",
      "matchStrength": "Exact",
      "matchScore": 95.5,
      "matchExplanation": "Exact match: Title exact match; Author exact match; Year exact match",
      "matchReasons": [
        "Title exact match: \"The Hobbit\"",
        "Author exact match: \"J.R.R. Tolkien\"",
        "Year exact match: 1937"
      ]
    }
  ],
  "totalCandidates": 1,
  "processingTime": "00:00:01.234"
}
```

### cURL Example

```bash
curl -X POST http://localhost:5026/api/books/search \
  -H "Content-Type: application/json" \
  -d '{"query": "orwell 1984", "maxResults": 5}'
```

## Example Queries

The AI can understand various "dirty" queries and extract structured information:

| Dirty Query | Extracted Info |
|------------|----------------|
| `"mark huckleberry"` | Title: The Adventures of Huckleberry Finn, Author: Mark Twain |
| `"tolkien hobbit illustrated 1937"` | Title: The Hobbit, Author: J.R.R. Tolkien, Year: 1937, Keywords: illustrated |
| `"orwell 1984"` | Title: 1984, Author: George Orwell |
| `"rowling philosopher stone"` | Title: Harry Potter and the Philosopher's Stone, Author: J.K. Rowling |
| `"that book about the whale moby"` | Title: Moby Dick, Author: Herman Melville |

## Matching Algorithm

The matching system uses a weighted scoring approach:

| Component | Weight | Description |
|-----------|--------|-------------|
| Title | 40% | How closely the book title matches |
| Author | 35% | How closely the author name matches |
| Keywords | 15% | Presence of keywords in book metadata |
| Year | 10% | How close the publication year is |

### Match Strength Levels

| Level | Criteria |
|-------|----------|
| **Exact** | Both title and author match exactly (95%+ similarity) |
| **Strong** | High confidence on both title and author (70%+ on both) |
| **Partial** | Good match on either title OR author (60%+) |
| **Weak** | Some keywords or partial matches found |

## Testing

The project includes comprehensive unit tests:

```bash
# Run all tests
dotnet test

# Run with verbose output
dotnet test --verbosity normal

# Run specific test class
dotnet test --filter "FullyQualifiedName~BookMatchingServiceTests"
```

### Test Coverage

| Test Class | Coverage |
|------------|----------|
| `BookMatchingServiceTests` | Matching algorithm logic |
| `SearchBooksUseCaseTests` | Main use case orchestration |
| `BookCandidateTests` | Domain entity validation |
| `ExtractedBookInfoTests` | Value object behavior |
| `GeminiExtractionServiceTests` | AI service integration |
| `OpenLibrarySearchServiceTests` | Book search integration |

## Configuration Reference

### appsettings.json

```json
{
  "Gemini": {
    "ApiKey": "",
    "Model": "gemini-2.5-flash",
    "BaseUrl": "https://generativelanguage.googleapis.com/v1beta"
  },
  "OpenLibrary": {
    "BaseUrl": "https://openlibrary.org",
    "TimeoutSeconds": 30
  }
}
```

| Setting | Description | Default |
|---------|-------------|---------|
| `Gemini:ApiKey` | Your Google Gemini API key | (required) |
| `Gemini:Model` | Gemini model to use | `gemini-2.5-flash` |
| `OpenLibrary:BaseUrl` | Open Library API URL | `https://openlibrary.org` |
| `OpenLibrary:TimeoutSeconds` | API timeout in seconds | `30` |

## Technologies Used

- **.NET 9** - Latest .NET framework
- **ASP.NET Core Web API** - REST API framework
- **Google Gemini AI** - AI model for query extraction (gemini-2.5-flash)
- **Open Library API** - Free book database API
- **xUnit + Moq + FluentAssertions** - Testing framework
- **Tailwind CSS** - Frontend styling (via CDN)

## Troubleshooting

### Common Issues

**"API key not configured" error**
- Ensure you've set the Gemini API key using one of the methods in Step 2
- If using environment variables, restart your terminal after setting them

**"Connection refused" error**
- Make sure no other application is using ports 5026 or 7111
- Try running with `dotnet run --urls "http://localhost:5000"`

**Gemini API rate limiting**
- The free tier has rate limits; wait a moment and try again
- Consider upgrading to a paid tier for production use

## License

This project was created as a technical assessment for InfoTrack.
