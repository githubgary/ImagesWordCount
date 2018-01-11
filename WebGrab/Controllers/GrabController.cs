using System.Threading.Tasks;
using System.Web.Http;

using WebGrab.Models;
using WebGrab.Services;

namespace WebGrab.Controllers
{
    [Route("api/grab")]
    public class GrabController : ApiController
    {
        protected PageParseService _service = new PageParseService();

        // GET: api/<controller>
        [HttpGet]
        public async Task<IHttpActionResult> GetAsync(string uri)
        {
            PageContents pageContents = await _service.ParseAsync(uri);
            if (pageContents == null)
                return NotFound();
            return Ok(pageContents);
        }
    }
}
