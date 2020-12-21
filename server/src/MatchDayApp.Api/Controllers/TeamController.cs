﻿using AutoMapper;
using MatchDayApp.Api.Extensions;
using MatchDayApp.Infra.CrossCutting.Contract.V1.Request.Query;
using MatchDayApp.Infra.CrossCutting.Contract.V1.Request.Team;
using MatchDayApp.Infra.CrossCutting.Contract.V1.Response;
using MatchDayApp.Infra.CrossCutting.Helpers;
using MatchDayApp.Infra.CrossCutting.Services.Interfaces;
using MatchDayApp.Infra.CrossCutting.V1;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MatchDayApp.Api.Controllers
{
    /// <summary>
    /// Endpoint responsible for manage teams
    /// </summary>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    public class TeamController : ControllerBase
    {
        private readonly ITeamAppService _teamService;
        private readonly IUriService _uriService;
        private readonly IMapper _mapper;

        public TeamController(ITeamAppService teamService, IMapper mapper, IUriService uriService)
        {
            _teamService = teamService
                ?? throw new ArgumentNullException(nameof(teamService));
            _mapper = mapper
                ?? throw new ArgumentNullException(nameof(mapper));
            _uriService = uriService
                ?? throw new ArgumentNullException(nameof(uriService));
        }

        /// <summary>
        /// Returns all teams in the system
        /// </summary>
        /// <param name="pagination">Pagination options</param>
        /// <response code="200">Returns all teams in the system</response>
        [HttpGet(ApiRotas.Team.GetAll)]
        [ProducesResponseType(typeof(PagedResponse<TimeResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAll([FromQuery] PaginationQuery pagination)
        {
            var teams = await _teamService
                .GetTeamsListAsync(pagination);

            var teamsResponse = _mapper
                .Map<IReadOnlyList<TimeResponse>>(teams);

            if (pagination == null || pagination.PageNumber < 1 || pagination.PageSize < 1)
            {
                return Ok(new PagedResponse<TimeResponse>(teamsResponse));
            }

            var paginationResponse = PaginationHelpers
                .CreatePaginatedResponse(_uriService, pagination, teamsResponse.ToList());

            return Ok(paginationResponse);
        }

        /// <summary>
        /// Return team by id
        /// </summary>
        /// <param name="teamId">Team identification in the system</param>
        /// <response code="200">Return team by id</response>
        /// <response code="404">Not found team</response>
        [HttpGet(ApiRotas.Team.Get)]
        [ProducesResponseType(typeof(Response<TimeResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            var team = await _teamService
                .GetTeamByIdAsync(id);

            if (team is null)
                return NotFound();

            return Ok(new Response<TimeResponse>(_mapper
                .Map<TimeResponse>(team)));
        }

        /// <summary>
        /// Create team
        /// </summary>
        /// <response code="201">Created team</response>
        /// <response code="400">An error when create team</response>
        [HttpPost(ApiRotas.Team.Create)]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateTeamRequest request)
        {
            request.OwnerUserId = Guid
                .Parse(HttpContext.GetUserId());

            var result = await _teamService
                .AddTeamAsync(request);

            if (!result)
            {
                return BadRequest(new Response<object>
                {
                    Message = "Ocorreu um erro ao adicionar time",
                    Succeeded = false
                });
            }

            return Created(string.Empty, new Response<object>
            {
                Message = "Time adicionado com sucesso",
                Succeeded = true
            });
        }

        /// <summary>
        /// Update team
        /// </summary>
        /// <response code="200">Updated team</response>
        /// <response code="400">An error when update team</response>
        /// <response code="404">Not found team</response>
        [HttpPut(ApiRotas.Team.Update)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] AtualizarTimeRequest request)
        {
            var result = await _teamService
                .UpdateTeamAsync(id, request);

            if (!result)
            {
                return BadRequest(new Response<object>
                {
                    Message = "Ocorreu um erro ao atualizar time",
                    Succeeded = false
                });
            }

            return Ok(new Response<object>
            {
                Message = "Time atualizado com sucesso",
                Succeeded = true
            });
        }

        /// <summary>
        /// Delete team
        /// </summary>
        /// <response code="204">Deleted team</response>
        /// <response code="404">Not found team</response>
        [HttpDelete(ApiRotas.Team.Delete)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var result = await _teamService
                .DeleteTeamAsync(id);

            if (!result)
            {
                return BadRequest(new Response<object>
                {
                    Message = "Ocorreu um erro ao deletar time",
                    Succeeded = false
                });
            }

            return NoContent();
        }
    }
}
