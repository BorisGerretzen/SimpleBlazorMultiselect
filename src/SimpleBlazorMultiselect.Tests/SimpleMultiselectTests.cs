using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Xunit;

namespace SimpleBlazorMultiselect.Tests;

public class SimpleMultiselectTests : TestContext
{
    private readonly List<string> _testOptions = new() { "Apple", "Banana", "Cherry", "Date", "Elderberry" };

    public SimpleMultiselectTests()
    {
        JSInterop.SetupModule("./_content/SimpleBlazorMultiselect/js/simpleMultiselect.js")
            .SetupModule("register", invocation => invocation.Arguments.Count == 2)
            .SetupVoid("dispose");
    }

    [Fact]
    public void Component_RendersWithDefaultText_WhenNoOptionsSelected()
    {
        var component = RenderComponent<SimpleMultiselect<string>>(parameters => parameters
            .Add(p => p.Options, _testOptions)
            .Add(p => p.DefaultText, "Choose items"));

        var button = component.Find("button");
        button.TextContent.Should().Contain("Choose items");
    }

    [Fact]
    public void Component_RendersSelectedOptions_WhenOptionsAreSelected()
    {
        var selectedOptions = new HashSet<string> { "Apple", "Banana" };

        var component = RenderComponent<SimpleMultiselect<string>>(parameters => parameters
            .Add(p => p.Options, _testOptions)
            .Add(p => p.SelectedOptions, selectedOptions));

        var button = component.Find("button");
        button.TextContent.Should().Contain("Apple, Banana");
    }

    [Fact]
    public void Component_TogglesDropdown_WhenButtonClicked()
    {
        var component = RenderComponent<SimpleMultiselect<string>>(parameters => parameters
            .Add(p => p.Options, _testOptions));

        var button = component.Find("button");
        button.Click();

        var dropdown = component.Find(".dropdown-menu.show");
        dropdown.Should().NotBeNull();
    }

    [Fact]
    public void Component_ShowsAllOptions_WhenDropdownIsOpen()
    {
        var component = RenderComponent<SimpleMultiselect<string>>(parameters => parameters
            .Add(p => p.Options, _testOptions));

        var button = component.Find("button");
        button.Click();

        var dropdownItems = component.FindAll(".dropdown-item");
        dropdownItems.Should().HaveCount(_testOptions.Count);

        foreach (var option in _testOptions)
        {
            component.Markup.Should().Contain(option);
        }
    }

