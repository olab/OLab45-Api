using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace OLabWebAPI.Endpoints.WebApi
{
  [Route("olab/api/v3/courses")]
  [ApiController]
  public class CoursesController : ControllerBase
  {
    // GET: api/Courses
    [HttpGet]
    public IEnumerable<string> Get()
    {
      return new string[] { "value1", "value2" };
    }

    // GET: api/Courses/5
    [HttpGet("{id}", Name = "Get")]
    public string Get(int id)
    {
      return "value";
    }

    // POST: api/Courses
    [HttpPost]
    public void Post([FromBody] string value)
    {
    }

    // PUT: api/Courses/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }

    // DELETE: api/ApiWithActions/5
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
  }
}
