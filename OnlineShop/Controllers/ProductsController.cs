﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Models.Db;

namespace OnlineShop.Controllers
{
    public class ProductsController : Controller
    {
        private readonly OnlineShopContext _context;

        public ProductsController(OnlineShopContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            return View(await _context.Products.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,FullDesc,Price,ImageName,Qty,Tag,VideoUrl")] Product product,IFormFile? MainImage, IFormFile[]? GalleryImages)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                // Nếu có hình ảnh trong GalleryImages
                if (GalleryImages != null)
                {
                    // Duyệt qua từng hình ảnh trong GalleryImages
                    foreach (var item in GalleryImages)
                    {
                        // Tạo một đối tượng mới để lưu thông tin về hình ảnh
                        var newgallery = new ProductGalery();
                        newgallery.ProductId = product.Id; // Gán ID sản phẩm

                        // Tạo tên file hình ảnh mới, kết hợp GUID và phần mở rộng
                        newgallery.ImageName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(item.FileName);

                        // Lấy đường dẫn thư mục hiện tại
                        string fn = Directory.GetCurrentDirectory();

                        // Tạo đường dẫn đầy đủ để lưu hình ảnh
                        string ImagePath = fn + "\\wwwroot\\images\\banners\\" + newgallery.ImageName;

                        // Sử dụng FileStream để lưu hình ảnh vào đường dẫn đã tạo
                        using (var stream = new FileStream(ImagePath, FileMode.Create))
                        {
                            item.CopyTo(stream); // Sao chép dữ liệu hình ảnh vào stream
                        }

                        // Thêm thông tin về hình ảnh mới vào cơ sở dữ liệu
                        _context.ProductGaleries.Add(newgallery);
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,FullDesc,Price,ImageName,Qty,Tag,VideoUrl")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
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
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
