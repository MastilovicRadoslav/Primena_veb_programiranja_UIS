﻿namespace Common.Models
{
    public class AcceptedRoadTripModel  //koristi za prijem podataka iz frontenda, sadrži sve potrebne informacije o vožnji koju korisnik želi da prihvati
    {
        public string Destination { get; set; } // ovo dobija 
        public string CurrentLocation { get; set; } // ovo dobija 
        public Guid RiderId { get; set; } // ovo dobija 
        public double Price { get; set; } // ovo dobija 
        public bool Accepted { get; set; } // ovo dobija 

        public int MinutesToDriverArrive { get; set; }
        public AcceptedRoadTripModel() { }
    }
}
