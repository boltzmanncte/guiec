
# Workflow
- Avoid installing release candidate nuget packages.
- When making changes always build the application to check it builds correctly.
- Remove unused references or usings in the file you modify.
- Prefer running single tests, and not the whole test suite, for performance.
- When adding new functionality add unit tests to test the new functionality.
- If the functionality is a UI feature add integration tests into vfv.GUIntegrationTests.
- If a new integration test is added include the decorator [Collection("WinAppDriver")] to make WinAppDriver available to the test.
- If infrastructure changes are done update the documentation accordingly.