//
// TornBot
//
// Copyright (C) 2024 TornBot.com
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TornBot.Services.TornBotWeb.API.Controllers.v1.Spy
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class SpyController : ControllerBase
    {
        // GET: api/v1/<SpyController>
        [HttpGet]
        public IActionResult Get()
        {
            //{ "error": { "code" => 2, "error" => "Invalid API key" }}
            return BadRequest(Common.Common.GetErrorInvalidAPIKey());
        }

        // GET api/<SpyController>/1234?key=ApiKey
        [HttpGet("{id}")]
        [SpyRequestResultFilter]
        public string Get([FromRoute] SpyModel model)
        {
            return "TEST: id: " + model.id + " | key: " + model.key;
        }
    }
}
