# Simple blazor multiselect
[![NuGet](https://img.shields.io/nuget/v/SimpleBlazorMultiselect.svg)](https://www.nuget.org/packages/SimpleBlazorMultiselect/)
[![Static Badge](https://img.shields.io/badge/View%20demo-6c757d)](https://borisgerretzen.github.io/SimpleBlazorMultiselect/)

This package contains a simple Blazor dropdown component that supports single and multiple selection.
It is compatible with Bootstrap in the sense that if you include Bootstrap in your project, the dropdown will look and feel like a Bootstrap dropdown including dark mode.

## Installation
You can find the Nuget package [here](https://www.nuget.org/packages/SimpleBlazorMultiselect/), install it using the following command:
```bash
dotnet add package SimpleBlazorMultiselect
```

## Usage
See the project `SimpleBlazorMultiselectDemo` for more examples of how to use the component, 
or take a look at the properties page on the [wiki](https://github.com/BorisGerretzen/SimpleBlazorMultiselect/wiki/Properties).
You can also view a live demo [here](https://borisgerretzen.github.io/SimpleBlazorMultiselect/).

> [!IMPORTANT]
> If you want to use the project without Bootstrap, set `SimpleMultiselectGlobals.Standalone` to `true` in your `Program.cs` file.
> Alternatively you can use a cascading parameter with name `Standalone` around the component.

Below are some short examples, they all use the following `@code` block:
```csharp
@code {
    private readonly List<string> _items = ["Item 1", "Item 2", "Item 3", "Item 4", "Item 5", "Item 6", "Item 7", "Item 8", "Item 9", "Item 10" ];
    private HashSet<string> _selectedItems = new();
}
```

### Basic dropdown
```html
<SimpleMultiselect
    Options="_items"
    @bind-SelectedOptions="_selectedItems"/>
```
![image](https://github.com/BorisGerretzen/SimpleBlazorMultiselect/assets/15902678/2f6bb03e-e076-44dc-a90d-1a8c24b84fee)

### Dropdown with custom item template
```html
<SimpleMultiselect
        Options="_items"
        @bind-SelectedOptions="_selectedItems">
    <SelectedOptionsRenderer Context="options">
        @foreach (var item in options)
        {
            <span 
                class="badge bg-primary"
                style="padding: 6px; margin-right: 10px;">
                @item
            </span>
        }
    </SelectedOptionsRenderer>
</SimpleMultiselect>
```
![image](https://github.com/BorisGerretzen/SimpleBlazorMultiselect/assets/15902678/fa0ee874-b95f-4ee7-b813-7c321aadef74)

### Dropdown with filter
```html
<SimpleMultiselect
        Options="_items"
        @bind-SelectedOptions="_selectedItems"
        CanFilter="true"/>
```
![image](https://github.com/BorisGerretzen/SimpleBlazorMultiselect/assets/15902678/5f54049a-23c0-428b-992f-7735cffb985f)

### Bootstrap dark mode
![image](https://github.com/user-attachments/assets/8613cd7a-cf6f-4116-b5f6-c0ee2b995fa6)

