using HanaHotel.BusinessLayer.Abstract;
using HanaHotel.BusinessLayer.Concrete;
using HanaHotel.BusinessLayer.Mapping;
using HanaHotel.DataAccessLayer.Abstract;
using HanaHotel.DataAccessLayer.Concrete;
using HanaHotel.DataAccessLayer.EntityFramework;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DataContext>();

// DataAccessLayer
builder.Services.AddScoped<IAboutDal, EfAboutDAL>();
builder.Services.AddScoped<IBookingDal, EfBookingDAL>();
builder.Services.AddScoped<IContactDal, EfContactDAL>();
builder.Services.AddScoped<IGuestDal, EfGuestDAL>();
builder.Services.AddScoped<IImageDal, EfImageDAL>();
builder.Services.AddScoped<IMessageCategoryDal, EfMessageCategoryDAL>();
builder.Services.AddScoped<IPaymentMethodDal, EfPaymentMethodDAL>();
builder.Services.AddScoped<IPromotionDal, EfPromotionDAL>();
builder.Services.AddScoped<IPromotionDetailDal, EfPromotionDetailDAL>();
builder.Services.AddScoped<IReviewDal, EfReviewDAL>();
builder.Services.AddScoped<IRoleDal, EfRoleDAL>();
builder.Services.AddScoped<IRoomDal, EfRoomDAL>();
builder.Services.AddScoped<IRoomDetailDal, EfRoomDetailDAL>();
builder.Services.AddScoped<IServiceDal, EfServiceDAL>();
builder.Services.AddScoped<IServiceDetailDal, EfServiceDetailDAL>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IUserDal, EfUserDAL>();
builder.Services.AddScoped<IHotelDal, EfHotelDAL>();
builder.Services.AddScoped<IHotelDetailDal, EfHotelDetailDAL>();



// BusinessLayer
builder.Services.AddScoped<IAboutService, AboutService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<IGuestService, GuestService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IMessageCategoryService, MessageCategoryService>();
builder.Services.AddScoped<IPaymentMethodService, PaymentMethodService>();
builder.Services.AddScoped<IPromotionService, PromotionService>();
builder.Services.AddScoped<IPromotionDetailService, PromotionDetailService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IRoomDetailService, RoomDetailService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IServiceDetailService, ServiceDetailService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IHotelService, HotelService>();
builder.Services.AddScoped<IHotelDetailService, HotelDetailService>();

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

builder.Services.AddCors(opt =>
{
	opt.AddPolicy("HanaHotelApiCors", opts =>
	{
		opts.AllowAnyOrigin()
			.AllowAnyHeader()
			.AllowAnyMethod();
	});
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
	options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
});

// IMPORTANT: register a distributed cache implementation before AddSession.
// The session middleware depends on IDistributedCache (DistributedSessionStore).
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromDays(1); // session expires after 1 day of inactivity
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor(); // để controller có thể truy cập session

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(PromotionProfile).Assembly);

var app = builder.Build();

// Gọi DataInitializer để tạo dữ liệu mẫu
await DataInitializer.TestDataAsync(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("HanaHotelApiCors");

app.UseSession();

app.UseAuthorization();

app.MapControllers();

//app.Run();

//using (var scope = app.Services.CreateScope())
//{
//    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

//    string[] roleNames = { "Admin", "User", "Manager", "Customer" };
//    foreach (var roleName in roleNames)
//    {
//        if (!await roleManager.RoleExistsAsync(roleName))
//        {
//            await roleManager.CreateAsync(new IdentityRole(roleName));
//        }
//    }
//}


app.Run();