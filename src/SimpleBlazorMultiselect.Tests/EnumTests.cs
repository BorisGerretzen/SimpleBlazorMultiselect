using AngleSharp.Html.Dom;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace SimpleBlazorMultiselect.Tests;

public class EnumTests : BaseTest
{
    [Fact]
    public void Component_WithEmptySet_ShouldShowNoChecks()
    {
        var options = Enum.GetValues<TestEnum>().ToList();
        var selectedItems = new HashSet<TestEnum>();

        var component = RenderComponent<SimpleMultiselect<TestEnum>>(parameters => parameters
            .Add(p => p.Options, options)
            .Add(p => p.SelectedOptions, selectedItems)
            .Add(p => p.StringSelector, item => item.ToString())
            .Add(p => p.DefaultText, "empty")
            .Add(p => p.SelectedOptionsChanged, EventCallback.Factory.Create<HashSet<TestEnum>>(this, newSelection => { selectedItems = newSelection; })));

        var button = component.Find("button");
        button.TextContent.Should().Be("empty");
        button.Click();

        // Check if all options are unchecked
        var dropdownItems = component.FindAll(".dropdown-item");
        foreach (var item in dropdownItems)
        {
            var checkbox = item.QuerySelector("input[type='checkbox']") as IHtmlInputElement;
            checkbox.Should().NotBeNull();
            checkbox.IsChecked.Should().BeFalse();
        }
    }

    [Fact]
    public void Component_WithPrefilledOptions_ShouldShowCorrectCheck()
    {
        var options = Enum.GetValues<TestEnum>().ToList();
        var selectedItems = new HashSet<TestEnum>
        {
            TestEnum.OptionB
        };

        var component = RenderComponent<SimpleMultiselect<TestEnum>>(parameters => parameters
            .Add(p => p.Options, options)
            .Add(p => p.SelectedOptions, selectedItems)
            .Add(p => p.StringSelector, item => item.ToString())
            .Add(p => p.DefaultText, "empty")
            .Add(p => p.SelectedOptionsChanged, EventCallback.Factory.Create<HashSet<TestEnum>>(this, newSelection => { selectedItems = newSelection; })));

        var button = component.Find("button");
        button.TextContent.Should().Be(nameof(TestEnum.OptionB));
        button.Click();

        // Check if all options are unchecked except OptionB
        var dropdownItems = component.FindAll(".dropdown-item");
        foreach (var item in dropdownItems)
        {
            var checkbox = item.QuerySelector("input[type='checkbox']") as IHtmlInputElement;
            checkbox.Should().NotBeNull();

            var optionText = item.TextContent.Trim();
            if (optionText == nameof(TestEnum.OptionB))
            {
                checkbox.Should().NotBeNull();
                checkbox.IsChecked.Should().BeTrue();
            }
            else
            {
                checkbox.IsChecked.Should().BeFalse();
            }
        }
    }

    [Fact]
    public void Component_CanUncheckPrefilled()
    {
        var options = Enum.GetValues<TestEnum>().ToList();
        var selectedItems = new HashSet<TestEnum>
        {
            TestEnum.OptionB
        };

        var component = RenderComponent<SimpleMultiselect<TestEnum>>(parameters => parameters
            .Add(p => p.Options, options)
            .Add(p => p.SelectedOptions, selectedItems)
            .Add(p => p.StringSelector, item => item.ToString())
            .Add(p => p.DefaultText, "empty")
            .Add(p => p.SelectedOptionsChanged, EventCallback.Factory.Create<HashSet<TestEnum>>(this, newSelection => { selectedItems = newSelection; })));

        var button = component.Find("button");
        button.TextContent.Should().Be(nameof(TestEnum.OptionB));
        button.Click();

        // Check if option B is checked
        var dropdownItems = component.FindAll(".dropdown-item");
        var optionBItem = dropdownItems.First(item => item.TextContent.Trim() == nameof(TestEnum.OptionB));
        var optionBCheckbox = optionBItem.QuerySelector("input[type='checkbox']") as IHtmlInputElement;
        optionBCheckbox.Should().NotBeNull();
        optionBCheckbox.IsChecked.Should().BeTrue();

        // Click to uncheck OptionB
        optionBItem.Click();

        // After clicking, OptionB should be deselected
        selectedItems.Should().BeEmpty();
        button = component.Find("button");
        button.TextContent.Should().Be("empty");
    }

    [Fact]
    public void Component_CanCheckUncheckedOption()
    {
        var options = Enum.GetValues<TestEnum>().ToList();
        var selectedItems = new HashSet<TestEnum>();

        var component = RenderComponent<SimpleMultiselect<TestEnum>>(parameters => parameters
            .Add(p => p.Options, options)
            .Add(p => p.SelectedOptions, selectedItems)
            .Add(p => p.StringSelector, item => item.ToString())
            .Add(p => p.DefaultText, "empty")
            .Add(p => p.SelectedOptionsChanged, EventCallback.Factory.Create<HashSet<TestEnum>>(this, newSelection => { selectedItems = newSelection; })));

        var button = component.Find("button");
        button.TextContent.Should().Be("empty");
        button.Click();

        // Check if all options are unchecked
        var dropdownItems = component.FindAll(".dropdown-item");
        var optionCItem = dropdownItems.First(item => item.TextContent.Trim() == nameof(TestEnum.OptionC));
        var optionCCheckbox = optionCItem.QuerySelector("input[type='checkbox']") as IHtmlInputElement;
        optionCCheckbox.Should().NotBeNull();
        optionCCheckbox.IsChecked.Should().BeFalse();

        // Click to check OptionC
        optionCItem.Click();

        // After clicking, OptionC should be selected
        selectedItems.Should().Contain(TestEnum.OptionC);
        button = component.Find("button");
        button.TextContent.Should().Be(nameof(TestEnum.OptionC));
    }

    private enum TestEnum
    {
        OptionA,
        OptionB,
        OptionC
    }
}