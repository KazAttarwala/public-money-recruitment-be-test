using System;
using System.Net.Http;
using System.Threading.Tasks;
using VacationRental.Api.Models;
using Xunit;

namespace VacationRental.Api.Tests
{
    [Collection("Integration")]
    public class PostBookingTests
    {
        private readonly HttpClient _client;
        private readonly DateTime _currentDate = DateTime.Now;

        public PostBookingTests(IntegrationFixture fixture)
        {
            _client = fixture.Client;
        }

        [Fact]
        public async Task GivenCompleteRequest_WhenPostBooking_ThenAGetReturnsTheCreatedBooking()
        {
            var postRentalRequest = new RentalBindingModel
            {
                Units = 4,
                PreparationTimeInDays = 3
            };

            ResourceIdViewModel postRentalResult;
            using (var postRentalResponse = await _client.PostAsJsonAsync($"/api/v1/rentals", postRentalRequest))
            {
                Assert.True(postRentalResponse.IsSuccessStatusCode);
                postRentalResult = await postRentalResponse.Content.ReadAsAsync<ResourceIdViewModel>();
            }

            var postBookingRequest = new BookingBindingModel
            {
                RentalId = postRentalResult.Id,
                Nights = 3,
                Start = _currentDate.AddDays(2)
            };

            ResourceIdViewModel postBookingResult;
            using (var postBookingResponse = await _client.PostAsJsonAsync($"/api/v1/bookings", postBookingRequest))
            {
                Assert.True(postBookingResponse.IsSuccessStatusCode);
                postBookingResult = await postBookingResponse.Content.ReadAsAsync<ResourceIdViewModel>();
            }

            using (var getBookingResponse = await _client.GetAsync($"/api/v1/bookings/{postBookingResult.Id}"))
            {
                Assert.True(getBookingResponse.IsSuccessStatusCode);

                var getBookingResult = await getBookingResponse.Content.ReadAsAsync<BookingViewModel>();
                Assert.Equal(postBookingRequest.RentalId, getBookingResult.RentalId);
                Assert.Equal(postBookingRequest.Nights, getBookingResult.Nights);
                Assert.Equal(postBookingRequest.Start, getBookingResult.Start);
            }
        }

        [Fact]
        public async Task GivenCompleteRequest_WhenBookingMultipleUnits_ThenAGetReturnsTheCreatedBookings()
        { 
            var postRentalRequest = new RentalBindingModel
            {
                Units = 2,
                PreparationTimeInDays = 3
            };

            ResourceIdViewModel postRentalResult;
            using (var postRentalResponse = await _client.PostAsJsonAsync($"/api/v1/rentals", postRentalRequest))
            {
                Assert.True(postRentalResponse.IsSuccessStatusCode);
                postRentalResult = await postRentalResponse.Content.ReadAsAsync<ResourceIdViewModel>();
            }

            var postBookingRequest1 = new BookingBindingModel
            {
                RentalId = postRentalResult.Id,
                Nights = 3,
                Start = _currentDate
            };

            ResourceIdViewModel postBookingResult1;
            using (var postBookingResponse = await _client.PostAsJsonAsync($"/api/v1/bookings", postBookingRequest1))
            {
                Assert.True(postBookingResponse.IsSuccessStatusCode);
                postBookingResult1 = await postBookingResponse.Content.ReadAsAsync<ResourceIdViewModel>();
            }

            var postBookingRequest2 = new BookingBindingModel
            {
                RentalId = postRentalResult.Id,
                Nights = 2,
                Start = _currentDate
            };

            ResourceIdViewModel postBookingResult2;
            using (var postBookingResponse = await _client.PostAsJsonAsync($"/api/v1/bookings", postBookingRequest2))
            {
                Assert.True(postBookingResponse.IsSuccessStatusCode);
                postBookingResult2 = await postBookingResponse.Content.ReadAsAsync<ResourceIdViewModel>();
            }

            var postBookingRequest3 = new BookingBindingModel
            {
                RentalId = postRentalResult.Id,
                Nights = 2,
                Start = _currentDate.AddDays(postRentalRequest.PreparationTimeInDays + 2)//DateTime.Now.AddDays(postRentalRequest.PreparationTimeInDays + 1)
            };

            ResourceIdViewModel postBookingResult3;
            using (var postBookingResponse = await _client.PostAsJsonAsync($"/api/v1/bookings", postBookingRequest3))
            {
                Assert.True(postBookingResponse.IsSuccessStatusCode);
                postBookingResult3 = await postBookingResponse.Content.ReadAsAsync<ResourceIdViewModel>();
            }

            using (var getBookingResponse = await _client.GetAsync($"/api/v1/bookings/{postBookingResult1.Id}"))
            {
                Assert.True(getBookingResponse.IsSuccessStatusCode);

                var getBookingResult = await getBookingResponse.Content.ReadAsAsync<BookingViewModel>();
                Assert.Equal(postBookingRequest1.RentalId, getBookingResult.RentalId);
                Assert.Equal(postBookingRequest1.Nights, getBookingResult.Nights);
                Assert.Equal(postBookingRequest1.Start, getBookingResult.Start);
            }
            using (var getBookingResponse = await _client.GetAsync($"/api/v1/bookings/{postBookingResult2.Id}"))
            {
                Assert.True(getBookingResponse.IsSuccessStatusCode);

                var getBookingResult = await getBookingResponse.Content.ReadAsAsync<BookingViewModel>();
                Assert.Equal(postBookingRequest2.RentalId, getBookingResult.RentalId);
                Assert.Equal(postBookingRequest2.Nights, getBookingResult.Nights);
                Assert.Equal(postBookingRequest2.Start, getBookingResult.Start);
            }
            using (var getBookingResponse = await _client.GetAsync($"/api/v1/bookings/{postBookingResult3.Id}"))
            {
                Assert.True(getBookingResponse.IsSuccessStatusCode);

                var getBookingResult = await getBookingResponse.Content.ReadAsAsync<BookingViewModel>();
                Assert.Equal(postBookingRequest3.RentalId, getBookingResult.RentalId);
                Assert.Equal(postBookingRequest3.Nights, getBookingResult.Nights);
                Assert.Equal(postBookingRequest3.Start, getBookingResult.Start);
            }
        }

