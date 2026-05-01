using System.Windows.Input;

namespace FinalYearProject.Controls;

public partial class Card : ContentView
{
	public Card()
	{
		InitializeComponent();
	}

	public static readonly BindableProperty CornerRadiusProperty =
		BindableProperty.Create(
			nameof(CornerRadius),
			typeof(float),
			typeof(Card),
			0f);

	public float CornerRadius
	{
		get => (float)GetValue(CornerRadiusProperty);
		set => SetValue(CornerRadiusProperty, value);
	}

    public static readonly BindableProperty BackgroundProperty =
        BindableProperty.Create(
            nameof(Background),
            typeof(Color),
            typeof(Card),
            Colors.White);

    public Color Background
    {
        get => (Color)GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public static readonly BindableProperty FontColourProperty =
        BindableProperty.Create(
            nameof(FontColour),
            typeof(Color),
            typeof(Card),
            Colors.White);

    public Color FontColour
    {
        get => (Color)GetValue(FontColourProperty);
        set => SetValue(FontColourProperty, value);
    }

    public static readonly BindableProperty FontSizeProperty =
        BindableProperty.Create(
            nameof(FontSize),
            typeof(int),
            typeof(Card),
            12);

    public int FontSize
    {
        get => (int)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(
            nameof(Text),
            typeof(string),
            typeof(Card),
            "");

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(
            nameof(Command),
            typeof(ICommand),
            typeof(Card));

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(
            nameof(CommandParameter),
            typeof(object),
            typeof(Card));

    public object CommandParameter
    {
        get => (object)GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }
}