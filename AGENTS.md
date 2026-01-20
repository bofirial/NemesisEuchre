# Claude AI Assistant Instructions

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

NemesisEuchre is a console-based engine for training a high-performance Euchre AI using massive dataset seeding and genetic algorithms. The project is in early development with infrastructure in place but core game logic not yet implemented.

## Operating Principles
- Secure-by-default: least privilege, defense-in-depth, automation over heroics.
- Reliability: design for failure; graceful degradation; error budgets and SLOs.
- Evidence-driven: propose options with tradeoffs; quantify impact, cost, risk.
- Simplicity: choose the simplest design that scales 10–100x; avoid premature complexity.

## How to Collaborate
- Ask 3–7 crisp clarifying questions when requirements are ambiguous.
- State assumptions explicitly; document risks and mitigations.
- Offer 2–3 viable options with tradeoffs and a clear recommendation.
- Provide runnable examples, scaffolds, and checklists; keep outputs concise and skimmable.

## Default Output Format
- Start with a one-paragraph summary and a bullet list of key decisions.
- Use sections, short lists, and code fences. Include diagrams in Mermaid where helpful.
- Provide acceptance criteria and Definition of Done for each deliverable.
- End with next 3–5 high-impact actions and owners/placeholders.

## Build & Development Commands

```bash
# Restore dependencies
dotnet restore

# Build (Debug)
dotnet build

# Build (Release)
dotnet build --configuration Release

# Run the application
dotnet run --project NemesisEuchre.Console

# Run tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Verify formatting without changes
dotnet format --verify-no-changes
```

## Code Quality & Standards

- Follow the project's coding standards
- Implement proper error handling
- Write clear, maintainable code
- Include appropriate comments and documentation

### Comments Policy
- **Do NOT add unnecessary comments** - write self-documenting code
- **NEVER add single-line comments that describe what the code obviously does**
- Only include comments for:
    - Complex business logic that isn't obvious
    - Non-obvious algorithms or workarounds
    - TODO/FIXME with ticket references
- If the code needs a comment to be understood, refactor the code to be clearer instead

### Testing

- **Framework**: xUnit
- **Assertions**: FluentAssertions (preferred over xUnit assertions)
- **Mocking**: Moq
- **Test data generation**: Bogus
- **Coverage**: Coverlet (collected automatically in CI)

Tests should be in `NemesisEuchre.Console.Tests` with the pattern `*Tests.cs`.

## Project Structure Conventions

- **File organization**: One class per file, filename matches class name

## Future Development Notes

This project is designed for AI/ML workloads

