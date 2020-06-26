using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserAPI.Contracts.V1.Requests;
using UserAPI.Contracts.V1.Routes;
using UserAPI.RabbitMQ.Producer;
using UserAPI.RabbitMQ.Requests;
using UserAPI.RabbitMQ.Responses;
using Prometheus;

namespace UserAPI.Controllers
{
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserAPIRabbitRPCService _userAPIRabbitRPCService;

        private const string UserIdClaim = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";

        private const string GetUserMethod = "GetUser";
        private const string GetAllUsersMethod = "GetAllUsers";
        private const string CreateUserMethod = "CreateUser";
        private const string DeleteUserMethod = "DeleteUser";
        private const string UpdateUserMethod = "UpdateUser";

        private Counter creationCounter = Metrics.CreateCounter("usersCreated", "Users created counter");
        private Counter deletedCounter = Metrics.CreateCounter("usersDeleted", "Users deleted counter");


        public UserController(ILogger<UserController> logger, IUserAPIRabbitRPCService userAPIRabbitProducer)
        {
            _logger = logger;
            _userAPIRabbitRPCService = userAPIRabbitProducer;
        }

        [Authorize]
        [HttpGet(Routes.UserRoutes.GetAll)]
        public async Task<IActionResult> GetAllAsync([FromQuery] GetAllUserRequest getAllUserRequest)
        {
            if (!User.Claims.Any(x => x.Type == UserIdClaim)) return Unauthorized();
            var id = User.Claims.First(x => x.Type == UserIdClaim).Value;

            _logger.LogInformation($"{nameof(UserController)}.{nameof(GetAllAsync)}: Recieved request.");

            var getAllUsersRabbitRequest = new GetAllUsersRabbitRequest()
            {
                Id = id,
                LocationScope = getAllUserRequest.LocationScope,
                Travellers = getAllUserRequest.Travellers,
                MinAge = getAllUserRequest.MinAge,
                MaxAge = getAllUserRequest.MaxAge,
                GenderOption = getAllUserRequest.GenderOption
            };

            _logger.LogInformation($"{nameof(UserController)}.{nameof(GetAllAsync)}: Sending request to UserService for method {GetAllUsersMethod}.");
            var getAllUsersRabbitResponse = await _userAPIRabbitRPCService.PublishRabbitMessageWaitForResponseAsync<GetAllUsersRabbitResponse>(GetAllUsersMethod, getAllUsersRabbitRequest);

            if (getAllUsersRabbitResponse.FoundUsers)
            {
                return Ok(getAllUsersRabbitResponse.Users);
            }
            else
            {
                return NotFound();
            }
        }

        [Authorize]
        [HttpGet(Routes.UserRoutes.Get)]
        public async Task<IActionResult> GetAsync([FromRoute] string id)
        {
            if (id == "usingtoken")
            {
                if (!User.Claims.Any(x => x.Type == UserIdClaim)) return Unauthorized();
                id = User.Claims.First(x => x.Type == UserIdClaim).Value;
            }

            _logger.LogInformation($"{nameof(UserController)}.{nameof(GetAsync)}: Recieved request.");

            _logger.LogInformation($"{nameof(UserController)}.{nameof(GetAsync)}: Sending request to UserService for method {GetUserMethod}.");
            var getUserRabbitResponse = await _userAPIRabbitRPCService.PublishRabbitMessageWaitForResponseAsync<GetUserRabbitResponse>(GetUserMethod, new GetUserRabbitRequest() { Id = id });
                       
            if (getUserRabbitResponse.FoundUser)
            {
                _logger.LogInformation($"{nameof(UserController)}.{nameof(GetAsync)}: User retrieved with id: {getUserRabbitResponse.User.Id}");

                return Ok(getUserRabbitResponse.User);
            }
            else
            {
                return NotFound();
            }
        }

