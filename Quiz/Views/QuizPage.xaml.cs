using Quiz.Data;
using Quiz.ViewModels;

namespace Quiz.Views;

public partial class QuizPage : ContentPage
{
	public QuizPage()
	{
		InitializeComponent();
        BindingContext = App.ServiceProvider.GetService<QuizViewModel>();
    }
}