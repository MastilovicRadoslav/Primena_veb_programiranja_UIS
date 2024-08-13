﻿using Common.Entities;
using Common.Models;

namespace Common.Mapper
{
    public class RoadTripEntityMapper
    {
        public RoadTripEntityMapper() { }

        public static RoadTripModel MapRoadTripEntityToRoadTrip(RoadTripEntity roadTrip)
        {
            return new RoadTripModel(roadTrip.CurrentLocation, roadTrip.Destination, roadTrip.RiderId, roadTrip.DriverId, roadTrip.Price, roadTrip.Accepted, roadTrip.TripId, roadTrip.SecondsToDriverArive, roadTrip.SecondsToEndTrip, roadTrip.IsFinished, roadTrip.IsRated);
        }
    }
}
