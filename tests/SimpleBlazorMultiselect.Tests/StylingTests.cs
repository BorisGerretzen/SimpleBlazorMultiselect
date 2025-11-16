using Bunit;
using FluentAssertions;
using Xunit;

namespace SimpleBlazorMultiselect.Tests;

public class StylingTests : BaseTest
{
    [Fact]
    public void Component_AppliesCustomCssClasses_WhenProvided()
    {
        var component = RenderComponent<SimpleMultiselect<string>>(parameters => parameters
            .Add(p => p.Options, TestOptions)
            .Add(p => p.Class, "custom-class"));

        var container = component.Find(".simple-dropdown");
        container.ClassList.Should().Contain("custom-class");
    }

    [Fact]
    public void Component_AppliesCustomStyles_WhenProvided()
    {
        var component = RenderComponent<SimpleMultiselect<string>>(parameters => parameters
            .Add(p => p.Options, TestOptions)
            .Add(p => p.Style, "width: 300px;"));

        var container = component.Find(".simple-dropdown");
        container.GetAttribute("style").Should().Contain("width: 300px;");
    }

    [Fact]
    public void Component_SetsButtonId_WhenProvided()
    {
        var component = RenderComponent<SimpleMultiselect<string>>(parameters => parameters
            .Add(p => p.Options, TestOptions)
            .Add(p => p.Id, "test-multiselect"));

        var button = component.Find("button");
        button.Id.Should().Be("test-multiselect");
    }

    [Fact]
    public void Component_ShowsStandaloneStyles_WhenStandaloneIsTrue()
    {
        var component = RenderComponent<SimpleMultiselect<string>>(parameters => parameters
            .Add(p => p.Options, TestOptions)
            .Add(p => p.Standalone, true));

        var container = component.Find(".simple-dropdown");
        container.ClassList.Should().Contain("simple-bs-compat");
    }
}