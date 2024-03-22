using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Project.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EntitiesController : ControllerBase
    {
        private readonly IDatabaseService _databaseService;

        public EntitiesController(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [HttpGet]
        public IActionResult Get(int pageNumber = 1, int pageSize = 10, string sortBy = "Id", string sortOrder = "asc")
        {
            var entities = _databaseService.GetEntities(); // Retrieve entities from data source

            // Apply sorting
            switch (sortBy.ToLower())
            {
                case "gender":
                    entities = sortOrder.ToLower() == "asc" ? entities.OrderBy(e => e.Gender) : entities.OrderByDescending(e => e.Gender);
                    break;
                case "birthdate":
                    entities = sortOrder.ToLower() == "asc" ? entities.OrderBy(e => e.Dates.FirstOrDefault(d => d.DateType == "BirthDate").DateValue) : entities.OrderByDescending(e => e.Dates.FirstOrDefault(d => d.DateType == "BirthDate").DateValue);
                    break;
                default:
                    entities = sortOrder.ToLower() == "asc" ? entities.OrderBy(e => e.Id) : entities.OrderByDescending(e => e.Id);
                    break;
            }

            // Apply pagination
            var paginatedEntities = entities.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            return Ok(paginatedEntities);
        }


        // Implement other CRUD endpoints as needed
        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            // Implement logic to retrieve entity by ID
            var entity = _databaseService.GetEntityById(id);
            if (entity == null)
            {
                return NotFound(); // Return 404 Not Found if entity with given ID is not found
            }
            return Ok(entity);
        }
        
        [HttpGet("search")]
        public IActionResult Search(string search)
        {
            // Implement logic to search entities by provided search text
            var entities = _databaseService.SearchEntities(search);
            return Ok(entities);
        }
        
        [HttpGet("filter")]
        public IActionResult Filter(string gender, DateTime? startDate, DateTime? endDate, string[] countries)
        {
            // Implement logic to filter entities based on provided parameters
            var filteredEntities = _databaseService.FilterEntities(gender, startDate, endDate, countries);
            return Ok(filteredEntities);
        }

    }
}