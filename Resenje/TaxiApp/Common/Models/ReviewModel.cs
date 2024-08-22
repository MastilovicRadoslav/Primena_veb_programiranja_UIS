namespace Common.Models
{
    public class ReviewModel //koristim za ocenjivanje pa mi treba Id voznje i ocena
    {
        public Guid tripId { get; set; } //Id voznje
        public int rating { get; set; } //Ocena voznje

        public ReviewModel()
        {
        }
    }
}
