using AutoMapper;
using Leave_management.Contract;
using Leave_management.Data;
using Leave_management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Leave_management.Controllers
{
    [Authorize ]
    public class LeaveRequestsController : Controller
    {

            private readonly ILeaveRequestsRepository _LeaveRequestsrepo;
        private readonly ILeaveTypeRepository _LeaveTyperepo;
        private readonly ILeaveAllocationRepository _leaveallocationrepo;
        private readonly IMapper _mapper;
            private readonly UserManager<Employee> _userManager;

            public LeaveRequestsController(
                ILeaveRequestsRepository LeaveRequestsrepo,
                ILeaveTypeRepository LeaveTyperepo,
                ILeaveAllocationRepository leaveallocationrepo,
                IMapper mapper,
                UserManager<Employee> userManager)
            {
                _LeaveRequestsrepo = LeaveRequestsrepo;
            _LeaveTyperepo = LeaveTyperepo;
            _leaveallocationrepo = leaveallocationrepo;
            _mapper = mapper;
                _userManager = userManager;
            }
        [Authorize(Roles = "Administrator")]
        // GET: LeaveRequestsController
        public async Task<ActionResult> Index()
        {
            var leaveRequests =await _LeaveRequestsrepo.FindAll();
            var LeaveRequestsModel = _mapper.Map<List<LeaveRequestsVM>>(leaveRequests);
            var model = new AdminLeaveRequestsViewVM
            {
                TotalRequests = LeaveRequestsModel.Count,
                ApprovedRequests = LeaveRequestsModel.Count(q => q.Approved == true),
                PendingRequests = LeaveRequestsModel.Count(q => q.Approved == null),
                RejectedRequests = LeaveRequestsModel.Count(q => q.Approved == false),
                LeaveRequests = LeaveRequestsModel
            };
            return View(model);
        }

        // GET: LeaveRequestsController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var leaveRequest =await _LeaveRequestsrepo.FindById(id);
            var model = _mapper.Map<LeaveRequestsVM>(leaveRequest);
            return View(model);
        }

        public async Task<ActionResult> ApproveRequest (int id)
        {

            try
            {
                var user =await _userManager.GetUserAsync(User);
                var leaveRequest =await _LeaveRequestsrepo.FindById(id);
                leaveRequest.Approved = true;
                leaveRequest.ApprovedById = user.Id;
                leaveRequest.DateActioned = DateTime.Now;

                var isSuccess = _LeaveRequestsrepo.Update(leaveRequest);

                return RedirectToAction(nameof(Index));
                
            }
            catch (Exception ex)
            {

                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<ActionResult> RejectRequest(int id)
        {
            try
            {
                var user =await _userManager.GetUserAsync(User);
                var leaveRequest =await _LeaveRequestsrepo.FindById(id);
                var employeeid = leaveRequest.RequestingEmployeeId;
                var leaveTypeId = leaveRequest.LeaveTypeId;
                var allocation = await _leaveallocationrepo.GetLeaveAllocationsByEmployeeAndType(employeeid, leaveTypeId);
                int daysRequested = (int)(leaveRequest.EndDate - leaveRequest.StartDate).TotalDays;
                allocation.NumberOfDays -= daysRequested;

                leaveRequest.Approved = false;
                leaveRequest.ApprovedById = user.Id;
                leaveRequest.DateActioned = DateTime.Now;

                 await _LeaveRequestsrepo.Update(leaveRequest);
                 await _leaveallocationrepo.Update(allocation);
                return RedirectToAction(nameof(Index));

            }
            catch (Exception ex)
            {

                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<ActionResult> MyLeave()
        {
            var employee =await _userManager.GetUserAsync(User);
            var employeeid = employee.Id;

            var employeeallocations =await _leaveallocationrepo.GetLeaveAllocationsByEmployee(employeeid);
            var employeerequests =await _LeaveRequestsrepo.GetLeaveRequestsByEmployee(employeeid);

            var employeeAllocationModel = _mapper.Map<List<LeaveAllocationVM>>(employeeallocations);
            var employeeRequestsModel = _mapper.Map<List<LeaveRequestsVM>>(employeerequests);

            var model = new EmployeeLeaveRequestViewVM
            {
                LeaveAllocations = employeeAllocationModel,
                LeaveRequests = employeeRequestsModel
            };
            return View(model);

        }

        public async Task<ActionResult> CancelRequest(int id)
        {
            var leaveRequest =await _LeaveRequestsrepo.FindById(id);
            leaveRequest.Cancelled = true;
            await _LeaveRequestsrepo.Update(leaveRequest);
            return RedirectToAction("MyLeave");
        } 

        // GET: LeaveRequestsController/Create
        public async Task<ActionResult> Create()
        {
            var leaveTypes = await _LeaveTyperepo.FindAll();
            var leaveTypesitems = leaveTypes.Select(q => new SelectListItem
            {
                Text = q.Name,
                Value = q.Id.ToString()
            });
            var model = new CreateLeaveRequestVM
            {
                LeaveTypes = leaveTypesitems
            };
            return View(model);
        }

        // POST: LeaveRequestsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateLeaveRequestVM model)
        {
            try
            {
                var startDate = Convert.ToDateTime(model.StartDate);
                var endDate = Convert.ToDateTime(model.EndDate);
                var leaveTypes =await _LeaveTyperepo.FindAll();
                var leaveTypesitems = leaveTypes.Select(q => new SelectListItem
                {
                    Text = q.Name,
                    Value = q.Id.ToString()
                });
                model.LeaveTypes = leaveTypesitems;
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                if (DateTime.Compare(startDate, endDate) > 1) 
                {
                    ModelState.AddModelError("", "Start date cannot be futher in the future than the end date");
                    return View(model);
                }

                var employee =await _userManager.GetUserAsync(User);
                var allocation =await _leaveallocationrepo.GetLeaveAllocationsByEmployeeAndType(employee.Id, model.LeaveTypeId);
                int daysRequested = (int)(endDate - startDate).TotalDays;

                if(daysRequested > allocation.NumberOfDays)
                {
                    ModelState.AddModelError("", "You do not have sufficient days");
                    return View(model);
                }

                var leaveRequestModel = new LeaveRequestsVM
                {
                    RequestingEmployeeId = employee.Id,
                    StartDate = startDate,
                    EndDate = endDate,
                    Approved = null,
                    DateRequested = DateTime.Now,
                    DateActioned = DateTime.Now,
                    LeaveTypeId = model.LeaveTypeId,
                    RequestComments = model.RequestComments
                };

                var leaveRequest = _mapper.Map<LeaveRequests>(leaveRequestModel);
                var isSuccess =await _LeaveRequestsrepo.Create(leaveRequest);

                if (!isSuccess)
                {
                    ModelState.AddModelError("", "Something went wrong");
                    return View(model);
                }
                return RedirectToAction("MyLeave");
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", "Something went wrong");
                return View(model);
            }
        }

        // GET: LeaveRequestsController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: LeaveRequestsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: LeaveRequestsController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LeaveRequestsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
