using Quiz.Data;

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
            _databaseService = serviceProvider.GetService<IDatabaseService>();
            MainPage = new AppShell();
        }

        protected override async void OnStart()
        {
            await _databaseService.InitializeDatabaseAsync();
        }
    }
}
