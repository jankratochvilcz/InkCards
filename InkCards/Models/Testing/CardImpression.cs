using InkCards.Models.Cards;
using System;

namespace InkCards.Models.Testing
{
    public class CardImpression
    {
        public int Id { get; set; }

        public Guid CardId { get; set; }

        public DateTime Date { get; set; }

        public long FrontMillisecondsSpent { get; set; }

        public long BackMillisecondsSpent { get; set; }

        public CardSide TestedSide { get; set; }

        public bool GuessedCorrectly { get; set; }
    }
}
