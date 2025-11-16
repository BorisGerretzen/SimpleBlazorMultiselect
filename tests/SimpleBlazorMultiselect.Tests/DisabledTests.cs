using Bunit;
using FluentAssertions;
using Xunit;

namespace SimpleBlazorMultiselect.Tests;

public class DisabledTests : BaseTest
{
    [Fact]
    public void Component_WhenDisabled_HasDisabledAttribute()
    {
        var component = RenderComponent<SimpleMultiselect<string>>(parameters => parameters
            .Add(p => p.Options, TestOptions)
            .Add(p => p.Disabled, true)
        );

        var button = component.Find("button");
        button.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void Component_WhenDisabled_DoesNotOpenOnClick()
    {
        var component = RenderComponent<SimpleMultiselect<string>>(parameters => parameters
            .Add(p => p.Options, TestOptions)
            .Add(p => p.Disabled, true)
        );

        var button = component.Find("button");
        button.Click();

        AssertClosed(component);
    }

    [Fact]
    public void Component_WhenEnabled_OpensOnClick()
    {
        var component = RenderComponent<SimpleMultiselect<string>>(parameters => parameters
            .Add(p => p.Options, TestOptions)
            .Add(p => p.Disabled, false)
        );

        var button = component.Find("button");
        button.Click();

        AssertOpen(component, TestOptions.Count);
    }

    [Fact]
    public void Component_WhenDisabledThenEnabled_OpensOnClick()
    {
        var component = RenderComponent<SimpleMultiselect<string>>(parameters => parameters
            .Add(p => p.Options, TestOptions)
            .Add(p => p.Disabled, true)
        );

        var button = component.Find("button");
        button.Click();
        AssertClosed(component);

        component.SetParametersAndRender(parameters => parameters.Add(p => p.Disabled, false));

        button.Click();
        AssertOpen(component, TestOptions.Count);
    }

    [Fact]
    public void Component_WhenEnabledThenDisabled_DoesNotOpenOnClick()
    {
        var component = RenderComponent<SimpleMultiselect<string>>(parameters => parameters
            .Add(p => p.Options, TestOptions)
            .Add(p => p.Disabled, false)
        );

        var button = component.Find("button");
        button.Click();
        AssertOpen(component, TestOptions.Count);

        component.SetParametersAndRender(parameters => parameters.Add(p => p.Disabled, true));

        button.Click();
        AssertClosed(component);
    }

    [Fact]
    public void Component_WhenDisabledWithMenuOpen_ClosesMenu()
    {
        var component = RenderComponent<SimpleMultiselect<string>>(parameters => parameters
            .Add(p => p.Options, TestOptions)
        );

        var button = component.Find("button");
        button.Click();
        AssertOpen(component, TestOptions.Count);

        component.SetParametersAndRender(parameters => parameters.Add(p => p.Disabled, true));

        AssertClosed(component);
    }
}