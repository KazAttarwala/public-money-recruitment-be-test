using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using VacationRental.Api.Models;

namespace VacationRental.Api.Controllers
{
    [Route("api/v1/bookings")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly IDictionary<int, RentalViewModel> _rentals;
        private readonly IDictionary<int, BookingViewModel> _bookings;

        public BookingsController(
            IDictionary<int, RentalViewModel> rentals,
            IDictionary<int, BookingViewModel> bookings)
        {
            _rentals = rentals;
            _bookings = bookings;
        }

        [HttpGet]
        [Route("{bookingId:int}")]
        public BookingViewModel Get(int bookingId)
        {
            if (!_bookings.ContainsKey(bookingId))
                throw new ApplicationException("Booking not found");

            return _bookings[bookingId];
        }

        [HttpPost]
        public ResourceIdViewModel Post(BookingBindingModel model)
        {
            if (model.Nights <= 0)
                throw new ApplicationException("Nigts must be positive");
            if (!_rentals.ContainsKey(model.RentalId))
                throw new ApplicationException("Rental not found");

            if (!CanMakeBooking(model.Start, model.Nights, _rentals[model.RentalId].PreparationTimeInDays, model.RentalId))
                throw new ApplicationException("Not available");
           
            var key = new ResourceIdViewModel { Id = _bookings.Keys.Count + 1 };

            _bookings.Add(key.Id, new BookingViewModel
            {
                Id = key.Id,
                Nights = model.Nights,
                RentalId = model.RentalId,
                Start = model.Start.Date
            });

            return key;
        }

        private bool CanMakeBooking(DateTime newBookingStart, int numOfNights, int preparationTime, int rentalId)
        {
            var rentalBookings = _bookings.Values.Where(booking => booking.RentalId == rentalId);

            var conflictingBookings = rentalBookings.Where(booking =>
            (booking.Start <= newBookingStart.Date && booking.Start.AddDays(booking.Nights + preparationTime) > newBookingStart.Date)
                        || (booking.Start < newBookingStart.AddDays(numOfNights + preparationTime) && booking.Start.AddDays(booking.Nights + preparationTime)
                            >= newBookingStart.AddDays(numOfNights + preparationTime))
                        || (booking.Start > newBookingStart && booking.Start.AddDays(booking.Nights + preparationTime) < newBookingStart.AddDays(numOfNights + preparationTime)));

            return conflictingBookings.Count() < _rentals[rentalId].Units;
        }
    }
}