        [Fact]
        public async Task GivenCompleteRequest_WhenPostBooking_ThenAPostReturnsErrorWhenBookingSameUnit()
        {
            var postRentalRequest = new RentalBindingModel
            {
                Units = 1,
                PreparationTimeInDays = 0
            };

            ResourceIdViewModel postRentalResult;
            using (var postRentalResponse = await _client.PostAsJsonAsync($"/api/v1/rentals", postRentalRequest))
            {
                Assert.True(postRentalResponse.IsSuccessStatusCode);
                postRentalResult = await postRentalResponse.Content.ReadAsAsync<ResourceIdViewModel>();
            }

            var postBooking1Request = new BookingBindingModel
            {
                RentalId = postRentalResult.Id,
                Nights = 3,
                Start = _currentDate
            };

            using (var postBooking1Response = await _client.PostAsJsonAsync($"/api/v1/bookings", postBooking1Request))
            {
                Assert.True(postBooking1Response.IsSuccessStatusCode);
            }

            var postBooking2Request = new BookingBindingModel
            {
                RentalId = postRentalResult.Id,
                Nights = 1,
                Start = _currentDate.AddDays(1)
            };

            await Assert.ThrowsAsync<ApplicationException>(async () =>
            {
                using (var postBooking2Response = await _client.PostAsJsonAsync($"/api/v1/bookings", postBooking2Request))
                {
                }
            });
        }

        [Fact]
        public async Task GivenCompleteRequest_WhenPostBooking_ThenAPostReturnsErrorWhenBookingSameUnitDuringPreparation()
        {
            var postRentalRequest = new RentalBindingModel
            {
                Units = 1,
                PreparationTimeInDays = 2
            };

            ResourceIdViewModel postRentalResult;
            using (var postRentalResponse = await _client.PostAsJsonAsync($"/api/v1/rentals", postRentalRequest))
            {
                Assert.True(postRentalResponse.IsSuccessStatusCode);
                postRentalResult = await postRentalResponse.Content.ReadAsAsync<ResourceIdViewModel>();
            }

            var postBooking1Request = new BookingBindingModel
            {
                RentalId = postRentalResult.Id,
                Nights = 3,
                Start = _currentDate
            };

            using (var postBooking1Response = await _client.PostAsJsonAsync($"/api/v1/bookings", postBooking1Request))
            {
                Assert.True(postBooking1Response.IsSuccessStatusCode);
            }

            var postBooking2Request = new BookingBindingModel
            {
                RentalId = postRentalResult.Id,
                Nights = 1,
                Start = _currentDate.AddDays(4)
            };

            await Assert.ThrowsAsync<ApplicationException>(async () =>
            {
                using (var postBooking2Response = await _client.PostAsJsonAsync($"/api/v1/bookings", postBooking2Request))
                {
                }
            });
        }

        [Fact]
        public async Task GivenCompleteRequest_WhenPostBooking_ThenAPostReturnsErrorWhenBookingBeforeCurrentDay()
        {
            var postRentalRequest = new RentalBindingModel
            {
                Units = 1,
                PreparationTimeInDays = 2
            };

            ResourceIdViewModel postRentalResult;
            using (var postRentalResponse = await _client.PostAsJsonAsync($"/api/v1/rentals", postRentalRequest))
            {
                Assert.True(postRentalResponse.IsSuccessStatusCode);
                postRentalResult = await postRentalResponse.Content.ReadAsAsync<ResourceIdViewModel>();
            }

            var postBooking1Request = new BookingBindingModel
            {
                RentalId = postRentalResult.Id,
                Nights = 3,
                Start = new DateTime(2020, 02, 16)
            };

            await Assert.ThrowsAsync<ApplicationException>(async () =>
            {
                using (var postBooking2Response = await _client.PostAsJsonAsync($"/api/v1/bookings", postBooking1Request))
                {
                }
            });
        }
    }
}
