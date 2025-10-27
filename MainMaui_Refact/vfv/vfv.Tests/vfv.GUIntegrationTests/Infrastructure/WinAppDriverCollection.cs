namespace vfv.GUIntegrationTests.Infrastructure;

/// <summary>
/// Collection definition for tests that require WinAppDriver
/// </summary>
[CollectionDefinition("WinAppDriver")]
public class WinAppDriverCollection : ICollectionFixture<WinAppDriverFixture>
{
    // This class has no code, and is never instantiated.
    // Its purpose is to be the place to apply [CollectionDefinition] and the ICollectionFixture<> interface.
}
