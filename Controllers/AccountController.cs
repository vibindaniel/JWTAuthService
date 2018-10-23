using AutoMapper;
using DuploAuth.Helpers;
using DuploAuth.Models.Entities;
using DuploAuth.Models.ViewModels;
using DuploAuth.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using static DuploAuth.Helpers.Constants.Strings;

namespace DuploAuth.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    [AuthorizeRoles(JwtClaims.Admin, JwtClaims.SuperAdmin)]
    public class AccountController : ControllerBase
    {
        private readonly ApplicationDbContext _appDbContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public AccountController(UserManager<AppUser> userManager, IMapper mapper, ApplicationDbContext appDbContext)
        {
            _userManager = userManager;
            _mapper = mapper;
            _appDbContext = appDbContext;
        }

        [HttpGet]
        [Route("users")]
        public async Task<IActionResult> Users()
        {
            return Ok(await _userManager.Users.ToListAsync());
        }

        [HttpGet("me")]
        [AuthorizeRoles]
        public async Task<IActionResult> GetMe()
        {
            var email = User.FindFirst(ClaimTypes.Email).Value;
            return Ok(await _userManager.FindByEmailAsync(email));
        }

        [HttpPost]
        [Route("AddUser")]
        public async Task<IActionResult> AddUser([FromBody] UserViewModel user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdentity = _mapper.Map<AppUser>(user);
            var result = await _userManager.CreateAsync(userIdentity);
            if (!result.Succeeded)
            {
                return new BadRequestObjectResult(Errors.AddErrorsToModelState(result, ModelState));
            }
            _userManager.AddToRoleAsync(userIdentity, user.Role).Wait();

            var inviteSubject = string.Empty;
            var inviteBody = string.Empty;
            var variable = await _appDbContext.Variables.FirstOrDefaultAsync(x => x.Name == "invite_subject");
            try
            {
                inviteSubject = variable.Value;
            }
            catch (NullReferenceException) { }
            variable = await _appDbContext.Variables.FirstOrDefaultAsync(x => x.Name == "invite_message");
            try
            {
                inviteBody = variable.Value;
            }
            catch (NullReferenceException) { }

            try
            {
                EmailRepository.EmailSender(_appDbContext, inviteBody, user.Email, user.Name, inviteSubject);
            }
            catch (NullReferenceException) { }

            return Ok();
        }

        [HttpPost]
        [Route("UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromBody] UserViewModel user)
        {
            if (string.IsNullOrEmpty(user.Email))
            {
                return BadRequest("Email is required");
            }

            if (user.Role == JwtClaims.SuperAdmin)
            {
                return BadRequest("Super Admin cannot be modified");
            }

            var userInDb = await _userManager.FindByEmailAsync(user.Email);
            if (userInDb != null)
            {
                foreach (var prop in user.GetType().GetProperties())
                {
                    if (prop.Name == nameof(userInDb.Role))
                    {
                        _userManager.RemoveFromRoleAsync(userInDb, userInDb.Role).Wait();
                        _userManager.AddToRoleAsync(userInDb, user.Role).Wait();
                    }
                    userInDb.GetType().GetProperty(prop.Name).SetValue(userInDb, prop.GetValue(user));
                }
                var result = await _userManager.UpdateAsync(userInDb);
                if (!result.Succeeded)
                {
                    return new BadRequestObjectResult(Errors.AddErrorToResponse(result));
                }

                return Ok();
            }

            return BadRequest("User not found");
        }

        [HttpDelete("DeleteUser/{Email}")]
        public async Task<IActionResult> DeleteUser(string Email)
        {
            var userInDb = await _userManager.FindByEmailAsync(Email);

            if (userInDb != null && userInDb.Role != JwtClaims.SuperAdmin)
            {
                var result = await _userManager.DeleteAsync(userInDb);
                if (!result.Succeeded)
                {
                    return new BadRequestObjectResult(Errors.AddErrorToResponse(result));
                }

                return Ok();
            }

            return BadRequest($"User with email {Email} not Found");
        }
    }
}