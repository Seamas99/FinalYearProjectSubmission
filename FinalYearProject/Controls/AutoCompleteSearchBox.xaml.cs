using System.Collections;
using System.Windows.Input;

namespace FinalYearProject.Controls;

public partial class AutoCompleteSearchBox : ContentView
{
	public AutoCompleteSearchBox()
	{
		InitializeComponent();
    }

    /// <summary>
    /// Bindable properties for control
    /// </summary>
    public static readonly BindableProperty SearchCommandProperty =
        BindableProperty.Create(nameof(SearchCommand), typeof(ICommand), typeof(AutoCompleteSearchBox));

    public static readonly BindableProperty ResultsSourceProperty =
        BindableProperty.Create(nameof(ResultsSource), typeof(IEnumerable), typeof(AutoCompleteSearchBox));

    public static readonly BindableProperty ItemTemplateProperty =
        BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(AutoCompleteSearchBox));

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(AutoCompleteSearchBox));

    public static readonly BindableProperty DisplayMemberPathProperty =
    BindableProperty.Create(nameof(DisplayMemberPath), typeof(string), typeof(AutoCompleteSearchBox), string.Empty);

    public static readonly BindableProperty SelectedItemProperty =
    BindableProperty.Create(nameof(SelectedItem), typeof(string), typeof(AutoCompleteSearchBox), string.Empty, defaultBindingMode : BindingMode.TwoWay);

    public ICommand SearchCommand 
    {
        get => (ICommand)GetValue(SearchCommandProperty);
        set => SetValue(SearchCommandProperty, value);
    }
    public IEnumerable ResultsSource
    { 
        get => (IEnumerable)GetValue(ResultsSourceProperty);
        set => SetValue(ResultsSourceProperty, value);
    }
    public DataTemplate ItemTemplate
    {
        get => (DataTemplate)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }
    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }
    public string DisplayMemberPath
    {
        get => (string)GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }
    public string SelectedItem
    {
        get => (string)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    // Logic to toggle dropdown visibility
    private bool _isDropdownOpen;
    public bool IsDropdownOpen
    {
        get => _isDropdownOpen;
        set { _isDropdownOpen = value; OnPropertyChanged(); }
    }

    //To prevent infinite loop in onselection/text changed
    private bool _isUpdatingFromCode;

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdatingFromCode) return;
        if (string.IsNullOrWhiteSpace(e.NewTextValue))
        {
            IsDropdownOpen = false;
            return;
        }

        // Execute the command provided by the property
        SearchCommand?.Execute(e.NewTextValue);
        IsDropdownOpen = true;
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedItem = e.CurrentSelection.FirstOrDefault();
        if (selectedItem == null) return;

        string textToSet = string.Empty;

        if (!string.IsNullOrEmpty(DisplayMemberPath))
        {
            var prop = selectedItem.GetType().GetProperty(DisplayMemberPath);
            textToSet = prop?.GetValue(selectedItem)?.ToString() ?? selectedItem.ToString();
            SelectedItem = textToSet;
        }
        else
        {
            textToSet = selectedItem.ToString();
        }

        _isUpdatingFromCode = true;
        //Update the SearchBar Text
        InternalSearchBar.Text = textToSet;
        _isUpdatingFromCode = false;

        //Close the dropdown
        IsDropdownOpen = false;

        //Clear selection so the user can click the same item again later
        ((CollectionView)sender).SelectedItem = null;
    }
}