using System;

namespace VacationRental.Api.Models
{
    public class BookingBindingModel
    {
        public int RentalId { get; set; }

        public DateTime Start
        {
            get => _startIgnoreTime;
            set => _startIgnoreTime = value.Date;
        }

        //can't we get rid of backing field?
        private DateTime _startIgnoreTime;
        public int Nights { get; set; }
    }
}
