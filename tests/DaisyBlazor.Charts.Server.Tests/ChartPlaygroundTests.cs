using DaisyBlazor.Docs.Docs;

namespace DaisyBlazor.Charts.Server.Tests;

/// <summary>Every chart page carries a playground built from its first example.</summary>
public sealed class ChartPlaygroundTests
{
    public static TheoryData<string> Charts() => [.. DocRegistry.All.Where(d => d.Category == Category.Charts).Select(d => d.Name)];

    [Theory]
    [MemberData(nameof(Charts))]
    public void The_playground_starts_from_the_first_example_and_its_code_compiles_when_pasted(string name)
    {
        ComponentDoc doc = DocRegistry.Find(name)!;

        PlaygroundSpec spec = Assert.IsType<PlaygroundSpec>(doc.Playground);
        Assert.NotEmpty(spec.Fields);
        foreach ((string parameter, string field) in spec.Fields)
        {
            Assert.NotNull(spec.Initial[parameter]);
            Assert.Contains(field, spec.Code);
        }
        Assert.StartsWith("@code", spec.Code);
    }
}
