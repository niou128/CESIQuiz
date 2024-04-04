using CommunityToolkit.Maui.Views;
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

        [ObservableProperty]
        private bool _answerSelected;

        [ObservableProperty]
        private double opacityButton1 = 1.0;

        [ObservableProperty]
        private double opacityButton2 = 1.0;

        [ObservableProperty]
        private double opacityButton3 = 1.0;

        [ObservableProperty]
        private double opacityButton4 = 1.0;

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
                Console.WriteLine(ex);
            }
        }

        [RelayCommand]
        private void CheckAnswer(string selectedChoice)
        {
            if (CurrentQuestion is null)
            {
                // Question is null, don't proceed.
                return;
            }

            var correctAnswer = CurrentQuestion.Choices[CurrentQuestion.CorrectAnswerIndex];
            if (selectedChoice == correctAnswer)
            {
                _score++;
                //await Application.Current.MainPage.DisplayAlert("Résultat", "Bonne réponse!", "OK");
            }
            else
            {
                //await Application.Current.MainPage.DisplayAlert("Résultat", "Mauvaise réponse!", "OK");
            }

            RevealAnswers(CurrentQuestion.CorrectAnswerIndex);

            //await ShowNextQuestionAsync();
            AnswerSelected = true;
        }

        private void RevealAnswers(int selectedIndex)
        {
            // Réinitialiser tous les boutons à pleine opacité
            OpacityButton1 = OpacityButton2 = OpacityButton3 = OpacityButton4 = 0.5;

            // Réglez la bonne réponse à pleine opacité
            switch (CurrentQuestion.CorrectAnswerIndex)
            {
                case 0: OpacityButton1 = 1.0; break;
                case 1: OpacityButton2 = 1.0; break;
                case 2: OpacityButton3 = 1.0; break;
                case 3: OpacityButton4 = 1.0; break;
            }
        }

        [RelayCommand]
        private async Task ShowNextQuestionAsync()
        {
            if (_questionIndex < Questions.Count - 1)
            {
                _questionIndex++;
                CurrentQuestion = Questions[_questionIndex];
                ResetAnswerState();
            }
            else
            {
                // Quiz terminé
                CurrentQuestion = null;
                QuizStarted = false;
                AnswerSelected = false;
                await Application.Current.MainPage.DisplayAlert("Quiz", "Le quiz est terminé. Votre score : " + _score, "OK");
            }
        }

        private void ResetAnswerState()
        {
            AnswerSelected = false;
            OpacityButton1 = OpacityButton2 = OpacityButton3 = OpacityButton4 = 1;
            //OnPropertyChanged(nameof(AnswerSelected));
        }
    }
}
