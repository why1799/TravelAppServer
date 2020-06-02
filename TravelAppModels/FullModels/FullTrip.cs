using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using TravelAppModels.Models;

namespace TravelAppModels.FullModels
{
    public class FullTrip : Trip
    {
        public FullTrip()
        {

        }
        public FullTrip(Trip trip)
        {
            Id = trip.Id;
            UserId = trip.UserId;
            Name = trip.Name;
            TextField = trip.TextField;
            PhotoIds = trip.PhotoIds;
            FileIds = trip.FileIds;
            PlaceIds = trip.PlaceIds;
            GoodIds = trip.GoodIds;
            GoalIds = trip.GoalIds;
            PurchaseIds = trip.PurchaseIds;
            FromDate = trip.FromDate;
            ToDate = trip.ToDate;
            LastUpdate = trip.LastUpdate;
            IsDeleted = trip.IsDeleted;
            Places = trip.Places;
            Goods = trip.Goods;
            Goals = trip.Goals;
            Purchases = trip.Purchases;
            Notes = trip.Notes;
        }

        public new Place[] Places { get; set; }
        public new Good[] Goods { get; set; }
        public new Goal[] Goals { get; set; }
        public new Purchase[] Purchases { get; set; }
    }
}
