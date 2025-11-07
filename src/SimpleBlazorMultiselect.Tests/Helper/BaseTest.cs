using Bunit;
using FluentAssertions;

namespace SimpleBlazorMultiselect.Tests;

public class BaseTest : TestContext
{
    protected readonly List<string> TestOptions =
    [
        "Apple",
        "Banana",
        "Cherry",
        "Date",
        "Elderberry"
    ];

    public BaseTest()
    {
        JSInterop.SetupModule("./_content/SimpleBlazorMultiselect/js/simpleMultiselect.js")
            .SetupModule("register", invocation => invocation.Arguments.Count == 2)
            .SetupVoid("dispose");
    }

    protected static void AssertOpen(IRenderedComponent<SimpleMultiselect<string>> component, int? expectedItemCount = null)
    {
        var dropdown = component.Find(".dropdown-menu.show");
        dropdown.Should().NotBeNull();

        if (expectedItemCount.HasValue)
        {
            var dropdownItems = component.FindAll(".dropdown-item");
            dropdownItems.Should().HaveCount(expectedItemCount.Value);
        }
    }

    protected static void AssertClosed(IRenderedComponent<SimpleMultiselect<string>> component)
    {
        var dropdown = component.FindAll(".dropdown-menu.show");
        dropdown.Should().BeEmpty();

        var dropdownItems = component.FindAll(".dropdown-item");
        dropdownItems.Should().HaveCount(0);
    }
}