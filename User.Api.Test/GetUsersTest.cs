using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using User.Api.Features.Users;
using User.API.Common.Models;

namespace User.Api.Test
{
    public class GetUsersTest
    {
        [Fact]
        public void Validate()
        {
            Assert.True(true);
        }

        [Fact]
        public async void GetUsersUsingHappyFlow()
        {
            // Arrange
            var controller = new GetUsersController();
            var query = new GetUsersWithPaginationQuery(1, 10);

            // Act
            var result = await controller.GetUsers(query);
            

            //Assert
            Assert.IsType<PaginatedList<Domain.Entities.User>>(result);
            Assert.Equal(1, result.TotalCount);
        }
    }
}
