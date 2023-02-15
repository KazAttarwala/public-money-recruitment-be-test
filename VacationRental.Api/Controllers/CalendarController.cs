using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using VacationRental.Api.Models;

namespace VacationRental.Api.Controllers
{
    [Route("api/v1/calendar")]
    [ApiController]
    public class CalendarController : ControllerBase
    {
        private readonly IDictionary<int, RentalViewModel> _rentals;
        private readonly IDictionary<int, BookingViewModel> _bookings;

        public CalendarController(
            IDictionary<int, RentalViewModel> rentals,
            IDictionary<int, BookingViewModel> bookings)
        {
            _rentals = rentals;
            _bookings = bookings;
        }

        [HttpGet]
        public CalendarViewModel Get(int rentalId, DateTime start, int nights)
        {
            if (nights < 0)
                throw new ApplicationException("Nights must be positive");
            if (!_rentals.ContainsKey(rentalId))
                throw new ApplicationException("Rental not found");

            var calendarEnd = start.AddDays(nights);
            var allRentalBookings = _bookings.Values.Where(b => b.RentalId == rentalId);
            var rentalBookingsForSelectedTimeFrame = allRentalBookings.Where(b => b.Start >= start || b.Start.AddDays(b.Nights) <= calendarEnd);

            var calendar = CreateCalendar(rentalId, start, nights);
   
            var rental = _rentals[rentalId];
            foreach (var booking in rentalBookingsForSelectedTimeFrame)
            {
                var bookingEnd = booking.Start.AddDays(booking.Nights);
                var calendarBookingStart = booking.Start < start ? start : booking.Start;
                var calendarBookingEnd = bookingEnd > calendarEnd ? calendarEnd : bookingEnd;
                var prepEnd = calendarBookingEnd.AddDays(rental.PreparationTimeInDays) > calendarEnd ? calendarEnd : calendarBookingEnd.AddDays(rental.PreparationTimeInDays);

                int unitForReservation = SelectUnit(calendar.Dates, prepEnd, calendarBookingStart);

                var calendarBooking = new CalendarBookingViewModel
                {
                    Id = booking.Id,
                    Unit = unitForReservation,
                };

                var bookingRangeCalendarDates = calendar.Dates.Where(cvm => calendarBookingStart <= cvm.Date && bookingEnd > cvm.Date);

                foreach (var bookingRangeCalendarDate in bookingRangeCalendarDates)
                    bookingRangeCalendarDate.Bookings.Add(calendarBooking);

                var bookingPreparationTime = new UnitModel
                {
                    Unit = unitForReservation,
                };

                var preparationRangeCalendarDates = calendar.Dates.Where(cvm => bookingEnd < cvm.Date && prepEnd >= cvm.Date);

                foreach (var preparationRangeCalendarDate in preparationRangeCalendarDates)
                    preparationRangeCalendarDate.PreparationTimes.Add(bookingPreparationTime);
            }

            return calendar;
        }

        private CalendarViewModel CreateCalendar(int rentalId, DateTime start, int nights)
        {
            CalendarViewModel cvm = new CalendarViewModel()
            {
                RentalId = rentalId,
                Dates = new List<CalendarDateViewModel>()
            };

            for (var i = 0; i < nights; i++)
            {
                cvm.Dates.Add(new CalendarDateViewModel
                {
                    Date = start.Date.AddDays(i),
                    Bookings = new List<CalendarBookingViewModel>(),
                    PreparationTimes = new List<UnitModel>()
                });
            }

            return cvm;
        }

        private int SelectUnit(List<CalendarDateViewModel> calendarDates, DateTime prepEnd, DateTime calendarBookingStart)
        {
            var bookingsForCalendarPeriod = calendarDates.Where(cvm => calendarBookingStart <= cvm.Date && prepEnd >= cvm.Date).SelectMany(cvm => cvm.Bookings);
            var preparationTimes = calendarDates.Where(cvm => calendarBookingStart <= cvm.Date && prepEnd >= cvm.Date).SelectMany(cvm => cvm.PreparationTimes);

            //get units already booked 
            var unavailableUnits = bookingsForCalendarPeriod.Select(cvm => cvm.Unit).ToList();
            //add units that need to be prepared
            unavailableUnits.AddRange(preparationTimes.Select(um => um.Unit));
            //remove duplicate unit numbers and order them
            unavailableUnits = unavailableUnits.Distinct().OrderBy(unit => unit).ToList();

            int availableUnit = 1;
            for (int i = 0; i < unavailableUnits.Count; i++)
            {
                if (i + 1 != unavailableUnits[i])
                {
                    availableUnit = i + 1;
                    break;
                }

                availableUnit = unavailableUnits[i] + 1;
            }

            return availableUnit;
        }
    }
}
