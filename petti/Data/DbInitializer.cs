using Microsoft.EntityFrameworkCore;
using petti.Models;

namespace petti.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // 1. التأكد من تطبيق كل الـ Migrations
            await context.Database.MigrateAsync();

            // =========================================================================
            // كود التحديث التلقائي: لتعديل Pet Sitting إلى Nail Trim فوراً في الداتابيز
            // =========================================================================
            var oldSittingService = await context.Services
                .Include(s => s.Category)
                .FirstOrDefaultAsync(s => s.Name == "Pet Sitting");

            if (oldSittingService != null)
            {
                oldSittingService.Name = "Nail Trim & Paw Care";
                oldSittingService.Description = "Stress-free nail clipping with precision safety guards, smoothing, and organic paw balm massage.";
                oldSittingService.Price = 8.00m;
                oldSittingService.DurationMinutes = 25;
                oldSittingService.TargetPetType = "All";

                if (oldSittingService.Category != null)
                {
                    oldSittingService.Category.Name = "Nail & Paw Care";
                    oldSittingService.Category.IconClass = "fa-paw";
                    oldSittingService.Category.Description = "At-home nail trimming and paw pad hygiene care";
                }

                await context.SaveChangesAsync();
            }

            // =========================================================================
            // إضافة آراء العملاء الحقيقية (Testimonials) بخصائص الموديل الصحيحة
            // =========================================================================
            if (!await context.Testimonials.AnyAsync())
            {
                var testimonials = new List<Testimonial>
                {
                    new Testimonial
                    {
                        ClientName = "Lina H.",
                        PetInfo = "Owner of Max, Wire Fox Terrier",
                        Feedback = "The groomer arrived on time and my terrier, who hates the car, was completely relaxed the whole visit. Booking took under two minutes.",
                        Rating = 5,
                        ClientImageUrl = "https://images.unsplash.com/photo-1544005313-94ddf0286df2?q=80&w=150&auto=format&fit=crop",
                        IsFeatured = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Testimonial
                    {
                        ClientName = "Omar Q.",
                        PetInfo = "Owner of Luna, Persian Cat",
                        Feedback = "I ordered food and booked a bath for the same afternoon. The sitter even sent photo updates — my cat has never looked this fluffy.",
                        Rating = 5,
                        ClientImageUrl = "https://images.unsplash.com/photo-1502685104226-ee32379fefbe?q=80&w=150&auto=format&fit=crop",
                        IsFeatured = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Testimonial
                    {
                        ClientName = "Rana S.",
                        PetInfo = "Owner of Bear, Golden Retriever",
                        Feedback = "Fixed pricing sold me instantly. No haggling at the door, and the caregiver's ID badge matched the app before I let them in.",
                        Rating = 5,
                        ClientImageUrl = "https://images.unsplash.com/photo-1531123897727-8f129e1688ce?q=80&w=150&auto=format&fit=crop",
                        IsFeatured = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                await context.Testimonials.AddRangeAsync(testimonials);
                await context.SaveChangesAsync();
            }

            // إذا كانت الفئات موجودة مسبقاً نتوقف هنا
            if (await context.Categories.AnyAsync()) return;

            // =========================================================================
            // 2. إضافة الفئات (في حال كانت الداتابيز جديدة)
            // =========================================================================
            var catFood = new Category { Name = "Food", Type = "Product", IconClass = "fa-bone", Description = "Nutritious pet food and treats" };
            var catToys = new Category { Name = "Toys", Type = "Product", IconClass = "fa-baseball", Description = "Enrichment toys and bundles" };
            var catGroomingProd = new Category { Name = "Grooming", Type = "Product", IconClass = "fa-scissors", Description = "Grooming brushes and gear" };
            var catHealth = new Category { Name = "Health", Type = "Product", IconClass = "fa-kit-medical", Description = "Supplements and hygiene items" };
            var catBathingProd = new Category { Name = "Bathing", Type = "Product", IconClass = "fa-shower", Description = "Shampoos and conditioners" };

            var catGroomingServ = new Category { Name = "Home Grooming", Type = "Service", IconClass = "fa-scissors", Description = "At-home pet grooming visits" };
            var catBathingServ = new Category { Name = "Bathing", Type = "Service", IconClass = "fa-shower", Description = "At-home pet washing visits" };
            var catNailServ = new Category { Name = "Nail & Paw Care", Type = "Service", IconClass = "fa-paw", Description = "At-home nail trimming and paw pad hygiene care" };

            await context.Categories.AddRangeAsync(
                catFood, catToys, catGroomingProd, catHealth, catBathingProd,
                catGroomingServ, catBathingServ, catNailServ
            );
            await context.SaveChangesAsync();

            // =========================================================================
            // 3. إضافة الخدمات المنزلية
            // =========================================================================
            var s1 = new Service
            {
                Name = "Home Grooming",
                Description = "Full coat trim, styling and de-shedding treatment, tailored to your pet's breed.",
                Price = 18.00m,
                DurationMinutes = 60,
                TargetPetType = "Dog",
                CategoryId = catGroomingServ.CategoryId,
                IsActive = true
            };

            var s2 = new Service
            {
                Name = "Bathing",
                Description = "Gentle wash with hypoallergenic shampoo, blow-dry, and paw balm finish.",
                Price = 12.00m,
                DurationMinutes = 30,
                TargetPetType = "All",
                CategoryId = catBathingServ.CategoryId,
                IsActive = true
            };

            var s3 = new Service
            {
                Name = "Nail Trim & Paw Care",
                Description = "Stress-free nail clipping with precision safety guards, smoothing, and organic paw balm massage.",
                Price = 8.00m,
                DurationMinutes = 25,
                TargetPetType = "All",
                CategoryId = catNailServ.CategoryId,
                IsActive = true
            };

            await context.Services.AddRangeAsync(s1, s2, s3);
            await context.SaveChangesAsync();

            // =========================================================================
            // 4. إضافة المنتجات
            // =========================================================================
            var products = new List<Product>
            {
                new Product
                {
                    Name = "Grain-Free Chicken Kibble 3kg",
                    Description = "Nutrient-rich dry food supporting lean muscles and optimal digestion.",
                    Price = 14.50m,
                    StockQuantity = 25,
                    TargetPetType = "Dog",
                    CategoryId = catFood.CategoryId,
                    IsActive = true,
                    Images = new List<ProductImage> { new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1589924691995-400dc9ecc119?q=80&w=500&auto=format&fit=crop" } }
                },
                new Product
                {
                    Name = "Rope & Rubber Chew Bundle",
                    Description = "Durable chew toys designed for dental health and daily fetch games.",
                    Price = 6.90m,
                    StockQuantity = 40,
                    TargetPetType = "Dog",
                    CategoryId = catToys.CategoryId,
                    IsActive = true,
                    Images = new List<ProductImage> { new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1591946614720-90a587da4a36?q=80&w=500&auto=format&fit=crop" } }
                },
                new Product
                {
                    Name = "De-Shedding Brush Pro",
                    Description = "Gently removes undercoat hair without scratching sensitive skin.",
                    Price = 9.90m,
                    StockQuantity = 15,
                    TargetPetType = "All",
                    CategoryId = catGroomingProd.CategoryId,
                    IsActive = true,
                    Images = new List<ProductImage> { new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1601758064135-c3b0ce7e8f39?q=80&w=500&auto=format&fit=crop" } }
                },
                new Product
                {
                    Name = "Joint & Coat Supplement Chews",
                    Description = "Glucosamine and Omega-3 soft chews to promote hip agility and coat shine.",
                    Price = 16.00m,
                    StockQuantity = 30,
                    TargetPetType = "Dog",
                    CategoryId = catHealth.CategoryId,
                    IsActive = true,
                    Images = new List<ProductImage> { new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1583511655857-d19b40a7a54e?q=80&w=500&auto=format&fit=crop" } }
                },
                new Product
                {
                    Name = "Oatmeal Shampoo & Conditioner Set",
                    Description = "Calming organic formula providing relief for dry and itchy skin.",
                    Price = 11.50m,
                    StockQuantity = 20,
                    TargetPetType = "All",
                    CategoryId = catBathingProd.CategoryId,
                    IsActive = true,
                    Images = new List<ProductImage> { new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1583512603805-3cc6b41f3edb?q=80&w=500&auto=format&fit=crop" } }
                },
                new Product
                {
                    Name = "Puzzle Treat Dispenser",
                    Description = "Stimulates mental problem-solving and slows down fast eaters.",
                    Price = 13.20m,
                    StockQuantity = 18,
                    TargetPetType = "All",
                    CategoryId = catToys.CategoryId,
                    IsActive = true,
                    Images = new List<ProductImage> { new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1601758124277-f0086701be34?q=80&w=500&auto=format&fit=crop" } }
                },
                new Product
                {
                    Name = "Wet Food Variety Pack (12x)",
                    Description = "Succulent shreds in gravy featuring chicken, salmon, and turkey recipes.",
                    Price = 10.80m,
                    StockQuantity = 35,
                    TargetPetType = "Cat",
                    CategoryId = catFood.CategoryId,
                    IsActive = true,
                    Images = new List<ProductImage> { new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1601758176689-3e6b3b3ba2b0?q=80&w=500&auto=format&fit=crop" } }
                },
                new Product
                {
                    Name = "Nail Trimmer with Safety Guard",
                    Description = "Ergonomic stainless steel clippers preventing over-cutting nails.",
                    Price = 7.40m,
                    StockQuantity = 50,
                    TargetPetType = "All",
                    CategoryId = catHealth.CategoryId,
                    IsActive = true,
                    Images = new List<ProductImage> { new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1620000617641-6d4bb2416f18?q=80&w=500&auto=format&fit=crop" } }
                }
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }
    }
}