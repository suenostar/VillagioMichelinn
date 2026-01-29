namespace Teste;

public partial class Pagamento : ContentPage
{
    public Pagamento()
    {
        InitializeComponent();
    }
    private async void OnInicioClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Reservas());
    }
}