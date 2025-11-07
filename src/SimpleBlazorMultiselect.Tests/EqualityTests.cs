using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace SimpleBlazorMultiselect.Tests;

public class EqualityTests : BaseTest
{
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
}