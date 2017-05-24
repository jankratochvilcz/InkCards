using System;

namespace InkCards.Models.Cards
{
    public class CardCollection
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public DateTime? LastOpened { get; set; }
    }
}
