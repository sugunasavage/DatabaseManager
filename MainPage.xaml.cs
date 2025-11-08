using DatabaseManager.Models;
using DatabaseManager.ViewModels;

namespace DatabaseManager;

public partial class MainPage : ContentPage
{
	public MainPage(MainViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;

		// Populate the database type picker
		DatabaseTypePicker.ItemsSource = Enum.GetValues(typeof(DatabaseType));
	}
}
