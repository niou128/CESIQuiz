using Quiz.Data;

namespace Quiz
{
    public partial class App : Application
    {
        private readonly IDatabaseService _databaseService;

        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _databaseService = serviceProvider.GetService<IDatabaseService>();
            MainPage = new AppShell();
        }

        protected override async void OnStart()
        {
            await _databaseService.InitializeDatabaseAsync();
        }
    }
}