    [Fact]
    public void Component_SelectsOption_WhenOptionClicked()
    {
        var selectedOptions = new HashSet<string>();
        var component = RenderComponent<SimpleMultiselect<string>>(parameters => parameters
            .Add(p => p.Options, _testOptions)
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
            .Add(p => p.Options, _testOptions)
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
            .Add(p => p.Options, _testOptions)
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
            .Add(p => p.Options, _testOptions)
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
            .Add(p => p.Options, _testOptions)
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
            .Add(p => p.Options, _testOptions)
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
    public void Component_AppliesCustomCssClasses_WhenProvided()
    {
        var component = RenderComponent<SimpleMultiselect<string>>(parameters => parameters
            .Add(p => p.Options, _testOptions)
            .Add(p => p.Class, "custom-class"));

        var container = component.Find(".simple-dropdown");
        container.ClassList.Should().Contain("custom-class");
    }

    [Fact]
    public void Component_AppliesCustomStyles_WhenProvided()
    {
        var component = RenderComponent<SimpleMultiselect<string>>(parameters => parameters
            .Add(p => p.Options, _testOptions)
            .Add(p => p.Style, "width: 300px;"));

        var container = component.Find(".simple-dropdown");
        container.GetAttribute("style").Should().Contain("width: 300px;");
    }

    [Fact]
    public void Component_SetsButtonId_WhenProvided()
    {
        var component = RenderComponent<SimpleMultiselect<string>>(parameters => parameters
            .Add(p => p.Options, _testOptions)
            .Add(p => p.Id, "test-multiselect"));

        var button = component.Find("button");
        button.Id.Should().Be("test-multiselect");
    }

    [Fact]
    public void Component_ShowsStandaloneStyles_WhenStandaloneIsTrue()
    {
        var component = RenderComponent<SimpleMultiselect<string>>(parameters => parameters
            .Add(p => p.Options, _testOptions)
            .Add(p => p.Standalone, true));

        var container = component.Find(".simple-dropdown");
        container.ClassList.Should().Contain("simple-bs-compat");
    }

    [Fact]
    public void Component_CachesFilteredOptions_ForPerformance()
    {
        var component = RenderComponent<SimpleMultiselect<string>>(parameters => parameters
            .Add(p => p.Options, _testOptions)
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

    [Fact]
    public void Component_CanDeselect_WhenPrefilledValueItems()
    {
        var options = new List<TestValueItem>
        {
            new("1", "Apple"),
            new("2", "Banana"),
            new("3", "Cherry")
        };
        var selectedItems = new HashSet<TestValueItem>
        {
            new("1", "Apple")
        };

        var component = RenderComponent<SimpleMultiselect<TestValueItem>>(parameters => parameters
            .Add(p => p.Options, options)
            .Add(p => p.SelectedOptions, selectedItems)
            .Add(p => p.StringSelector, item => item.Name)
            .Add(p => p.DefaultText, "Select fruits")
            .Add(p => p.SelectedOptionsChanged, EventCallback.Factory.Create<HashSet<TestValueItem>>(this, newSelection => { selectedItems = newSelection; })));

        var button = component.Find("button");
        button.TextContent.Should().Contain("Apple");
        button.Click();

        // Now only apple should be checked
        var appleOption = component.FindAll(".dropdown-item")[0];
        var appleCheckbox = appleOption.QuerySelector<IHtmlInputElement>("input[type='checkbox']");
        appleCheckbox.Should().NotBeNull();
        appleCheckbox.IsChecked.Should().BeTrue();

        appleOption.Click();

        // After clicking, apple should be deselected
        selectedItems.Should().BeEmpty();
        button = component.Find("button");
        button.TextContent.Should().Be("Select fruits");
    }

    [Fact]
    public void Component_CanDeselect_WhenPrefilledReferenceItems()
    {
        var options = new List<TestReferenceItem>
        {
            new("1", "Apple"),
            new("2", "Banana"),
            new("3", "Cherry")
        };
        var selectedItems = new HashSet<TestReferenceItem>
        {
            new("1", "Apple")
        };

        var component = RenderComponent<SimpleMultiselect<TestReferenceItem>>(parameters => parameters
            .Add(p => p.Options, options)
            .Add(p => p.SelectedOptions, selectedItems)
            .Add(p => p.StringSelector, item => item.Name)
            .Add(p => p.DefaultText, "Select fruits")
            .Add(p => p.SelectedOptionsChanged, EventCallback.Factory.Create<HashSet<TestReferenceItem>>(this, newSelection => { selectedItems = newSelection; })));

        var button = component.Find("button");
        button.TextContent.Should().Contain("Apple");
        button.Click();

        // Now only apple should be checked
        var appleOption = component.FindAll(".dropdown-item")[0];
        var appleCheckbox = appleOption.QuerySelector<IHtmlInputElement>("input[type='checkbox']");
        appleCheckbox.Should().NotBeNull();
        appleCheckbox.IsChecked.Should().BeTrue();

        appleOption.Click();

        // After clicking, apple should be deselected
        selectedItems.Should().BeEmpty();
        button = component.Find("button");
        button.TextContent.Should().Be("Select fruits");
    }

    [Fact]
    public void Component_CanDeselectValueItem_WhenMatchByReference()
    {
        var options = new List<TestValueItem>
        {
            new("1", "Apple"),
            new("2", "Banana"),
            new("3", "Cherry")
        };
        var selectedItems = new HashSet<TestValueItem>
        {
            new("1", "Apple")
        };

        var component = RenderComponent<SimpleMultiselect<TestValueItem>>(parameters => parameters
            .Add(p => p.Options, options)
            .Add(p => p.SelectedOptions, selectedItems)
            .Add(p => p.StringSelector, item => item.Name)
            .Add(p => p.DefaultText, "Select fruits")
            .Add(p => p.SelectedOptionsChanged, EventCallback.Factory.Create<HashSet<TestValueItem>>(this, newSelection => { selectedItems = newSelection; }))
            .Add(p => p.MatchByReference, true)); // Should not matter for value types

        var button = component.Find("button");
        button.TextContent.Should().Contain("Apple");
        button.Click();

        // Now only apple should be checked
        var appleOption = component.FindAll(".dropdown-item")[0];
        var appleCheckbox = appleOption.QuerySelector<IHtmlInputElement>("input[type='checkbox']");
        appleCheckbox.Should().NotBeNull();
        appleCheckbox.IsChecked.Should().BeTrue();

        appleOption.Click();

        // After clicking, apple should be deselected
        selectedItems.Should().BeEmpty();
        button = component.Find("button");
        button.TextContent.Should().Be("Select fruits");
    }

    [Fact]
    public void Component_CannotDeselectIdenticalInstance_WhenMatchByReference()
    {
        var options = new List<TestReferenceItem>
        {
            new("1", "Apple"),
            new("2", "Banana"),
            new("3", "Cherry")
        };
        var selectedItems = new HashSet<TestReferenceItem>
        {
            new("1", "Apple")
        };

        var component = RenderComponent<SimpleMultiselect<TestReferenceItem>>(parameters => parameters
            .Add(p => p.Options, options)
            .Add(p => p.SelectedOptions, selectedItems)
            .Add(p => p.StringSelector, item => item.Name)
            .Add(p => p.DefaultText, "Select fruits")
            .Add(p => p.SelectedOptionsChanged, EventCallback.Factory.Create<HashSet<TestReferenceItem>>(this, newSelection => { selectedItems = newSelection; }))
            .Add(p => p.MatchByReference, true)); // This will break the deselection

        var button = component.Find("button");
        button.TextContent.Should().Contain("Apple");
        button.Click();

        // Apple should not be checked because the instance is different
        // So clicking it will add another apple instead of removing the existing one
        var appleOption = component.FindAll(".dropdown-item")[0];
        var appleCheckbox = appleOption.QuerySelector<IHtmlInputElement>("input[type='checkbox']");
        appleCheckbox.Should().NotBeNull();
        appleCheckbox.IsChecked.Should().BeFalse();

        appleOption.Click();

        // After clicking, we should have two apples
        selectedItems.Should().HaveCount(2);
        button = component.Find("button");
        button.TextContent.Should().Be("Apple, Apple");
    }

    private record TestValueItem(string Id, string Name);

    private class TestReferenceItem(string id, string name)
    {
        public string Id { get; set; } = id;
        public string Name { get; set; } = name;
    }
}