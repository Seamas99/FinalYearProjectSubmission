namespace FinalYearProject.Screens;

public partial class SignUpView : ContentPage
{
	public SignUpView(SignUpViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}