﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TuyenDungFPT.Models;
using SlugGenerator;
using Microsoft.AspNetCore.Authorization;

namespace TuyenDungFPT.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = "Admin")]
	public class JobCategoriesController : Controller
	{
		private readonly TuyenDungFPTDbContext _context;

		public JobCategoriesController(TuyenDungFPTDbContext context)
		{
			_context = context;
		}

		// GET: JobCategories
		public async Task<IActionResult> Index()
		{
			return View(await _context.JobCategories.ToListAsync());
		}

		// GET: JobCategories/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var jobCategory = await _context.JobCategories
				.FirstOrDefaultAsync(m => m.Id == id);
			if (jobCategory == null)
			{
				return NotFound();
			}

			return View(jobCategory);
		}

		// GET: JobCategories/Create
		public IActionResult Create()
		{
			return View();
		}

		// POST: JobCategories/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Id,Name,Name_khongdau")] JobCategory jobCategory)
		{
			if (ModelState.IsValid)
			{
				if (string.IsNullOrWhiteSpace(jobCategory.Name_khongdau))
				{
					jobCategory.Name_khongdau = jobCategory.Name.GenerateSlug();
				}
				_context.Add(jobCategory);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			return View(jobCategory);
		}

		// GET: JobCategories/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var jobCategory = await _context.JobCategories.FindAsync(id);
			if (jobCategory == null)
			{
				return NotFound();
			}
			return View(jobCategory);
		}

		// POST: JobCategories/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Name_khongdau")] JobCategory jobCategory)
		{
			if (id != jobCategory.Id)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					if (string.IsNullOrWhiteSpace(jobCategory.Name_khongdau))
					{
						jobCategory.Name_khongdau = jobCategory.Name.GenerateSlug();
					}
					_context.Update(jobCategory);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!JobCategoryExists(jobCategory.Id))
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
			return View(jobCategory);
		}

		// GET: JobCategories/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var jobCategory = await _context.JobCategories
				.FirstOrDefaultAsync(m => m.Id == id);
			if (jobCategory == null)
			{
				return NotFound();
			}

			return View(jobCategory);
		}

		// POST: JobCategories/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var jobCategory = await _context.JobCategories.FindAsync(id);
			if (jobCategory != null)
			{
				_context.JobCategories.Remove(jobCategory);
			}

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool JobCategoryExists(int id)
		{
			return _context.JobCategories.Any(e => e.Id == id);
		}
	}
}
