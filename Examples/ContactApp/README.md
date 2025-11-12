# Contact App - Supabase + Oxpecker + HTMX Example

This is an F# web application demonstrating the use of [Oxpecker](https://github.com/Lanayx/Oxpecker) web framework with [HTMX](https://htmx.org/) for dynamic interactions, integrated with [Supabase](https://supabase.com/) for backend data storage.

## About

This example is adapted from the [ContactApp example](https://github.com/Lanayx/Oxpecker/tree/develop/examples/ContactApp) in the Oxpecker repository, which is an F# version of the contact app presented in the [Hypermedia Systems](https://hypermedia.systems/) book.

**Key Features:**
- Server-side rendering with Oxpecker's ViewEngine
- Dynamic interactions using HTMX (no JavaScript!)
- Form validation with Oxpecker.ModelValidation
- Supabase integration for persistent data storage
- Contact archiving to CSV/ZIP
- Search and pagination
- Inline editing and deletion

## Prerequisites

1. **.NET 9.0 SDK** or later
2. **Supabase Project** - Create a free project at [supabase.com](https://supabase.com/)
3. **Database Setup** - Run the SQL migration below to create the contacts table

### Database Schema

Execute this SQL in your Supabase SQL Editor:

```sql
-- Create contacts table
CREATE TABLE contacts (
    id SERIAL PRIMARY KEY,
    first VARCHAR(100) NOT NULL,
    last VARCHAR(100) NOT NULL,
    phone VARCHAR(20) NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE
);

-- Enable Row Level Security (RLS)
ALTER TABLE contacts ENABLE ROW LEVEL SECURITY;

-- Create a policy to allow all operations (adjust as needed for production)
CREATE POLICY "Allow all operations on contacts" ON contacts
    FOR ALL
    USING (true)
    WITH CHECK (true);

-- Insert some sample data
INSERT INTO contacts (first, last, email, phone) VALUES
    ('John', 'Smith', 'john@example.com', '123-456-7890'),
    ('Dana', 'Crandith', 'dcran@example.com', '123-456-7891'),
    ('Edith', 'Neutvaar', 'en@example.com', '123-456-7892'),
    ('Alice', 'Johnson', 'alice@example.com', '123-456-7893'),
    ('Bob', 'Williams', 'bob@example.com', '123-456-7894');
```

## Configuration

Set your Supabase credentials as environment variables:

```bash
export SUPABASE_URL="https://your-project.supabase.co"
export SUPABASE_KEY="your-anon-key"
```

Or create a `.env` file (not recommended for production):

```
SUPABASE_URL=https://your-project.supabase.co
SUPABASE_KEY=your-anon-key
```

You can find these values in your Supabase project dashboard:
- Go to **Project Settings** → **API**
- Copy the **Project URL** (SUPABASE_URL)
- Copy the **anon public** key (SUPABASE_KEY)

## Running the Application

```bash
# Navigate to the ContactApp directory
cd Examples/ContactApp

# Run the application
dotnet run

# Open your browser to
# http://localhost:5000
```

## Project Structure

```
ContactApp/
├── Models.fs                  # Domain models and DTOs
├── ContactService.fs          # Supabase data access layer
├── Tools.fs                   # Helper functions and archiver
├── Handlers.fs                # HTTP endpoint handlers
├── Program.fs                 # Application entry point
├── templates/
│   ├── shared/
│   │   ├── layout.fs         # Main layout template
│   │   ├── errors.fs         # Error display helpers
│   │   └── contactFields.fs  # Reusable form fields
│   ├── index.fs              # Contact list view
│   ├── show.fs               # Contact detail view
│   ├── edit.fs               # Contact edit form
│   └── new.fs                # New contact form
├── wwwroot/
│   ├── site.css              # Stylesheet
│   └── spinning-circles.svg  # Loading indicator
└── README.md
```

## Key Technologies

- **[Oxpecker](https://github.com/Lanayx/Oxpecker)** - F# web framework
- **[Oxpecker.ViewEngine](https://github.com/Lanayx/Oxpecker)** - Type-safe HTML generation
- **[Oxpecker.HTMX](https://github.com/Lanayx/Oxpecker)** - HTMX integration for F#
- **[HTMX](https://htmx.org/)** - High-power tools for HTML
- **[Supabase](https://supabase.com/)** - Open source Firebase alternative
- **[Supabase.FSharp](../../Supabase.FSharp)** - Idiomatic F# wrapper for Supabase

## Features Demonstrated

### 1. **HTMX Integration**
   - Progressive search with live updates
   - Infinite scroll pagination
   - Inline deletions with confirmations
   - Progress indicators

### 2. **Model Validation**
   - Server-side form validation
   - Email format validation
   - Unique email constraint
   - Real-time email validation via HTMX

### 3. **Supabase Integration**
   - CRUD operations
   - Pagination with range queries
   - Search functionality
   - Async/Task-based data access

### 4. **F# Best Practices**
   - Type-safe HTML generation
   - Computation expressions for configuration
   - F# Async workflows
   - Idiomatic F# patterns

## Learning Resources

- [Hypermedia Systems Book](https://hypermedia.systems/) - The book this example is based on
- [Oxpecker Documentation](https://github.com/Lanayx/Oxpecker)
- [HTMX Documentation](https://htmx.org/docs/)
- [Supabase Documentation](https://supabase.com/docs)
- [Supabase F# Guide](../../Documentation/FSharp.md)

## License

This example is provided as-is for educational purposes.