        [Authorize]
        [HttpPost(Routes.UserRoutes.Create)]
        public async Task<IActionResult> CreateAsync(CreateUserRequest createUserRequest)
        {
            _logger.LogInformation($"{nameof(UserController)}.{nameof(CreateAsync)}: Recieved request.");

            var createUserRabbitRequest = new CreateUserRabbitRequest()
            {
                Username = createUserRequest.Username,
                Email = createUserRequest.Email,
                Password = createUserRequest.Password,
                FirstName = createUserRequest.FirstName,
                LastName = createUserRequest.LastName,
                BirthCountry = createUserRequest.BirthCountry,
                DOB = createUserRequest.DOB,
                Male = createUserRequest.Male
            };

            _logger.LogInformation($"{nameof(UserController)}.{nameof(CreateAsync)}: Sending request to UserService for method {CreateUserMethod}.");
            var createUserRabbitResponse = await _userAPIRabbitRPCService.PublishRabbitMessageWaitForResponseAsync<CreateUserRabbitResponse>(CreateUserMethod, createUserRabbitRequest);

            if (((List<IdentityError>)createUserRabbitResponse.IdentityResult.Errors).Count == 0)
            {
                _logger.LogInformation($"{nameof(UserController)}.{nameof(CreateAsync)}: User successfully created.");
                creationCounter.Inc();
                return Created(Routes.UserRoutes.Get.Replace("{id}", createUserRabbitResponse.User.Id), createUserRabbitResponse.User);
            }
            else
            {
                _logger.LogInformation($"{nameof(UserController)}.{nameof(CreateAsync)}: User creation failed.");
                return BadRequest(createUserRabbitResponse.IdentityResult.Errors);
            }
        }

        [Authorize]
        [HttpPut(Routes.UserRoutes.Update)]
        public async Task<IActionResult> UpdateAsync(UpdateUserRequest updateUserRequest)
        {
            if (!User.Claims.Any(x => x.Type == UserIdClaim)) return Unauthorized();
            var id = User.Claims.First(x => x.Type == UserIdClaim).Value;

            _logger.LogInformation($"{nameof(UserController)}.{nameof(UpdateAsync)}: Recieved request.");

            var updateUserRabbitRequest = new UpdateUserRabbitRequest()
            {
                Id = id,
                Email = updateUserRequest.Email,
                FirstName = updateUserRequest.FirstName,
                LastName = updateUserRequest.LastName,
                BirthCountry = updateUserRequest.BirthCountry,
                Latitude = updateUserRequest.Latitude,
                Longitude = updateUserRequest.Longitude,
                DOB = updateUserRequest.DOB,
                Male = updateUserRequest.Male
            };

            _logger.LogInformation($"{nameof(UserController)}.{nameof(UpdateAsync)}: Sending request to UserService for method {UpdateUserMethod}.");
            var updateUserRabbitResponse = await _userAPIRabbitRPCService.PublishRabbitMessageWaitForResponseAsync<UpdateUserRabbitResponse>(UpdateUserMethod, updateUserRabbitRequest);

            if (((List<IdentityError>)updateUserRabbitResponse.IdentityResult.Errors).Count == 0)
            {
                _logger.LogInformation($"{nameof(UserController)}.{nameof(UpdateAsync)}: User successfully updated.");

                return Ok(updateUserRabbitResponse.User);
            }
            else
            {
                _logger.LogInformation($"{nameof(UserController)}.{nameof(UpdateAsync)}: User update failed.");
                return BadRequest(updateUserRabbitResponse.IdentityResult.Errors);
            }
        }

        [Authorize]
        [HttpDelete(Routes.UserRoutes.Delete)]
        public async Task<IActionResult> DeleteAsync()
        {
            if (!User.Claims.Any(x => x.Type == UserIdClaim)) return Unauthorized();
            var id = User.Claims.First(x => x.Type == UserIdClaim).Value;

            _logger.LogInformation($"{nameof(UserController)}.{nameof(DeleteAsync)}: Recieved request.");

            var deleteUserRabbitResponse = await _userAPIRabbitRPCService.PublishRabbitMessageWaitForResponseAsync<DeleteUserRabbitResponse>(DeleteUserMethod, new DeleteUserRabbitRequest() { Id = id });

            if (deleteUserRabbitResponse.Successful)
            {
                _logger.LogInformation($"{nameof(UserController)}.{nameof(DeleteAsync)}: User successfully deleted.");
                deletedCounter.Inc();
                return Ok();
            }
            else
            {
                _logger.LogInformation($"{nameof(UserController)}.{nameof(DeleteAsync)}: User not found.");
                return NotFound();
            }
        }

        [HttpGet("/testendpoint")]
        public async Task<IActionResult> TestEndpoint()
        {
            return Ok("Showing test endpoint");
        }
    }
}
