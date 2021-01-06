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
    public class RecordsController : ControllerBase
    {
        private readonly RecordContext _context;
        private readonly StreamContext _contextStream;

        public RecordsController(RecordContext context, StreamContext contextStream)
        {
            _context = context;
            _contextStream = contextStream;
        }

        [HttpGet]
        [Route("healthz")]
        public string Healthz()
        {
            return "Active123";
        }

        // GET: api/Records/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Record>> GetRecord(long id)
        {
            var record = await _context.Records.FindAsync(id);

            if (record == null)
            {
                return NotFound();
            }

            return record;
        }

        [HttpGet("monitored/images/{id}")]
        public async Task<ActionResult<ResponseDTO>> GetImageByMonitoredId(string id)
        {
            return new ResponseDTO("Thành công", 200, await _context.Records.Where(s => (s.Type == 0)
                                            && (s.MonitoredObjectId == id)
                                ).ToArrayAsync());
        }

        [HttpGet("monitored/videos/{id}")]
        public async Task<ActionResult<ResponseDTO>> GetVideoByMonitoredId(string id)
        {
            return new ResponseDTO("Thành công", 200, await _context.Records.Where(s => (s.Type == 1)
                                            && (s.MonitoredObjectId == id)
                                ).ToArrayAsync());
        }

        [HttpGet("monitored/{id}")]
        public async Task<ActionResult<ResponseDTO>> GetRecordsByMonitoredId(string id)
        {
            return new ResponseDTO("Thành công", 200, await _context.Records.Where(s =>  (s.MonitoredObjectId == id)
                                ).ToArrayAsync());
        }


        [HttpGet("supervisedArea/images/{id}")]
        public async Task<ActionResult<ResponseDTO>> GetImageBySupervisedAreaId(string id)
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
                case "ALL_PROJECT":
                    problemType = 4;
                    break;
                default:
                    problemType = -1;
                    break;
            }

            return new ResponseDTO("Thành công", 200, await _context.Records.Where(s => (s.Type == 0)
                                            && (s.IdSupervisedArea == id) && (s.ProblemType == problemType)
                                ).ToArrayAsync());
        }

        [HttpGet("supervisedArea/videos/{id}")]
        public async Task<ActionResult<ResponseDTO>> GetVideoBySupervisedAreaId(string id)
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
                case "ALL_PROJECT":
                    problemType = 4;
                    break;
                default:
                    problemType = -1;
                    break;
            }

            return new ResponseDTO("Thành công", 200, await _context.Records.Where(s => (s.Type == 1)
                                            && (s.IdSupervisedArea == id) && (problemType ==4|| s.ProblemType == problemType)
                                ).ToArrayAsync());
        }


        // GET: api/Records/5
        [HttpGet()]
        [Route("image-traning")]
        public async Task<ActionResult<ResponseDTO>> GetRecordTraing()
        {
            return new ResponseDTO("Thành công", 200, await _context.Records.Where(s => (s.IsTraining == true)).ToArrayAsync());
        }


        [HttpPost]
        [Route("search-image-video")]
        public async Task<ActionResult<ResponseDTO>> searchRecords(RecordDTO recordDTO, int skip, int take, int page, int pageSize)
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
                case "ALL_PROJECT":
                    problemType = 4;
                    break;
                default:
                    problemType = -1;
                    break;
            }
            var result = _context.Records.Where(s => (recordDTO.Type == null || s.Type == recordDTO.Type)
                                            && (problemType == 4 ? (recordDTO.ProblemType == null || s.ProblemType == recordDTO.ProblemType) : s.ProblemType == problemType)
                                            && (recordDTO.IsTraining == null || s.IsTraining == recordDTO.IsTraining)
                                            && (recordDTO.Title == null || s.Title.Contains(recordDTO.Title))
                                            && (recordDTO.IdSupervisedArea == null || s.IdSupervisedArea == recordDTO.IdSupervisedArea)
                                            && (recordDTO.IdDrone == null || s.IdDrone == recordDTO.IdDrone)
                                            && (recordDTO.IdCampaign == null || s.IdCampaign == recordDTO.IdCampaign)
                                            && (recordDTO.IdFlightPath == null || s.IdFlightPath == recordDTO.IdFlightPath)
                                            && (recordDTO.MonitoredObjectId == null || s.MonitoredObjectId == recordDTO.MonitoredObjectId)
                                            && (recordDTO.start == null || s.CreatedAt >= recordDTO.start)
                                            && (recordDTO.end == null || s.CreatedAt <= recordDTO.end)
                                );
            return new ResponseDTO("Thành công!", 200, await result.OrderByDescending(c => c.Id).Skip(take == 0 ? page* pageSize : skip* take).Take(take == 0 ? pageSize : take).ToArrayAsync(), result.Count()) ;
        }


        // PUT: api/Records/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseDTO>> PutRecord(long id, RecordDTO recordDTO)
        {
            var record = await _context.Records.FindAsync(id);
            record.Type = recordDTO.Type == null ? record.Type : recordDTO.Type;
            record.Description = recordDTO.Description == null ? record.Description : recordDTO.Description;
            record.ProblemType = recordDTO.ProblemType == null ? record.ProblemType : recordDTO.ProblemType;
            record.IsTraining = recordDTO.IsTraining == null ? record.IsTraining : recordDTO.IsTraining;
            record.Title = recordDTO.Title == null ? record.Title : recordDTO.Title;
            record.IdSupervisedArea = recordDTO.IdSupervisedArea == null ? record.IdSupervisedArea : recordDTO.IdSupervisedArea; 
            record.IdDrone = recordDTO.IdDrone == null ? record.IdDrone : recordDTO.IdDrone;
            record.IdCampaign = recordDTO.IdCampaign == null ? record.IdCampaign : recordDTO.IdCampaign;
            record.IdFlightPath = recordDTO.IdFlightPath == null ? record.IdFlightPath : recordDTO.IdFlightPath;
            record.Longitude = recordDTO.Longitude == null ? record.Longitude : recordDTO.Longitude;
            record.Latitude = recordDTO.Latitude == null ? record.Latitude : recordDTO.Latitude;
            record.MonitoredObjectId = recordDTO.MonitoredObjectId == null ? record.MonitoredObjectId : recordDTO.MonitoredObjectId;
            record.MetaData = recordDTO.MetaData == null ? record.MetaData : recordDTO.MetaData;
            _context.Entry(record).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RecordExists(id))
                {
                    return new ResponseDTO("Không tồn tại ", 404); 
                }
                else
                {
                    throw;
                }
            }

            return new ResponseDTO("Update thành công",200,record);
        }

        // POST: api/Records
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Record>> PostRecord(Record record)
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
                case "ALL_PROJECT":
                    problemType = 4;
                    break;
                default:
                    problemType = -1;
                    break;
            }


            Random rd = new Random();
            record.Title = record.Title != null ? record.Title : (record.Type == 0 ? "Ảnh theo dõi " : "Video theo dõi ") + DateTime.Now.ToString("hh:mm:ss dd/mm/yyyy");
            record.Description = record.Description == null ? record.Title + " ngày " + DateTime.Now.ToString("dd/mm/yyyy") : record.Description;
            record.IsTraining = record.IsTraining == null ? rd.Next(1, 100) < 50 : record.IsTraining;
            record.ProblemType = problemType;

            _context.Records.Add(record);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRecord", new { id = record.Id }, record);
        }

        // POST: api/Records
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        [Route("cut-stream/{streamId}")]
        public async Task<ActionResult<ResponseDTO>> CutStream (Record record,long streamId)
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
                case "ALL_PROJECT":
                    problemType = 4;
                    break;
                default:
                    problemType = -1;
                    break;
            }


            var recordOld = await _contextStream.Streams.FindAsync(streamId);
            if (recordOld == null)
            {
                return new ResponseDTO("Không tồn tại ", 404);
            }
            Random rd = new Random();
            record.Title = "Video cắt gọn " + recordOld.Title;
            record.Description = recordOld.SessionDescription;
            record.IsTraining = record.IsTraining == null ? rd.Next(1, 100) < 50 : record.IsTraining;
            record.ProblemType = problemType;
            record.Type = 1;
            _context.Records.Add(record);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRecord", new { id = record.Id }, record);
        }

        // POST: api/Records
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        [Route("cut-video/{videoId}")]

        public async Task<ActionResult<ResponseDTO>> CutVideo(Record record, long videoId)
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
                case "ALL_PROJECT":
                    problemType = 4;
                    break;
                default:
                    problemType = -1;
                    break;
            }

            var recordOld = await _context.Records.FindAsync(videoId);
            if (recordOld == null)
            {
                return new ResponseDTO("Không tồn tại ", 404);
            }
            Random rd = new Random();
            record.Title = "Video cắt gọn " + recordOld.Title;
            record.Description = recordOld.Description;
            record.IsTraining = record.IsTraining == null ? rd.Next(1, 100) < 50 : record.IsTraining;
            record.ProblemType = problemType;
            record.Type = 1;
            record.Longitude = recordOld.Longitude;
            record.Latitude = recordOld.Latitude;
            record.MetaData = recordOld.MetaData;
            record.MonitoredObjectId = recordOld.MonitoredObjectId;
            record.IdSupervisedArea = recordOld.IdSupervisedArea; 
            record.IdDrone = recordOld.IdDrone;
            record.IdCampaign = recordOld.IdCampaign;
            record.IdFlightPath = recordOld.IdFlightPath;

            _context.Records.Add(record);
            await _context.SaveChangesAsync();

            CreatedAtAction("GetRecord", new { id = record.Id }, record);
            return new ResponseDTO("Lưu thành công", 200, record);

        }

        /// <summary>
        /// Sao chép giá trị đối tượng, nếu đối tượng cần sao chép không có trường đó thì bỏ qua
        /// </summary>
        /// <typeparam name="T">object</typeparam>
        /// <param name="sourceObject">Đầu vào bản cần sao chép</param>
        /// <param name="destObject">Đối tượng cần lấy sau khi sao chép</param>
        /// Created by: BVMINH (24/08/2020)
        public static void CopyObject<T>(object sourceObject, ref T destObject)
        {
            if (sourceObject == null || destObject == null)
            {
                return;
            }
            Type sourceType = sourceObject.GetType();
            Type targetType = destObject.GetType();
            foreach (PropertyInfo p in sourceType.GetProperties())
            {
                //  Get the matching property in the destination object
                PropertyInfo targetObj = targetType.GetProperty(p.Name);
                //  If there is none, skip
                if (targetObj == null)
                    continue;
                //set value
                targetObj.SetValue(destObject, p.GetValue(sourceObject, null), null);
            }
        }

        // DELETE: api/Records/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Record>> DeleteRecord(long id)
        {
            var record = await _context.Records.FindAsync(id);
            if (record == null)
            {
                return NotFound();
            }

            _context.Records.Remove(record);
            await _context.SaveChangesAsync();

            return record;
        }

        private bool RecordExists(long id)
        {
            return _context.Records.Any(e => e.Id == id);
        }
    }
}
