# Grammophone.Domos.Tests.Music.Cases

Shared provider-independent test cases for the Domos music test application.

This project contains the common MSTest cases and seed data used by both EF6 and EF Core provider-specific test projects. It verifies manager access, entity access behavior, impersonation, and workflow path authorization using the fictional music domain.

## Target Frameworks

- `net472`
- `net8.0`

## Required Projects

This project expects these sibling projects to be available when building from the solution or from extracted submodules:

- `Grammophone.Domos.Tests.Music.Logic`
