﻿using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Quiz.Data;
using Quiz.ViewModels;
using Quiz.Views;

namespace Quiz
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "questions.db");
            builder
            .UseMauiApp<App>()
            // Enregistrez votre service de base de données
            .Services.AddSingleton<IDatabaseService, SQLiteDatabaseService>(serviceProvider =>
                new SQLiteDatabaseService(databasePath));
            builder.Services.AddTransient<QuizViewModel>();
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddTransient<QuizPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
