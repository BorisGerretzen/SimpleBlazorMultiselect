using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace SimpleBlazorMultiselect.Tests;

public class SimpleMultiselectTests : BaseTest
{
    [Fact]
    public void Component_RendersWithDefaultText_WhenNoOptionsSelected()
    {
        var component = RenderComponent<SimpleMultiselect<string>>(parameters => parameters
            .Add(p => p.Options, TestOptions)
            .Add(p => p.DefaultText, "Choose items"));

        var button = component.Find("button");
        button.TextContent.Should().Contain("Choose items");
    }

    [Fact]
    public void Component_RendersSelectedOptions_WhenOptionsAreSelected()
    {
        var selectedOptions = new HashSet<string> { "Apple", "Banana" };

        var component = RenderComponent<SimpleMultiselect<string>>(parameters => parameters
            .Add(p => p.Options, TestOptions)
            .Add(p => p.SelectedOptions, selectedOptions));

        var button = component.Find("button");
        button.TextContent.Should().Contain("Apple, Banana");
    }

    [Fact]
    public void Component_TogglesDropdown_WhenButtonClicked()
    {
        var component = RenderComponent<SimpleMultiselect<string>>(parameters => parameters
            .Add(p => p.Options, TestOptions));

        var button = component.Find("button");
        button.Click();

        AssertOpen(component);
    }

    [Fact]
    public void Component_ShowsAllOptions_WhenDropdownIsOpen()
    {
        var component = RenderComponent<SimpleMultiselect<string>>(parameters => parameters
            .Add(p => p.Options, TestOptions));

        var button = component.Find("button");
        button.Click();

        var dropdownItems = component.FindAll(".dropdown-item");
        dropdownItems.Should().HaveCount(TestOptions.Count);

        foreach (var option in TestOptions)
        {
            component.Markup.Should().Contain(option);
        }
    }

    [Fact]
    public void Component_SelectsOption_WhenOptionClicked()
    {
        var selectedOptions = new HashSet<string>();
        var component = RenderComponent<SimpleMultiselect<string>>(parameters => parameters
            .Add(p => p.Options, TestOptions)
            .Add(p => p.SelectedOptions, selectedOptions)
            .Add(p => p.SelectedOptionsChanged, EventCallback.Factory.Create<HashSet<string>>(this, newSelection => { selectedOptions = newSelection; })));

        var button = component.Find("button");
        button.Click();

        var firstOption = component.FindAll(".dropdown-item")[0];
        firstOption.Click();

        firstOption = component.FindAll(".dropdown-item")[0];
        var checkbox = firstOption.QuerySelector<IHtmlInputElement>("input[type='checkbox']");
        checkbox.Should().NotBeNull();
        checkbox.IsChecked.Should().BeTrue();
        selectedOptions.Should().Contain("Apple");
    }

    [Fact]
    public void Component_DeselectsOption_WhenSelectedOptionClicked()
    {
        var selectedOptions = new HashSet<string> { "Apple" };
        var component = RenderComponent<SimpleMultiselect<string>>(parameters => parameters
            .Add(p => p.Options, TestOptions)
            .Add(p => p.SelectedOptions, selectedOptions)
            .Add(p => p.SelectedOptionsChanged, EventCallback.Factory.Create<HashSet<string>>(this, newSelection => { selectedOptions = newSelection; })));

        var button = component.Find("button");
        button.Click();

        var firstOption = component.FindAll(".dropdown-item")[0];
        firstOption.Click();

        selectedOptions.Should().NotContain("Apple");
        foreach (var option in component.FindAll(".dropdown-item"))
        {
            var cb = option.QuerySelector<IHtmlInputElement>("input[type='checkbox']");
            cb!.IsChecked.Should().BeFalse();
        }
    }

    [Fact]
    public void Component_ShowsFilterInput_WhenCanFilterIsTrue()
    {
        var component = RenderComponent<SimpleMultiselect<string>>(parameters => parameters
            .Add(p => p.Options, TestOptions)
            .Add(p => p.CanFilter, true));

        var button = component.Find("button");
        button.Click();

        var filterInput = component.Find(".simple-filter-input");
        filterInput.Should().NotBeNull();
        filterInput.GetAttribute("placeholder").Should().Be("Filter...");
    }

