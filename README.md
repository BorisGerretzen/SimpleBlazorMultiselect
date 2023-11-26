# Simple blazor multiselect
This package contains a simple blazor dropdown component that supports single and multiple selection.

## Usage
See the project `SimpleBlazorMultiselectDemo` for more examples of how to use the component.
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
                @item.Name
            </span>
        }
    </SelectedOptionsRenderer>
</SimpleMultiselect>
```

### Dropdown with filter
```html
<SimpleMultiselect
        Options="_items"
        CanFilter="true"
        Options="_items"
        @bind-SelectedOptions="_selectedItems"/>
```

