﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using System.IdentityModel.Tokens.Jwt;
using FeedYourCat.Helpers;
using Microsoft.Extensions.Options;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using FeedYourCat.Services;
using FeedYourCat.Entities;
using FeedYourCat.Models.Feeders;
using FeedYourCat.Models.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Internal;

namespace FeedYourCat.Controllers
{
    [ApiController]
    [Route("")]
    public class FeedersController : ControllerBase
    {
        private IFeederService _feederService;
        private IValidationService _validationService;
        private IHttpContextAccessor _httpContextAccessor;
        private IMapper _mapper;

        public FeedersController(
            IHttpContextAccessor httpContextAccessor,
            IFeederService feederService,
            IValidationService validationService,
            IMapper mapper)
        {
            _httpContextAccessor = httpContextAccessor;
            _feederService = feederService;
            _validationService = validationService;
            _mapper = mapper;
        }

        [HttpGet("/api/admin/feeders")]
        public IActionResult GetAllFeeders()
        {
            string token_base = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            var token = token_base.Split(" ")[1];
            if (!_validationService.ValidateRole(token, "admin"))
            {
                return BadRequest("You are not admin!");
            }
            return Ok(_feederService.GetAllFeeders());
        }

        [HttpGet("/api/admin/moderation/feeders")]
        public IActionResult GetFeederRequests()
        {
            string token_base = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            var token = token_base.Split(" ")[1];
            if (!_validationService.ValidateRole(token, "admin"))
            {
                return BadRequest("You are not admin!");
            }

            var feeders = _mapper.Map<IList<FeederModel>>(_feederService.GetFeederRequests());
            return Ok(feeders);
        }

        [HttpGet("/api/admin/moderated/feeders")]
        public IActionResult GetRegisteredFeeders()
        {
            string token_base = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            var token = token_base.Split(" ")[1];
            if (!_validationService.ValidateRole(token, "admin"))
            {
                return BadRequest("You are not admin!");
            }

            var feeders = _mapper.Map<IList<FeederModel>>(_feederService.GetRegisteredFeeders());
            return Ok(feeders);
        }

        [HttpGet("/api/admin/feeders/approve/{id}")]
        public IActionResult Approve(int id)
        {
            string token_base = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            var token = token_base.Split(" ")[1];
            if (!_validationService.ValidateRole(token, "admin"))
            {
                return BadRequest("You are not admin!");
            }
            _feederService.Approve(id);
            return Ok();
        }

        [HttpDelete("/api/admin/feeders/decline/{id}")]
        public IActionResult Delete(int id)
        {
            string token_base = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            var token = token_base.Split(" ")[1];
            if (!_validationService.ValidateRole(token, "admin"))
            {
                return BadRequest("You are not admin!");
            }

            _feederService.Decline(id);

            return Ok();
        }

        [HttpGet("/api/user/feeders")]
        public IActionResult GetUserFeeders()
        {
            string token_base = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            var token = token_base.Split(" ")[1];
            if (!_validationService.ValidateRole(token, "user"))
            {
                return BadRequest("You are not user!");
            }

            int userId = _validationService.ValidateUserId(token);
            if (userId == -1)
            {
                return BadRequest("You are not user!");
            }
            var feeders = _mapper.Map<IList<FeederModel>>(_feederService.GetUserFeeders(userId));
            return Ok(feeders);
        }

        [HttpPost("/api/user/feeders/register")]
        public IActionResult RegisterFeeder([FromBody] NewFeederModel model)
        {
            string token_base = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            var token = token_base.Split(" ")[1];
            if (!_validationService.ValidateRole(token, "user"))
            {
                return BadRequest("You are not user!");
            }
            int userId = _validationService.ValidateUserId(token);
            if (userId == -1)
            {
                return BadRequest("You are not user!");
            }

            var feeder = _mapper.Map<Feeder>(model);
            feeder.User_Id = userId;
            _feederService.RegisterFeeder(feeder);
            return Ok();
        }

        [HttpPost("/api/user/feeders/redact")]
        public IActionResult RedactFeeder([FromBody] FeederModel model)
        {
            string token_base = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            var token = token_base.Split(" ")[1];
            if (!_validationService.ValidateRole(token, "user"))
            {
                return BadRequest("You are not user!");
            }
            int userId = _validationService.ValidateUserId(token);
            if (userId == -1)
            {
                return BadRequest("You are not user!");
            }

            if (!_validationService.ValidateUserFeeder(model.User_Id, userId))
            {
                return BadRequest("It's not your feeder!");
            }

            var feeder = _mapper.Map<Feeder>(model);
            _feederService.UpdateFeeder(feeder);
            return Ok();
        }

        [HttpGet("/api/user/feeders/fill/{id}")]
        public IActionResult FillFeeder(int id)
        {
            string token_base = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            var token = token_base.Split(" ")[1];
            if (!_validationService.ValidateRole(token, "user"))
            {
                return BadRequest("You are not user!");
            }
            int userId = _validationService.ValidateUserId(token);
            if (userId == -1)
            {
                return BadRequest("You are not user!");
            }

            if (!_validationService.ValidateUserFeeder(id, userId))
            {
                return BadRequest("It's not your feeder!");
            }
            int newFullness = _feederService.FillFeeder(id);
            return Ok(newFullness);
        }
    }
}