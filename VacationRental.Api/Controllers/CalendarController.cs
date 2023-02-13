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

            //var result = new CalendarViewModel
            //{
            //    RentalId = rentalId,
            //    Dates = new List<CalendarDateViewModel>()
            //};

            var calendarEnd = start.AddDays(nights);
            var allRentalBookings = _bookings.Values.Where(b => b.RentalId == rentalId);
            var rentalBookingsForSelectedTimeFrame = allRentalBookings.Where(b => b.Start.Date >= start.Date && b.Start.AddDays(b.Nights).Date <= calendarEnd);

            var calendar = CreateCalendar(rentalId, start, nights);
            //for (var i = 0; i < nights; i++)
            //{
            //    var date = new CalendarDateViewModel
            //    {
            //        Date = start.Date.AddDays(i),
            //        Bookings = new List<CalendarBookingViewModel>(),
            //        PreparationTimes = new List<UnitModel>()
            //    };

            //    //foreach (var booking in _bookings.Values.Where(b => b.RentalId == rentalId))
            //    //{
            //    //    if (booking.Start <= date.Date && booking.Start.AddDays(booking.Nights) > date.Date)
            //    //    {
            //    //        date.Bookings.Add(new CalendarBookingViewModel { Id = booking.Id });
            //    //    }
            //    //}

            var rental = _rentals[rentalId];
            foreach (var booking in rentalBookingsForSelectedTimeFrame)
            {

                var bookingEnd = booking.Start.AddDays(booking.Nights);
                var calendarBookingStart = booking.Start < start ? start : booking.Start;
                var calendarBookingEnd = bookingEnd > calendarEnd ? calendarEnd : bookingEnd;
                var prepEnd = calendarBookingEnd.AddDays(rental.PreparationTimeInDays) > calendarEnd ? calendarEnd : calendarBookingEnd.AddDays(rental.PreparationTimeInDays);

                int unitForReservation = SelectUnit(calendar.Dates, prepEnd, calendarBookingStart);
            }

            //    result.Dates.Add(date);
            //}



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
            var designatedPeriodCalendarBookings = calendarDates.Where(date => calendarBookingStart <= date.Date && prepEnd >= date.Date).SelectMany(date => date.Bookings);
            var preparationTimes = calendarDates.Where(date => calendarBookingStart <= date.Date && prepEnd >= date.Date).SelectMany(date => date.PreparationTimes);

            var unavailableUnits = designatedPeriodCalendarBookings.Select(cBooking => cBooking.Unit).ToList();
            unavailableUnits.AddRange(preparationTimes.Select(time => time.Unit));
            unavailableUnits = unavailableUnits.Distinct().OrderBy(unit => unit).ToList();

            int choosenUnit = 1;
            for (int i = 0; i < unavailableUnits.Count; i++)
            {
                if (i + 1 != unavailableUnits[i])
                {
                    choosenUnit = i + 1;
                    break;
                }

                choosenUnit = unavailableUnits[i] + 1;
            }

            return choosenUnit;
        }
    }
}
