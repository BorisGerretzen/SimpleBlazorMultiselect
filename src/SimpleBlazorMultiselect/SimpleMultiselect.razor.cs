using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace SimpleBlazorMultiselect;

public partial class SimpleMultiselect<TItem> : ComponentBase
{
    /// <summary>
    /// Represents the selected options in the multiselect dropdown.
    /// </summary>
    [Parameter]
    public HashSet<TItem> SelectedOptions { get; set; } = new();

    /// <summary>
    /// Event callback that is invoked when the selected options change.
    /// </summary>
    [Parameter]
    public EventCallback<HashSet<TItem>> SelectedOptionsChanged { get; set; }

    /// <summary>
    /// Represents the available options in the multiselect dropdown.
    /// </summary>
    [Parameter]
    public List<TItem> Options { get; set; } = new();

    /// <summary>
    /// Function to convert an item to a string for display in the dropdown.
    /// </summary>
    [Parameter]
    public Func<TItem, string> StringSelector { get; set; } = item => item?.ToString() ?? string.Empty;

    /// <summary>
    /// The default text to display when no options are selected.
    /// </summary>
    [Parameter]
    public string DefaultText { get; set; } = "Select Options";

    /// <summary>
    /// Optional custom renderer for the selected options.
    /// If not set, items will be rendered using <see cref="StringSelector"/> and concatenated with a comma.
    /// </summary>
    [Parameter]
    public RenderFragment<IEnumerable<TItem>>? SelectedOptionsRenderer { get; set; }

    /// <summary>
    /// Whether the dropdown should be filterable.
    /// </summary>
    [Parameter]
    public bool CanFilter { get; set; }

    /// <summary>
    /// A function that determines whether a given item matches the filter string.
    /// Default implementation checks if the item's string representation contains the filter string, case-insensitive.
    /// </summary>
    /// <returns>Returns true if the item's string representation contains the filter string, false otherwise.</returns>
    [Parameter]
    public Func<TItem, string, bool>? FilterPredicate { get; set; }

    private bool DefaultFilterPredicate(TItem item, string filterString)
    {
        return StringSelector(item).Contains(filterString, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Whether the virtualize component should be used to render the options.
    /// </summary>
    [Parameter]
    public bool Virtualize { get; set; }

    /// <summary>
    /// Whether the multiselect should allow multiple selections.
    /// </summary>
    [Parameter]
    public bool IsMultiSelect { get; set; } = true;

    /// <summary>
    /// The id the input element should have.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// Additional CSS classes to apply.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Additional CSS styles to apply.
    /// </summary>
    [Parameter]
    public string? Style { get; set; }

    /// <summary>
    /// Whether the multiselect is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// If true, the SimpleMultiselect component will be rendered properly without needing Bootstrap.
    /// </summary>
    [CascadingParameter(Name = "Standalone")]
    public bool Standalone { get; set; }
    
    private async Task ToggleOption(TItem option)
    {
        var newSelected = new HashSet<TItem>(SelectedOptions);
        
        // Find the existing selected item that matches this option by string representation
        var existingSelected = FindSelectedItemByStringRepresentation(option);
        
        if (existingSelected != null && newSelected.Remove(existingSelected))
        {
            // Item was found and removed (deselected)
        }
        else
        {
            // Item was not found, so add it (selected)
            if (!IsMultiSelect)
            {
                newSelected.Clear();
            }
            newSelected.Add(option);
        }

        SelectedOptions = newSelected;
        await SelectedOptionsChanged.InvokeAsync(SelectedOptions);

        if (!IsMultiSelect)
        {
            await CloseDropdown();
        }
    }

    private bool IsOptionSelected(TItem option)
    {
        return FindSelectedItemByStringRepresentation(option) != null;
    }
    
    private TItem? FindSelectedItemByStringRepresentation(TItem option)
    {
        var optionString = StringSelector(option);
        return SelectedOptions.FirstOrDefault(selected => StringSelector(selected) == optionString);
    }

    private List<TItem>? _filteredOptionsCache;
    private List<TItem>? _prevOptions;
    private Func<TItem, string, bool>? _prevFilterPredicate;
    private string? _prevFilterText;
    private bool _prevCanFilter;
    
    private List<TItem> FilteredOptions()
    {
        if(_prevCanFilter == CanFilter && _prevFilterPredicate == FilterPredicate && _prevFilterText == _filterText && _prevOptions == Options)
        {
            return _filteredOptionsCache ?? Options;
        }
        
        _prevOptions = Options;
        _prevCanFilter = CanFilter;
        _prevFilterPredicate = FilterPredicate;
        _prevFilterText = _filterText;
        
        _filteredOptionsCache = [];
        if(!CanFilter || string.IsNullOrWhiteSpace(_filterText))
        {
            _filteredOptionsCache.AddRange(Options);
        }
        else
        {
            var predicate = FilterPredicate ?? DefaultFilterPredicate;
            _filteredOptionsCache.AddRange(Options.Where(option => predicate(option, _filterText)));
        }
        
        return _filteredOptionsCache;
    }
}