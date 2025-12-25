using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using HanaHotel.DataAccessLayer.Concrete;
using HanaHotel.EntityLayer.Concrete;

public static class DataInitializer
{
	public static async Task TestDataAsync(IApplicationBuilder applicationBuilder)
	{
		using var scope = applicationBuilder.ApplicationServices.CreateScope();
		var context = scope.ServiceProvider.GetRequiredService<DataContext>();
		var userManager = scope.ServiceProvider.GetService<UserManager<User>>();

		if (context == null)
			return;

		// Apply migrations
		//if (context.Database.GetPendingMigrations().Any())
		//	context.Database.Migrate();

		// ABOUT
		if (!context.Abouts.Any())
		{
			context.Abouts.Add(new About
			{
				Title1 = "Chào mừng đến với khách sạn Hana Hotel",
				Title2 = "Trải nghiệm nghỉ dưỡng đẳng cấp",
				Content = "Hana Hotel tọa lạc ngay trung tâm thành phố, mang đến trải nghiệm sang trọng, tiện nghi hiện đại và dịch vụ tận tâm. Dù bạn đi công tác hay du lịch, chúng tôi luôn sẵn sàng mang đến kỳ nghỉ thoải mái và đáng nhớ.",
				RoomCount = 250,
				StaffCount = 150,
				CustomerCount = 5000
			});
			await context.SaveChangesAsync();
		}

		// ROLES
		//if (!context.Roles.Any())
		//{
		//	var roles = new[]
		//	{
		//		new Role { Name = "Admin" },
		//		new Role { Name = "Manager" },
		//		new Role { Name = "Staff" },
		//		new Role { Name = "Customer" }
		//	};
		//	context.Roles.AddRange(roles);
		//	await context.SaveChangesAsync();
		//}

		// ROOMS
		if (!context.Rooms.Any())
		{
			var rooms = new List<Room>
			{
				new Room { RoomName = "Phòng Tiêu Chuẩn", Description = "Phòng tiêu chuẩn tiện nghi, phù hợp cho nghỉ ngơi.", Size = 25, Price = 1500000, BedCount = 1 },
				new Room { RoomName = "Phòng Cao Cấp", Description = "Phòng rộng rãi với view đẹp.", Size = 30, Price = 2000000, BedCount = 1 },
				new Room { RoomName = "Suite", Description = "Suite sang trọng với khu vực tiếp khách riêng.", Size = 55, Price = 3000000, BedCount = 2 },
				new Room { RoomName = "Phòng Gia Đình", Description = "Không gian rộng cho gia đình, nhiều giường.", Size = 40, Price = 2500000, BedCount = 3 },
				new Room { RoomName = "Phòng Doanh Nhân", Description = "Phòng phù hợp cho khách công tác.", Size = 28, Price = 1800000, BedCount = 1 },
				new Room { RoomName = "Phòng Hạng Sang", Description = "Nội thất cao cấp, không gian tinh tế.", Size = 35, Price = 2800000, BedCount = 1 },
				new Room { RoomName = "Penthouse", Description = "Căn hộ sang trọng nhất với view toàn cảnh.", Size = 120, Price = 12000000, BedCount = 2 }
			};
			context.Rooms.AddRange(rooms);
			await context.SaveChangesAsync();
		}

		// SERVICES
		if (!context.Services.Any())
		{
			var services = new List<Service>
			{
				new Service { ServiceName = "Wi-Fi miễn phí", Price = 0, Unit = "lượt", Description = "Internet tốc độ cao miễn phí.", ServiceIcon = "fa fa-wifi" },
				new Service { ServiceName = "Phòng gym", Price = 0, Unit = "lượt", Description = "Phòng tập hiện đại và đầy đủ thiết bị.", ServiceIcon = "fa fa-dumbbell" },
				new Service { ServiceName = "Hồ bơi", Price = 0, Unit = "lượt", Description = "Hồ bơi ngoài trời thoáng mát.", ServiceIcon = "fa fa-swimmer" },
				new Service { ServiceName = "Dịch vụ Spa", Price = 0, Unit = "dịch vụ", Description = "Trị liệu thư giãn chuyên nghiệp.", ServiceIcon = "fa fa-spa" },
				new Service { ServiceName = "Dịch vụ phòng", Price = 0, Unit = "lần", Description = "Đặt món và phục vụ tận phòng.", ServiceIcon = "fa fa-utensils" },
				new Service { ServiceName = "Đưa đón sân bay", Price = 300000, Unit = "chuyến", Description = "Xe đưa đón sân bay tiện lợi.", ServiceIcon = "fa fa-shuttle-van" }
			};
			context.Services.AddRange(services);
			await context.SaveChangesAsync();
		}

		// MESSAGE CATEGORIES
		if (!context.MessageCategories.Any())
		{
			context.MessageCategories.AddRange(
				new MessageCategory { MessageCategoryName = "Yêu cầu chung" },
				new MessageCategory { MessageCategoryName = "Đặt phòng" },
				new MessageCategory { MessageCategoryName = "Góp ý" },
				new MessageCategory { MessageCategoryName = "Phàn nàn" },
				new MessageCategory { MessageCategoryName = "Yêu cầu dịch vụ" }
			);
			await context.SaveChangesAsync();
		}

		// CONTACTS
		if (!context.Contacts.Any())
		{
			context.Contacts.AddRange(
				new Contact
				{
					Name = "Nguyễn Thị Hoa",
					Email = "hoa.nguyen@gmail.com",
					Subject = "Hỏi về tình trạng phòng",
					Message = "Cho tôi hỏi khách sạn còn phòng đôi vào cuối tuần không?",
					Date = DateTime.Now.AddDays(-2),
					MessageCategoryId = context.MessageCategories.First().MessageCategoryId
				},
				new Contact
				{
					Name = "Trần Minh Quang",
					Email = "quang.tran@gmail.com",
					Subject = "Phản hồi về kỳ nghỉ",
					Message = "Kỳ nghỉ vừa rồi rất tuyệt, cảm ơn khách sạn!",
					Date = DateTime.Now.AddDays(-5),
					MessageCategoryId = context.MessageCategories.Skip(1).First().MessageCategoryId
				}
			);
			await context.SaveChangesAsync();
		}

		// GUESTS
		if (!context.Guests.Any())
		{
			context.Guests.AddRange(
				new Guest { Name = "An", Surname = "Nguyễn", City = "Hà Nội" },
				new Guest { Name = "Bình", Surname = "Trần", City = "Đà Nẵng" },
				new Guest { Name = "Hương", Surname = "Lê", City = "Hồ Chí Minh" }
			);
			await context.SaveChangesAsync();
		}

		// USERS (Identity + tiếng Việt)
		List<User> createdUsers = new();
		if (userManager != null && !context.Users.Any())
		if(true)
		{
			var adminRole = context.Roles.FirstOrDefault(r => r.Name == "Admin");
			var customerRole = context.Roles.FirstOrDefault(r => r.Name == "Customer");
			var managerRole = context.Roles.FirstOrDefault(r => r.Name == "Manager");

			var usersToCreate = new[]
			{
				new User
				{
					Name = "Quản trị hệ thống",
					DateOfBirth = new DateTime(1990,1,1),
					Gender = GenderType.Male,
					Address = "Hà Nội",
					PhoneNumber = "0912345678",
					Email = "admin@hanahotel.com",
					UserName = "admin",
					RoleId = adminRole?.Id ?? 0
				},
				new User
				{
					Name = "Lê Nhật Minh",
					DateOfBirth = new DateTime(1995,5,3),
					Gender = GenderType.Male,
					Address = "Hồ Chí Minh",
					PhoneNumber = "0987654321",
					Email = "minh.le@gmail.com",
					UserName = "minhle",
					RoleId = customerRole?.Id ?? 0
				},
				new User
				{
					Name = "Phạm Thị Linh",
					DateOfBirth = new DateTime(1998,8,10),
					Gender = GenderType.Female,
					Address = "Đà Nẵng",
					PhoneNumber = "0934567890",
					Email = "linh.pham@gmail.com",
					UserName = "linhpham",
					RoleId = customerRole?.Id ?? 0
				},
				new User
				{
					Name = "Trần Tuấn Kiệt",
					DateOfBirth = new DateTime(1988,3,15),
					Gender = GenderType.Male,
					Address = "Cần Thơ",
					PhoneNumber = "0978123456",
					Email = "kiet.tran@gmail.com",
					UserName = "kiettran",
					RoleId = managerRole?.Id ?? 0
				}
			};

			foreach (var u in usersToCreate)
			{
				var createResult = await userManager.CreateAsync(u, "Password123!");
				if (createResult.Succeeded)
				{
					createdUsers.Add(u);
				}
				else
				{
					if (!context.Users.Any(x => x.UserName == u.UserName))
					{
						context.Users.Add(u);
						createdUsers.Add(u);
					}
				}
			}

			await context.SaveChangesAsync();
		}

		// REVIEWS
		if (!context.Reviews.Any() && context.Rooms.Any() && context.Users.Any())
		{
			var roomIds = context.Rooms.Select(r => r.Id).Take(6).ToList();
			var userIds = context.Users.Select(u => u.Id).Take(6).ToList();

			var reviews = new List<Review>();
			for (int i = 0; i < Math.Min(roomIds.Count, userIds.Count); i++)
			{
				reviews.Add(new Review
				{
					UserId = userIds[i],
					RoomId = roomIds[i],
					Content = $"Đánh giá mẫu {i + 1} – rất hài lòng.",
					RatingStars = 5
				});
			}

			if (reviews.Any())
			{
				context.Reviews.AddRange(reviews);
				await context.SaveChangesAsync();
			}
		}

		// BOOKINGS
		if (!context.Bookings.Any() && context.Rooms.Any() && context.Users.Any())
		{
			var firstRoomId = context.Rooms.Select(r => r.Id).First();
			var firstUserId = context.Users.Select(u => u.Id).First();

			context.Bookings.AddRange(
				new Booking
				{
					FullName = "Nguyễn Văn Anh",
					Email = "vana@gmail.com",
					Phone = "0911222333",
					BookingDate = DateTime.Now.AddDays(-10),
					CheckInDate = DateTime.Now.AddDays(3),
					CheckOutDate = DateTime.Now.AddDays(5),
					Status = BookingStatus.Confirmed,
					AdditionalRequest = "Yêu cầu tầng cao, view đẹp",
					RoomId = firstRoomId,
					UserId = firstUserId
				},
				new Booking
				{
					FullName = "Trần Thị Bình",
					Email = "tranthib@gmail.com",
					Phone = "0944332211",
					BookingDate = DateTime.Now.AddDays(-8),
					CheckInDate = DateTime.Now.AddDays(5),
					CheckOutDate = DateTime.Now.AddDays(8),
					Status = BookingStatus.Pending,
					AdditionalRequest = "Thêm gối",
					RoomId = context.Rooms.Skip(1).Select(r => r.Id).FirstOrDefault(),
					UserId = context.Users.Skip(1).Select(u => u.Id).FirstOrDefault()
				}
			);

			await context.SaveChangesAsync();
		}
	}
}