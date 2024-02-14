﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ooadproject.Data;
using ooadproject.Models;
using static ooadproject.Models.StudentCourseManager;

namespace ooadproject.Controllers
{
    public class CourseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Person> _userManager;
        private readonly StudentCourseManager _courseManager;
        private readonly GradesManager _gradesManager;

        public CourseController(ApplicationDbContext context, UserManager<Person> userManager)
        {
            _context = context;
            _userManager = userManager;
            _courseManager = new StudentCourseManager(_context);
            _gradesManager = new GradesManager(_context);
        }

        public List<SelectListItem> GetTeacherNamesList()
        {
            return _context.Teacher
                .Select(item => new SelectListItem() { Text = $"{item.Title} {item.FirstName} {item.LastName}", Value = item.Id.ToString() })
                .ToList();
        }

        [Authorize(Roles = "StudentService")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Course.Include(c => c.Teacher);
            return View(await applicationDbContext.ToListAsync());
        }


        [Authorize(Roles = "StudentService")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Course == null)
            {
                return NotFound();
            }

            var course = await _context.Course
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        [Authorize(Roles = "StudentService")]
        public IActionResult Create()
        {
            // putting teacher names in list to display on Create form
            ViewData["TeacherID"] = new SelectList(GetTeacherNamesList(), "Value", "Text");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,TeacherID,AcademicYear,ECTS,Semester")] Course course)
        {
            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TeacherID"] = new SelectList(_context.Teacher, "Id", "Id", course.TeacherID);
            return View(course);
        }

        [Authorize(Roles = "StudentService")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Course == null)
            {
                return NotFound();
            }

            var course = await _context.Course.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            ViewData["TeacherID"] = new SelectList(GetTeacherNamesList(), "Value", "Text");
            return View(course);
        }

        [Authorize(Roles = "StudentService")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,TeacherID,AcademicYear,ECTS,Semester")] Course course)
        {
            if (id != course.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["TeacherID"] = new SelectList(_context.Teacher, "Id", "Id", course.TeacherID);
            return View(course);
        }
        [Authorize(Roles = "StudentService")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Course == null)
            {
                return NotFound();
            }

            var course = await _context.Course
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        [Authorize(Roles = "StudentService")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Course == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Course'  is null.");
            }
            var course = await _context.Course.FindAsync(id);
            if (course != null)
            {
                _context.Course.Remove(course);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CourseExists(int id)
        {
          return (_context.Course?.Any(e => e.ID == id)).GetValueOrDefault();
        }
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> CourseStatus(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var course = await _context.Course.FindAsync(id);

            if (course == null || course.Teacher.Id != user.Id)
            {
                return NotFound();
            }

            var studentCourses = await _context.StudentCourse
                .CountAsync(sc => sc.CourseID == id);

            var list = await _courseManager.RetrieveStudentCourseInfo(id);

            ViewData["course"] = course;
            ViewData["Info"] = list;
            ViewData["Maximum"] = await _courseManager.GetMaximumPoints(id);
            ViewData["NumberOfPassed"] = _courseManager.GetNumberOfPassed(list);
            ViewData["NumberOfStudents"] = studentCourses;
            ViewData["Courses"] = await _context.Course
                .Where(c => c.Teacher.Id == user.Id)
                .ToListAsync();

            return View();
        }



    }
}