    [Fact]
    public void Component_FiltersOptions_WhenFilterTextEntered()
    {
        var component = RenderComponent<SimpleMultiselect<string>>(parameters => parameters
            .Add(p => p.Options, TestOptions)
            .Add(p => p.CanFilter, true));

        var button = component.Find("button");
        button.Click();

        var filterInput = component.Find(".simple-filter-input");
        filterInput.Input("App");

        var dropdownItems = component.FindAll(".dropdown-item");
        dropdownItems.Should().HaveCount(1);
        dropdownItems[0].TextContent.Should().Contain("Apple");
    }

    [Fact]
    public void Component_UsesCustomStringSelector_WhenProvided()
    {
        var complexOptions = new List<TestValueItem>
        {
            new("1", "Apple"),
            new("2", "Banana")
        };

        var component = RenderComponent<SimpleMultiselect<TestValueItem>>(parameters => parameters
            .Add(p => p.Options, complexOptions)
            .Add(p => p.StringSelector, item => item.Name));

        var button = component.Find("button");
        button.Click();

        component.Markup.Should().Contain("Apple");
        component.Markup.Should().Contain("Banana");
    }

    [Fact]
    public void Component_UsesCustomFilterPredicate_WhenProvided()
    {
        var component = RenderComponent<SimpleMultiselect<string>>(parameters => parameters
            .Add(p => p.Options, TestOptions)
            .Add(p => p.CanFilter, true)
            .Add(p => p.FilterPredicate, (item, filter) => item.StartsWith(filter, StringComparison.OrdinalIgnoreCase)));

        var button = component.Find("button");
        button.Click();

        var filterInput = component.Find(".simple-filter-input");
        filterInput.Input("B");

        var dropdownItems = component.FindAll(".dropdown-item");
        dropdownItems.Should().HaveCount(1);
        dropdownItems[0].TextContent.Should().Contain("Banana");
    }

    [Fact]
    public void Component_HandlesEmptyOptions_Gracefully()
    {
        var component = RenderComponent<SimpleMultiselect<string>>(parameters => parameters
            .Add(p => p.Options, new List<string>()));

        var button = component.Find("button");
        button.Click();

        var dropdownItems = component.FindAll(".dropdown-item");
        dropdownItems.Should().BeEmpty();
    }

    [Fact]
    public void Component_SingleSelectMode_SelectsOnlyOneOption()
    {
        var selectedOptions = new HashSet<string>();
        var component = RenderComponent<SimpleMultiselect<string>>(parameters => parameters
            .Add(p => p.Options, TestOptions)
            .Add(p => p.SelectedOptions, selectedOptions)
            .Add(p => p.IsMultiSelect, false)
            .Add(p => p.SelectedOptionsChanged, EventCallback.Factory.Create<HashSet<string>>(this, newSelection => { selectedOptions = newSelection; })));

        var button = component.Find("button");
        button.Click();

        var firstOption = component.FindAll(".dropdown-item")[0];
        firstOption.Click();
        component.Render();

        JSInterop.VerifyInvoke("dispose");
        component.Instance.IsDropdownOpen.Should().BeFalse();

        button.Click();
        var secondOption = component.FindAll(".dropdown-item")[1];
        secondOption.Click();

        selectedOptions.Should().HaveCount(1);
        selectedOptions.Should().Contain("Banana");
        selectedOptions.Should().NotContain("Apple");
    }

    [Fact]
    public void Component_CachesFilteredOptions_ForPerformance()
    {
        var component = RenderComponent<SimpleMultiselect<string>>(parameters => parameters
            .Add(p => p.Options, TestOptions)
            .Add(p => p.CanFilter, true));

        var button = component.Find("button");
        button.Click();

        var filterInput = component.Find(".simple-filter-input");
        filterInput.Input("Appl");

        // Trigger multiple renders without changing filter
        component.Render();
        component.Render();

        var dropdownItems = component.FindAll(".dropdown-item");
        dropdownItems.Should().HaveCount(1);
        dropdownItems[0].TextContent.Should().Contain("Apple");
    }
}