using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Quiz.Data;
using Quiz.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Quiz.ViewModels
{
    public partial class QuizViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<Question> questions = new();

        [ObservableProperty]
        private Question currentQuestion;

        private readonly IDatabaseService _databaseService;

        private int _questionIndex;
        private int _score;

        [ObservableProperty]
        private bool quizStarted;

        public QuizViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [RelayCommand]
        private async Task StartQuizAsync()
        {
            try
            {
                QuizStarted = true;
                Questions.Clear();
                var allQuestions = await _databaseService.GetQuestionsAsync();
                var randomQuestions = allQuestions.OrderBy(x => Guid.NewGuid()).Take(10).ToList();

                foreach (var question in randomQuestions)
                {
                    Questions.Add(question);
                }

                _questionIndex = 0;
                _score = 0;
                if (Questions.Any())
                {
                    CurrentQuestion = Questions.First();
                }
            }
            catch (Exception ex)
            {
                // TODO: Handle exceptions as appropriate for your app.
                Console.WriteLine(ex);
            }
        }

        [RelayCommand]
        private async Task CheckAnswerAsync(string selectedChoice)
        {
            if (currentQuestion is null)
            {
                // Question is null, don't proceed.
                return;
            }

            var correctAnswer = CurrentQuestion.Choices[CurrentQuestion.CorrectAnswerIndex];
            if (selectedChoice == correctAnswer)
            {
                _score++;
                await Application.Current.MainPage.DisplayAlert("Résultat", "Bonne réponse!", "OK");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Résultat", "Mauvaise réponse!", "OK");
            }

            await ShowNextQuestionAsync();
        }

        private async Task ShowNextQuestionAsync()
        {
            if (_questionIndex < Questions.Count - 1)
            {
                _questionIndex++;
                CurrentQuestion = Questions[_questionIndex];
            }
            else
            {
                // Quiz terminé
                CurrentQuestion = null;
                QuizStarted = false;
                await Application.Current.MainPage.DisplayAlert("Quiz", "Le quiz est terminé. Votre score : " + _score, "OK");
            }
        }
    }
}
