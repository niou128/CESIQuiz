using Quiz.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quiz.Data
{
    public interface IDatabaseService
    {
        Task InitializeDatabaseAsync();
        Task<List<Question>> GetQuestionsAsync();
        Task AddQuestionAsync(Question question);
        Task<List<string>> GetCategoriesAsync();
    }
}
