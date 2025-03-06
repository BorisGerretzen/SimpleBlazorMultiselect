# Simple blazor multiselect Razor Class Library Component FORK without using Bootstrap
This package contains a simple blazor dropdown component that supports single and multiple selection.

This fork is designed to use as a component for the Razor Class Library without additional nuget packages and adding stylesheets.

## Installation
1. Add the following to your `_Imports.razor` file:
    ```csharp
    @using SimpleBlazorMultiselect
    ```


## Usage
See the project `SimpleBlazorMultiselectDemo` for more examples of how to use the component, 
or take a look at the properties page on the [wiki](https://github.com/BorisGerretzen/SimpleBlazorMultiselect/wiki/Properties).
The demo project is hosted on [GitHub Pages](https://borisgerretzen.github.io/SimpleBlazorMultiselect/).

Below are some short examples, they all use the following `@code` block:
```csharp
@code {
    private readonly List<string> _items = new() { "Item 1", "Item 2", "Item 3", "Item 4", "Item 5", "Item 6", "Item 7", "Item 8", "Item 9", "Item 10" };
    private List<string> _selectedItems = new();
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
