namespace VillagioMichelinn;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }


    private async void OnFamiliaClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CadastroFamilia());
    }

    private async void OnAgenciaClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CadastroAgencia());
    }

}