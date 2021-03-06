﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomentuumApi.Model.CivicTrack;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using MomentuumApi.Utils;

namespace MomentuumApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class CaseController : Controller
    {
        private readonly CivicTrackContext _context;

        public CaseController(CivicTrackContext context)
        {
            _context = context;
        }

        // GET: api/case
        [HttpGet, Authorize]
        public IEnumerable<TblCase> GetTblCase()
        {
            return _context.TblCase.Where(emp => emp.Deleted.Equals("false")).ToList();
        }

        /// <summary>
        /// Gets the counts of open, closed and total cases assigned to an employee
        /// </summary>
        /// <returns>The case counts for employee.</returns>
        [HttpGet("stats"), Authorize]
        public IActionResult GetCaseCountsForEmployee()
        {
            var user = JwtHelper.GetUser(HttpContext.User.Claims);
            var allCases = _context.TblCase
                                   .Where(x => x.CaseAssignedTo == user);

            var caseStats = new CaseStats();

            var closedCount = allCases.Where(x => x.Casestatus.Contains("Closed")).Count();
            var openCount = allCases.Where(x => x.Casestatus.Contains("Open") || x.Casestatus.Contains("Awaiting Info")).Count();
            var totalCount = allCases.Count();

            caseStats.ClosedCount = closedCount;
            caseStats.OpenCount = openCount;
            caseStats.TotalCount = totalCount;

            return new ObjectResult(caseStats);
        }

        [HttpGet("caseInfo"), Authorize]
        public IActionResult GetCaseInfo()
        {
            var user = JwtHelper.GetUser(HttpContext.User.Claims);
            var div = _context.TblEmployees.FirstOrDefault(empl => empl.EmployeeLogin.Equals(user)).Riding;

            var caseInfo = new CaseInfo();

            var caseType = _context.TblCaseType.Where(x => x.listtext != null && x.listtext != "" && x.id.Equals(div)).ToList();
            var caseStatus = _context.TblCaseStatus.Where(x => x.listtext != null && x.listtext != "" && x.id.Equals(div)).Distinct().ToList();
            var caseAssignedTo = _context.TblEmployees.Where(x => x.Riding.Equals(div)).ToList();

            caseInfo.casetype = caseType;
            caseInfo.casestatus = caseStatus;
            caseInfo.caseassignedto = caseAssignedTo;

            return new ObjectResult(caseInfo);
        }


        // GET api/case/{id}
        // getting the single case based on id
        [HttpGet("{id}"), Authorize]
        public IEnumerable<TblCase> GetCaseById(int id)
        {
            return _context.TblCase.Where(emp => emp.Caseid.Equals(id) && emp.Deleted.Equals("false")).ToList();
        }


        // GET api/case/emp/{id}
        // getting all the cases assigned to the Employee based on empployeelogin
        [HttpGet("emp/{id}"), Authorize]
        public IEnumerable<TblCase> GetCaseByEmpId(string id)
        {
            return _context.TblCase.Where(emp => emp.CaseAssignedTo.Equals(id) && emp.Deleted.Equals("false")).ToList();
        }



        // GET: api/case/client/emp
        // getting all the cases with client details assigned to the Employee based on employeelogin
        [HttpGet("client/emp"), Authorize]
        public IActionResult GetCaseClientByEmpJwt()
        {
            var user = JwtHelper.GetUser(HttpContext.User.Claims);
            var clientCase = _context.TblCase
                .Join(_context.TblVoter, cas => cas.IdVoter, cli => cli.Id, (cas, cli) => new { cas, cli })
                .Where(x => x.cas.CaseAssignedTo == user && x.cas.Deleted.Equals("false"))
				.Where(x => x.cas.CaseAssignedTo == user && !x.cas.Casestatus.Equals("Closed"))

				.ToList();

            if (clientCase == null)
            {
                return NotFound();
            }
            return new ObjectResult(clientCase);
        }

        // GET: api/case/client/{id}
        // getting all the cases with client details by case id
        [HttpGet("client/{id}"), Authorize]
        public IActionResult GetCaseClientById(int id)
        {

            var user = JwtHelper.GetUser(HttpContext.User.Claims);
            var div = _context.TblEmployees.FirstOrDefault(empl => empl.EmployeeLogin.Equals(user)).Riding;

            var caseInfo = new CaseInfo();

            var clientCase = _context.TblCase
                .Join(_context.TblVoter, cas => cas.IdVoter, cli => cli.Id, (cas, cli) => new { cas, cli })
                .FirstOrDefault(x => x.cas.Caseid == id && x.cas.Deleted.Equals("false"));

            var caseCode = _context.TblCaseCode.Where(x => x.listtext != null && x.listtext != "" && x.id.Equals(div)).Distinct().ToList();
            var caseType = _context.TblCaseType.Where(x => x.listtext != null && x.listtext != "" && x.id.Equals(div)).Distinct().ToList();
            var caseStatus = _context.TblCaseStatus.Where(x => x.listtext != null && x.listtext != "" && x.id.Equals(div)).Distinct().ToList();
            var caseAssignedTo = _context.TblEmployees.Where(x => x.Riding.Equals(div)).ToList();
			var signed = "";
			if (_context.SignatureCapture.Any(x => x.Case_id.Equals(id)))
			{
				signed = _context.SignatureCapture.FirstOrDefault(x => x.Case_id.Equals(id)).SignatureData.ToString();

			} else {
				
			}

	
	        caseInfo.casecode = caseCode;
            caseInfo.casedetails = clientCase;
            caseInfo.casetype = caseType;
            caseInfo.casestatus = caseStatus;
            caseInfo.caseassignedto = caseAssignedTo;
			caseInfo.signed = signed;

			if (clientCase == null)
            {
                return NotFound();
            }

            return new ObjectResult(caseInfo);
        }

        // GET: api/case/dropdowns
        // getting all the cases with client details by case id
        [HttpGet("dropdowns"), Authorize]
        public IActionResult GetCaseDropdowns()
        {

            var user = JwtHelper.GetUser(HttpContext.User.Claims);
            var div = _context.TblEmployees.FirstOrDefault(empl => empl.EmployeeLogin.Equals(user)).Riding;

            var caseInfo = new CaseInfo();

            var caseCode = _context.TblCaseCode.Where(x => x.listtext != null && x.listtext != "" && x.id.Equals(div)).Distinct().ToList();
            var caseType = _context.TblCaseType.Where(x => x.listtext != null && x.listtext != "" && x.id.Equals(div)).Distinct().ToList();
            var caseStatus = _context.TblCaseStatus.Where(x => x.listtext != null && x.listtext != "" && x.id.Equals(div)).Distinct().ToList();
            var caseAssignedTo = _context.TblEmployees.Where(x => x.Riding.Equals(div)).ToList();

            caseInfo.casecode = caseCode;
            caseInfo.casetype = caseType;
            caseInfo.casestatus = caseStatus;
            caseInfo.caseassignedto = caseAssignedTo;

            return new ObjectResult(caseInfo);
        }

        // DELETE: api/case/5  -- Soft Delete
        [HttpDelete("{id}"), Authorize]
        public async Task<IActionResult> DeleteTblCase([FromRoute] int? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tblCase = await _context.TblCase.SingleOrDefaultAsync(m => m.Caseid == id);
            if (tblCase == null)
            {
                return NotFound();
            }
            // Soft delete by changing value
            tblCase.Deleted = "true";
            await _context.SaveChangesAsync();

            return Ok(tblCase);
        }

		[HttpPost("signature"), Authorize]
		public async Task<IActionResult> AddSignatureAsync([FromBody]SignatureCapture sign)
		{
			try
			{
				_context.SignatureCapture.Add(sign);
				await _context.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				throw ex;
			}

			return Ok();
		}
		// PUT: api/Case/5
		[HttpPut("{id}"), Authorize]
        public async Task<IActionResult> PutTblCase([FromRoute] int? id, [FromBody] TblCase tblCase)
        {
            tblCase.Createdby = JwtHelper.GetUser(HttpContext.User.Claims).ToUpper();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != tblCase.Caseid)
            {
                return BadRequest();
            }

            _context.Entry(tblCase).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TblCaseExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Case
        [HttpPost, Authorize]
        public async Task<IActionResult> PostTblCase([FromBody] TblCase tblCase)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            tblCase.Createdby = JwtHelper.GetUser(HttpContext.User.Claims).ToUpper();
            tblCase.Userid = JwtHelper.GetUser(HttpContext.User.Claims).ToUpper();

            _context.TblCase.Add(tblCase);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTblCase", new { id = tblCase.Caseid }, tblCase);
        }

        private bool TblCaseExists(int? id)
        {
            return _context.TblCase.Any(e => e.Caseid == id);
        }

    }
}