# Grammophone.Domos.Tests.Music.Cases

Shared provider-independent test cases for the Domos music test application.

This project contains the common MSTest cases and seed data used by both EF6 and EF Core provider-specific test projects. It verifies manager access, entity access behavior, impersonation, and workflow path authorization using the fictional music domain.

## Target Frameworks

- `net472`
- `net8.0`

## Required Projects

This project expects these sibling projects to be available when building from the solution or from extracted submodules:

Direct project references:

- `Grammophone.Domos.Tests.Music.Logic`

Additional transitive project references:

- `Grammophone.Caching`
- `Grammophone.Configuration`
- `Grammophone.DataAccess`
- `Grammophone.Domos.AccessChecking`
- `Grammophone.Domos.Accounting`
- `Grammophone.Domos.DataAccess`
- `Grammophone.Domos.Domain`
- `Grammophone.Domos.Environment`
- `Grammophone.Domos.Logic`
- `Grammophone.Domos.Tests.Music.DataAccess`
- `Grammophone.Domos.Tests.Music.Domain`
- `Grammophone.Email`
- `Grammophone.GenericContentModel`
- `Grammophone.Logging`
- `Grammophone.Serialization`
- `Grammophone.Setup`
- `Grammophone.Storage`
- `Grammophone.Tasks`
- `Grammophone.TemplateRendering`
