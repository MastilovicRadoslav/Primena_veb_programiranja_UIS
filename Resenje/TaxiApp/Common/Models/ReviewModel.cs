namespace Common.Models
{
    public class ReviewModel
    {
        public Guid tripId { get; set; }
        public int rating { get; set; }

        public ReviewModel()
        {
        }
    }
}
