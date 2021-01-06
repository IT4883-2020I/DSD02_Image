using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IT4483Image.Models;
using IT4483Image.DTO;
using System.Reflection;

namespace IT4483Image.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StreamsController : ControllerBase
    {
        private readonly StreamContext _context;
        private readonly RecordContext _contextRecord;

        public StreamsController(StreamContext context, RecordContext contextRecord)
        {
            _context = context;
            _contextRecord = contextRecord;
        }

        [HttpGet]
        [Route("healthz")]
        public string Healthz()
        {
            return "Active123";
        }

        // GET: api/Streams/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Stream>> GetStream(long id)
        {
            var Stream = await _context.Streams.FindAsync(id);

            if (Stream == null)
            {
                return NotFound();
            }

            return Stream;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseDTO>> getStreams()
        {
            var projectType = HttpContext.Request.Headers["project-type"].ToString();
            var problemType = 0;
            switch (projectType)
            {
                case "CHAY_RUNG":
                    problemType = 0;
                    break;
                case "DE_DIEU":
                    problemType = 1;
                    break;
                case "LUOI_DIEN":
                    problemType = 2;
                    break;
                case "CAY_TRONG":
                    problemType = 3;
                    break;
                default:
                    problemType = 5;
                    break;
            }

            return new ResponseDTO("Thành công!", 200, await _context.Streams.Where(s =>
                                             (s.ProblemType == problemType)
                                ).OrderByDescending(c => c.Id).ToArrayAsync());
        }



        [HttpGet]
        [Route("active")]

        public async Task<ActionResult<ResponseDTO>> getActiveStreams()
        {
            var projectType = HttpContext.Request.Headers["project-type"].ToString();
            var problemType = 0;
            switch (projectType)
            {
                case "CHAY_RUNG":
                    problemType = 0;
                    break;
                case "DE_DIEU":
                    problemType = 1;
                    break;
                case "LUOI_DIEN":
                    problemType = 2;
                    break;
                case "CAY_TRONG":
                    problemType = 3;
                    break;
                default:
                    problemType = 5;
                    break;
            }

            return new ResponseDTO("Thành công!", 200, await _context.Streams.Where(s => 
                                             (s.ProblemType == problemType)&&(s.StopTime==s.StartTime)
                                ).OrderByDescending(c => c.Id).ToArrayAsync());
        }


        // POST: api/Streams
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        [Route("start-stream")]
        public async Task<ActionResult<Stream>> StartStream(Stream Stream)
        {
            var projectType = HttpContext.Request.Headers["project-type"].ToString();
            var problemType = 0;
            switch (projectType)
            {
                case "CHAY_RUNG":
                    problemType = 0;
                    break;
                case "DE_DIEU":
                    problemType = 1;
                    break;
                case "LUOI_DIEN":
                    problemType = 2;
                    break;
                case "CAY_TRONG":
                    problemType = 3;
                    break;
                default:
                    problemType = 5;
                    break;
            }

            Stream.Title = Stream.Title != null ? Stream.Title : "Video trực tiếp " + DateTime.Now.ToString("hh:mm:ss dd/mm/yyyy");
            Stream.ProblemType = problemType;
            Stream.StopTime = Stream.StartTime;
            _context.Streams.Add(Stream);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetStream", new { id = Stream.Id }, Stream);
        }

        [HttpPut]
        [Route("stop-stream/{id}")]
        public async Task<ActionResult<ResponseDTO>> StopStream(Stream streamDTO, long id)
        {
            var stream = await _context.Streams.FindAsync(id);

            stream.StopTime = streamDTO.StopTime;
            stream.Link = streamDTO.Link;
            stream.MetaData = streamDTO.MetaData == null ? stream.MetaData : streamDTO.MetaData;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StreamExists(id))
                {
                    return new ResponseDTO("không tồn tại ", 404);
                }
                else
                {
                    throw;
                }
            }
            Record record = new Record();
            record.Title =  "Video từ stream theo dõi " + DateTime.Now.ToString("hh:mm:ss dd/mm/yyyy");
            record.Description = stream.SessionDescription;
            record.MetaData = stream.MetaData;
            record.Type = 1;
            _contextRecord.Records.Add(record);
            await _context.SaveChangesAsync();
            CreatedAtAction("GetRecord", new { id = record.Id }, record);
            return new ResponseDTO("Update thành công", 200, stream);
        }



        private bool StreamExists(long id)
        {
            return _context.Streams.Any(e => e.Id == id);
        }
    }
}
