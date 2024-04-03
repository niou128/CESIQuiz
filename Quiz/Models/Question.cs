using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quiz.Models
{
    public class Question
    {
        // Identifiant unique pour chaque question, utile pour la gestion dans la base de données
        public int Id { get; set; }

        // Catégorie de la question, par exemple "Histoire", "Science", etc.
        public string Category { get; set; }

        // Le texte de la question
        public string Text { get; set; }

        // Liste des choix possibles pour cette question
        public List<string> Choices { get; set; }

        // L'index de la réponse correcte dans la liste des choix
        // Par exemple, si 0, cela signifie que le premier choix est la réponse correcte
        public int CorrectAnswerIndex { get; set; }

        // Constructeur sans paramètre (nécessaire pour certains types de sérialisation/désérialisation)
        public Question() { }

    }
}
