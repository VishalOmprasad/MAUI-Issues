using CommunityToolkit.Maui.Views;

namespace MauiToolkitPopupSample;

public partial class MainPage : ContentPage
{

	public MainPage()
	{
		InitializeComponent();
	}

	private void OnCounterClicked(object sender, EventArgs e)
	{
		this.ShowPopup(new PopupPage());
	}
}

