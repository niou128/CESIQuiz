using Quiz.Data;
using System.Diagnostics;

namespace Quiz
{
    public partial class App : Application
    {
        private readonly IDatabaseService _databaseService;
        public static IServiceProvider ServiceProvider { get; private set; }

        public App(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            InitializeComponent();
            _databaseService = serviceProvider.GetRequiredService<IDatabaseService>();
            MainPage = new AppShell();
        }

        protected override async void OnStart()
        {
            try
            {
                await _databaseService.InitializeDatabaseAsync();
                await _databaseService.InitializeDataAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Startup failure: {ex}");
            }
        }
    }
}
